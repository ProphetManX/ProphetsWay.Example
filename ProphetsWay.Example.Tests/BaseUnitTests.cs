using Shouldly;

using System;

namespace ProphetsWay.Example.Tests
{
	public abstract class BaseUnitTests<T> : IDisposable
	{
		protected T _da;

		/// <summary>
		/// The Data Access Layer under test, from <see cref="TestDataAccessFactory"/> - which is the only
		/// place in this suite that names an implementation, and the one line to change to run everything
		/// here against a different one.
		/// </summary>
		public BaseUnitTests()
		{
			_da = TestDataAccessFactory.CreateAs<T>();
		}

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
	/// Which test classes may not run at the same time as which others - which, in this suite, is all of them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every implementation in this repository writes to one store, so two test classes running at once are two
	/// threads writing to the same tables. Any assertion phrased over a whole set - a <c>GetCount</c>, a
	/// <c>GetAll().Count</c>, a page index - is then racing whatever the other class is inserting, and fails
	/// intermittently and only when run alongside its neighbour.
	/// </para>
	/// <para>
	/// xUnit runs collections in parallel and the classes within one collection in sequence, so one name here is
	/// one group that runs single file. <b>There used to be two.</b>
	/// <see cref="DataAccess.Entities.CompanyResource"/> shared no entity type with anything else, because its
	/// joins named synthetic company and resource identifiers and no row was ever created for them, so it stood
	/// alone and ran in parallel. Rule 10 on <see cref="DataAccess.IDaos.ICompanyResourceDao"/> ended that: a
	/// caller must name a company that exists and a resource that exists, so the join tests now insert
	/// <see cref="DataAccess.Entities.Company"/> and <see cref="DataAccess.Entities.Resource"/> rows of their
	/// own. That puts them against the exact whole-set counts in <see cref="DataAccessTransactionTests"/>,
	/// which read <c>GetCount&lt;Company&gt;()</c> before a transaction opens and assert on it afterwards - an
	/// assertion a Company inserted by another thread breaks, intermittently and only in a full run.
	/// </para>
	/// <para>
	/// The classes under <c>ConventionShowcase</c> are the exception and carry no <c>[Collection]</c> at all:
	/// their Data Access Layers never reach the store, so they have nothing to race.
	/// </para>
	/// <para>
	/// Splitting this group again means finding classes that share no entity type with any other, and the
	/// entity graph no longer offers one. It is worth revisiting only if the suite grows enough for the wall
	/// clock to matter, and worth measuring before rather than assuming.
	/// </para>
	/// </remarks>
	public static class TestCollections
	{
		public const string SharedStore = "DataStore - every entity type";
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
