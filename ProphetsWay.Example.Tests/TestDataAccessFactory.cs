using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.NoDB;

using System;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The one place in this suite that names a Data Access Layer implementation.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>To run this suite against a different implementation - an Entity Framework or an MSSQL Data Access
	/// Layer - change the single <c>return</c> in <see cref="Create"/> and nothing else.</b> No other file in
	/// this project constructs a Data Access Layer, which is what turns the claim on
	/// <see cref="IExampleDataAccess"/> - that the same tests pass against either one - into something a
	/// reader can act on rather than take on trust.
	/// </para>
	/// <para>
	/// Reading the choice from an environment variable or a <c>.runsettings</c> parameter would let one
	/// continuous integration run cover both implementations without a code edit at all, and that is what a
	/// real product should do. It is deliberately not done here: this repository is read before it is run, and
	/// one obvious line beats a lookup whose other half a reader has to go and find.
	/// </para>
	/// <para>
	/// <b>The Data Access Layers under <c>ConventionShowcase</c> do not come from here, and must not.</b> Each
	/// of them is deliberately mis-wired to demonstrate one convention failure, so it is the subject of its
	/// test rather than the implementation under test. Those classes construct their own and carry no
	/// dependency on this factory.
	/// </para>
	/// </remarks>
	public static class TestDataAccessFactory
	{
		/// <summary>
		/// Constructs the Data Access Layer every test in this suite runs against.
		/// </summary>
		public static IExampleDataAccess Create()
		{
			//>>> The one line to change to point this suite at another implementation. <<<
			return new ExampleDataAccess();
		}

		/// <summary>
		/// The same instance, seen through one of the Data Access Object interfaces
		/// <see cref="IExampleDataAccess"/> aggregates.
		/// </summary>
		/// <remarks>
		/// <see cref="BaseUnitTests{T}"/> closes over a Dao interface far more often than over the aggregate,
		/// and there is no generic constraint that expresses "an interface <see cref="IExampleDataAccess"/>
		/// happens to inherit" - so this is a cast. It is checked rather than blind: an implementation that
		/// stopped implementing one of the Dao interfaces would otherwise surface as an
		/// <see cref="InvalidCastException"/> from a constructor, naming neither the interface that is missing
		/// nor the factory that produced the instance.
		/// </remarks>
		public static T CreateAs<T>()
		{
			var da = Create();

			if (da is T)
				return (T)(object)da;

			var actual = da.GetType().FullName;
			da.Dispose();

			throw new InvalidOperationException(
				$"{nameof(TestDataAccessFactory)}.{nameof(Create)} returned {actual}, which does not implement {typeof(T).FullName}. " +
				$"Every type argument used with BaseUnitTests<T> has to be an interface that {nameof(IExampleDataAccess)} aggregates.");
		}
	}
}
