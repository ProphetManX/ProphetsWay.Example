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
	/// Layer - change the single line that initialises <c>_implementation</c> and nothing else.</b> No other
	/// file in this project constructs a Data Access Layer, which is what turns the claim on
	/// <see cref="IExampleDataAccess"/> - that the same tests pass against either one - into something a
	/// reader can act on rather than take on trust.
	/// </para>
	/// <para>
	/// <b>A second repository consuming this one cannot edit that line, so <see cref="Use"/> is the way in.</b>
	/// <c>ProphetsWay.EFTools</c> holds this repository as a pinned git submodule and is under a standing
	/// instruction never to edit a file underneath it - edits happen here and the pointer moves. Without
	/// <see cref="Use"/> the one repository that most wants to run this suite against its own implementation is
	/// the one repository that cannot. The usage sketch is on <see cref="Use"/>.
	/// </para>
	/// <para>
	/// <b>This is not selection from configuration, and must not become it.</b> Reading the implementation
	/// choice from an environment variable or a <c>.runsettings</c> parameter would let one continuous
	/// integration run cover both implementations without a code edit at all, and that is what a real product
	/// should do. It is deliberately not done here: this repository is read before it is run, and one obvious
	/// line beats a lookup whose other half a reader has to go and find. <see cref="Use"/> does not reopen
	/// that. It is code, written once, in a consuming assembly, and it leaves the default below as the visible
	/// unconditional line every test in this repository takes. Nothing here consults an environment, a file or
	/// a runner parameter, and nothing here should be changed so that it does.
	/// </para>
	/// <para>
	/// <b>Why the seam is here rather than a hook on <see cref="BaseUnitTests{T}"/>.</b> An overridable
	/// <c>CreateDataAccess</c> on the base test class would oblige a consuming repository to derive from every
	/// upstream test class and override once per class - which is exactly the set of adapter files that existed
	/// before 3.0.0, four to five lines each and no test logic in any of them. One assignment covers the whole
	/// suite; a virtual hook covers one class at a time. A hook would also miss the tests that call
	/// <see cref="Create"/> directly for a second reader instance, which no override on a base class reaches.
	/// </para>
	/// <para>
	/// <b>The Data Access Layers under <c>ConventionShowcase</c> do not come from here, and must not.</b> Each
	/// of them is deliberately mis-wired to demonstrate one convention failure, so it is the subject of its
	/// test rather than the implementation under test. Those classes construct their own and carry no
	/// dependency on this factory - so <see cref="Use"/> does not, and must not, change what they exercise.
	/// </para>
	/// </remarks>
	public static class TestDataAccessFactory
	{
		private static readonly object _gate = new object();

		/// <summary>
		/// The implementation every test in this suite runs against.
		/// </summary>
		//>>> The one line to change to point this suite at another implementation. <<<
		private static Func<IExampleDataAccess> _implementation = () => new ExampleDataAccess();

		/// <summary>
		/// Set by <see cref="Create"/> the first time it hands an instance out, and read by <see cref="Use"/>
		/// to refuse a swap arriving too late to apply to the whole run.
		/// </summary>
		private static bool _created;

		/// <summary>
		/// Points every test in this suite at <paramref name="implementation"/>, for a consuming assembly that
		/// cannot edit the default line above.
		/// </summary>
		/// <param name="implementation">
		/// Constructs one Data Access Layer instance per call, and is called once per instance rather than once
		/// in total - this suite builds a fresh one for every test, and several tests build a second alongside
		/// it. It must return a non-null instance implementing <see cref="IExampleDataAccess"/>. A delegate is
		/// taken rather than an instance precisely so that construction inputs the default does not need - a
		/// provider, a connection string, a context factory - can be closed over here and supplied nowhere
		/// else. Nothing in this seam assumes a parameterless constructor on the consumer's side.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="implementation"/> is null. Passing null to mean "put it back to the
		/// default" is not supported: this suite has one seam, set once, and a reset is a second seam.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when <see cref="Create"/> has already handed out an instance. See THREAD SAFETY below - this
		/// is the guard, and hitting it means part of the run would have used the default implementation and
		/// part the supplied one.
		/// </exception>
		/// <remarks>
		/// <para>
		/// <b>USAGE - the whole of what a consuming repository writes.</b> One file in the consuming test
		/// assembly. No test class is derived from, no test is edited, and no adapter is written:
		/// </para>
		/// <code>
		/// using System.Runtime.CompilerServices;
		/// using ProphetsWay.Example.Tests;
		///
		/// internal static class TestSeam
		/// {
		/// 	[ModuleInitializer]
		/// 	internal static void PointTheSuiteAtEntityFramework()
		/// 	{
		/// 		TestDataAccessFactory.Use(() =&gt; new ExampleDataAccess(Constants.ConnectionString));
		/// 	}
		/// }
		/// </code>
		/// <para>
		/// <b>THREAD SAFETY - and why a module initializer is the mechanism named above.</b> xUnit runs test
		/// collections in parallel. A static seam read by every test but written once is safe only if the write
		/// happens before the first test constructs a Data Access Layer, and a consuming assembly has no way to
		/// order an ordinary constructor against another collection's tests: xUnit 2.x offers class and
		/// collection fixtures, each constructed when its own collection starts running, by which time another
		/// collection may already be several tests in. A <c>[ModuleInitializer]</c> is not a fixture - the
		/// runtime runs it before any type in that assembly is touched, which is strictly before the test
		/// runner can construct a test class - so it is the only mechanism that orders correctly by
		/// construction rather than by hope. It requires C# 9 or later in the consuming project, which is no
		/// constraint on this repository: nothing here calls <see cref="Use"/>, and this project still compiles
		/// at C# 7.3 on its <c>net48</c> leg.
		/// </para>
		/// <para>
		/// <b>THREAD SAFETY - what is guaranteed here.</b> This method and <see cref="Create"/> take the same
		/// lock, so there are only two interleavings: either <see cref="Use"/> wins outright and every instance
		/// in the run comes from <paramref name="implementation"/>, or it arrives after an instance has been
		/// handed out and throws. There is no third outcome in which some tests silently run against one
		/// implementation and the rest against another - which is the failure this guard exists to make
		/// impossible, because a suite half-run against the wrong Data Access Layer reports a plausible mixture
		/// of passes and failures and names nothing. The delegate is invoked outside the lock, so a consumer
		/// whose construction is slow, or which takes locks of its own, neither serialises this suite nor
		/// deadlocks against it.
		/// </para>
		/// <para>
		/// <b>FAILURE MODE - what a consumer sees when the write is too late.</b> An
		/// <see cref="InvalidOperationException"/> from this method, naming the problem and the remedy. Thrown
		/// from a module initializer the runtime wraps it in a <c>TypeInitializationException</c> and every test
		/// in the consuming assembly fails at once, which is the intended shape: loud, immediate, and
		/// impossible to mistake for a Data Access Layer defect. The one case no guard can catch is a consumer
		/// that never calls this method at all - the suite then runs green against the in-memory implementation
		/// and proves nothing about the consumer's. An assembly wanting protection from that writes its own
		/// assertion that <see cref="Create"/> returns the type it expects; this repository cannot write it,
		/// because it does not know the type.
		/// </para>
		/// <para>
		/// <b>Calling this twice before the first <see cref="Create"/> is permitted, and the last call wins.</b>
		/// That is deliberately not guarded: one assembly has one seam and one place to set it, so a second
		/// call is a consumer's own duplication rather than a race, and a guard would add a second failure mode
		/// to a method whose value is having exactly one.
		/// </para>
		/// </remarks>
		public static void Use(Func<IExampleDataAccess> implementation)
		{
			if (implementation == null)
				throw new ArgumentNullException(nameof(implementation));

			lock (_gate)
			{
				if (_created)
					throw new InvalidOperationException(
						$"{nameof(TestDataAccessFactory)}.{nameof(Use)} was called after {nameof(Create)} had already handed out an instance, " +
						"so part of this run would have used one implementation and part another. Call it from a [ModuleInitializer] in the " +
						"consuming test assembly, which the runtime runs before the test runner can construct a test class - a class or " +
						"collection fixture is constructed too late, because another collection may already be running in parallel.");

				_implementation = implementation;
			}
		}

		/// <summary>
		/// Constructs the Data Access Layer every test in this suite runs against.
		/// </summary>
		/// <returns>
		/// A new instance, never null and never shared - each call constructs one. The caller owns it and
		/// disposes it, which is what <see cref="BaseUnitTests{T}"/> does after every test and what the tests
		/// building a second reader instance do with a <c>using</c>. This method keeps no reference to what it
		/// returns, so disposing one reaches nothing else.
		/// </returns>
		/// <remarks>
		/// The first call fixes the implementation for the rest of the process: after it, <see cref="Use"/>
		/// throws rather than swapping mid-run.
		/// </remarks>
		public static IExampleDataAccess Create()
		{
			Func<IExampleDataAccess> implementation;

			lock (_gate)
			{
				_created = true;
				implementation = _implementation;
			}

			//outside the lock - a consumer's construction may be slow, and may take locks of its own
			return implementation();
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
		/// nor the factory that produced the instance. That check earns its keep twice over once
		/// <see cref="Use"/> is in play, since the instance may then come from a repository this one has never
		/// been compiled against.
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
