using Shouldly;

using System;

namespace ProphetsWay.Example.Tests
{
	public abstract class BaseUnitTests<T> : IDisposable
	{
		protected T _da;

		public BaseUnitTests()
		{
			_da = GetIExampleDataAccess;
		}

		protected abstract T GetIExampleDataAccess { get; }

		/// <summary>
		/// <c>IBaseDataAccess</c> extends <see cref="IDisposable"/>, so the Data Access Layer this class
		/// constructs is torn down by xUnit after every test. Most subclasses close <typeparamref name="T"/>
		/// over a Dao interface rather than the aggregate, which is why the disposal goes through a cast
		/// rather than a generic constraint.
		/// </summary>
		public void Dispose()
		{
			(_da as IDisposable)?.Dispose();
		}
	}

	/// <summary>
	/// Which test classes may not run at the same time as which others.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every implementation in this repository writes to one process-wide store, so two test classes that touch
	/// the same entity type are two threads writing to the same table. Any assertion phrased over the whole set
	/// - a <c>GetCount</c>, a <c>GetAll().Count</c>, a page index - is then racing whatever the other class is
	/// inserting, and fails intermittently and only when run alongside its neighbour.
	/// </para>
	/// <para>
	/// xUnit runs collections in parallel and the classes within one collection in sequence, so the grouping has
	/// to follow the entity types the classes share. A class touching two entity types pulls both into the same
	/// collection, which is why the list below is one large group rather than one per entity:
	/// <c>BaseDataAccessTests</c> touches Company, Job, User, Transaction and Resource together, and
	/// <c>DepartmentDataAccessTests</c> inserts a User alongside its Departments.
	/// <see cref="DataAccess.Entities.CompanyResource"/> shares no entity type with any of them - its company
	/// identifiers are synthetic and no Company row is ever created - so it stands alone and still runs in
	/// parallel.
	/// </para>
	/// </remarks>
	public static class TestCollections
	{
		public const string CoreEntities = "DataStore - Company, Job, User, Transaction, Resource, Department";

		public const string CompanyResources = "DataStore - CompanyResource";
	}

	/// <summary>
	/// The wall clock read either side of a single call, so a timestamp written during that call can be bounded
	/// to a window no wider than the call itself.
	/// </summary>
	/// <remarks>
	/// Rule 18 on <see cref="DataAccess.IDaos.IDepartmentDao"/> names <see cref="DateTime.UtcNow"/> as the
	/// clock, which is what makes a tight window assertable at all. A Data Access Layer that hardcoded a stamp,
	/// or read the local clock, falls outside every window one of these produces - and carries the wrong
	/// <see cref="DateTime.Kind"/> besides.
	/// </remarks>
	public sealed class StampWindow
	{
		/// <summary>
		/// Resolution slack, and nothing else. <see cref="DateTime.UtcNow"/> on net48 advances in steps of
		/// roughly 15.6ms, and a Data Access Layer may round a stamp to the precision of whatever it stores it
		/// in, so a stamp taken inside the call can read a step either side of the bounds. This is not a sanity
		/// window - widening it past a clock step is how a wrong stamp starts passing.
		/// </summary>
		public const int ClockTickMs = 25;

		public static readonly TimeSpan Slack = TimeSpan.FromMilliseconds(ClockTickMs);

		private DateTime _opened;
		private DateTime _closed;

		private StampWindow()
		{
		}

		public static StampWindow Around(Action act)
		{
			var window = new StampWindow { _opened = DateTime.UtcNow };
			act();
			window._closed = DateTime.UtcNow;

			return window;
		}

		public static StampWindow Around<TResult>(Func<TResult> act, out TResult result)
		{
			var window = new StampWindow { _opened = DateTime.UtcNow };
			result = act();
			window._closed = DateTime.UtcNow;

			return window;
		}

		/// <summary>
		/// Asserts a value is Coordinated Universal Time and was read from the clock during the call this
		/// window was captured around.
		/// </summary>
		public void ShouldContainStamp(DateTime value)
		{
			ShouldBeUtcStamp(value);
			value.ShouldBeGreaterThanOrEqualTo(_opened - Slack);
			value.ShouldBeLessThanOrEqualTo(_closed + Slack);
		}

		public void ShouldContainStamp(DateTime? value)
		{
			value.ShouldNotBeNull();
			ShouldContainStamp(value.Value);
		}

		/// <summary>
		/// Rule 18's other half, for a value whose window is no longer in hand: the
		/// <see cref="DateTime.Kind"/> has to survive the round trip, not merely be right on the way out.
		/// </summary>
		public static void ShouldBeUtcStamp(DateTime value)
		{
			value.ShouldNotBe(default(DateTime));
			value.Kind.ShouldBe(DateTimeKind.Utc);
		}

		public static void ShouldBeUtcStamp(DateTime? value)
		{
			value.ShouldNotBeNull();
			ShouldBeUtcStamp(value.Value);
		}
	}
}
