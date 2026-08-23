using ProphetsWay.Example.DataAccess;

using System;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// What a Data Access Layer's store is structurally able to do, declared by whoever points this suite at
	/// it. Two tests here read it, and nothing else in the suite does.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Why this exists at all.</b> Almost everything in this suite is a rule every conforming Data Access
	/// Layer owes - that is what <c>Scope=Contract</c> means, and none of it is negotiable. A small number of
	/// tests are not rules but observations, pinned because a reader of this repository would otherwise be
	/// unable to tell an accepted tradeoff from a defect. Two of those observations are true of the in-memory
	/// implementation and <b>cannot</b> be true of a normalized relational one - not "are not yet", but cannot,
	/// because the row shape that makes one true is the row shape that makes the other impossible. Left
	/// unqualified they hand every correct relational implementation a permanent two-test failure, and a suite
	/// that is permanently two-red is a suite whose red stops meaning anything.
	/// </para>
	/// <para>
	/// <b>What this is not.</b> It is not a way to opt out of a rule. No <c>Scope=Contract</c> assertion reads
	/// this enum, and none may be made to: a contract test is an obligation on every implementation, and an
	/// obligation a caller can decline is not one. What a declaration here changes is <i>which</i> outcome a
	/// characterization test asserts - never <i>whether</i> it asserts. Both branches of both tests that read
	/// this assert something real, and the branch a normalized store takes is in one case the stronger of the
	/// two: it proves an <c>Update</c> did not cascade into rows the caller never named.
	/// </para>
	/// <para>
	/// <b>Declaring is code, once, beside the implementation choice.</b> The second argument to
	/// <see cref="TestDataAccessFactory.Use(Func{IExampleDataAccess}, StoreCapabilities)"/> and nowhere else -
	/// same file, same call, same module initializer. It is deliberately not an environment variable, a
	/// <c>.runsettings</c> parameter or a configuration file, for the reason the remarks on
	/// <see cref="TestDataAccessFactory"/> give at length: this repository is read before it is run, and one
	/// obvious line beats a lookup whose other half a reader has to go and find.
	/// </para>
	/// <para>
	/// <b>The default is the in-memory implementation's shape, so this repository declares nothing.</b> A
	/// consumer that never calls <see cref="TestDataAccessFactory.Use(Func{IExampleDataAccess}, StoreCapabilities)"/>
	/// gets <see cref="TestDataAccessFactory.InMemoryStore"/>, which is what
	/// <c>ProphetsWay.Example.DataAccess.NoDB</c> is - so the visible unconditional line every test here takes
	/// stays exactly as short as it was. The cost of that choice is borne by a consumer who forgets: they see
	/// the same two failures they would have seen anyway, which is no worse than today and is the only
	/// behaviour available, since nothing in this assembly can look at a Data Access Layer and infer its row
	/// shape.
	/// </para>
	/// <para>
	/// <b>These are flags rather than one "kind of store" value on purpose.</b> The two axes are independent -
	/// a document store denormalizes <i>and</i> isolates, an in-memory store here does the first and not the
	/// second, a relational store does the second and not the first - so collapsing them into a single
	/// <c>Relational</c> or <c>InMemory</c> label would force a third store to pick the nearest lie. Adding an
	/// axis later is a new flag, and a store that does not know about it keeps the value it already declares.
	/// </para>
	/// </remarks>
	[Flags]
	public enum StoreCapabilities
	{
		/// <summary>
		/// Declares neither capability below. A normalized relational store that provides isolation declares
		/// <see cref="TransactionIsolation"/> rather than this.
		/// </summary>
		None = 0,

		/// <summary>
		/// An <c>Update</c> of an entity stores its own copy of the navigation nodes it was handed, so a later
		/// read of that entity sees the edited node while the row that node names still carries its committed
		/// value. The two are physically separate data and may legitimately disagree.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>A normalized relational store must not declare this, and cannot fake it.</b> It holds one
		/// Companies row with one name and reads it back through a join, so the entity's view of a company and
		/// the Companies table are the same data and cannot differ. The only way to make them appear to differ
		/// is to cascade the write into the Company, Job and Department rows the caller never named - which is
		/// a defect, and which the branch a store without this flag takes is written to catch.
		/// </para>
		/// <para>
		/// This is a property of the store's row shape. It is not asked for by the SNAPSHOT RULE on
		/// <see cref="IExampleDataAccess"/>, which requires only that a write reads its argument rather than
		/// adopting it, and it must never be treated as though it were.
		/// </para>
		/// </remarks>
		DenormalizedNavigationWrites = 1,

		/// <summary>
		/// A write made inside an open transaction is not visible to another Data Access Layer instance until
		/// that transaction commits.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <c>IBaseDataAccess</c> specifies no isolation level at all, so this is neither required nor
		/// forbidden - which is precisely why it has to be declared rather than detected. The in-memory store
		/// does not have it: it applies writes as they arrive and keeps an undo log to reverse them, and there
		/// is nowhere else for an uncommitted row to live. A store backed by a real database gets isolation
		/// from its provider and should declare this.
		/// </para>
		/// <para>
		/// <b>Declaring it also stops the suite waiting on a lock.</b> The test that reads this does not merely
		/// assert differently, it takes a different path: against a store with isolation the read of an
		/// uncommitted row is not <i>performed</i>, because that read blocks until the writing transaction ends
		/// and then dies on the provider's command timeout. Skipping the assertion while still making the read
		/// would leave the whole cost in place, so the branch is around the read and not around the
		/// <c>ShouldBe</c>.
		/// </para>
		/// </remarks>
		TransactionIsolation = 2
	}
}
