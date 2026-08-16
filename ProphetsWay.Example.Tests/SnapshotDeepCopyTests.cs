using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;

using Shouldly;

using System;

using Xunit;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The half of the SNAPSHOT RULE on <see cref="IExampleDataAccess"/> that no other class in this suite
	/// touches: <b>a snapshot is deep</b>. Every other test here reads and writes scalars, and an implementation
	/// that copies the top-level entity and shares everything hanging off it satisfies all of them while leaving
	/// stored data reachable and mutable through <see cref="User.Company"/>, <see cref="User.Job"/>,
	/// <see cref="User.Department"/>, <see cref="Transaction.User"/> and <see cref="Transaction.Company"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Five separate failures are covered, because they are five different mistakes rather than five views of
	/// one: a copy that stops after the first level, a write that adopts the graph handed to it, a write that
	/// cascades into the graph handed to it, a store that hands the one object it holds to every entity naming
	/// that row, and an undo entry that captured a shared node rather than a copy of it. Adoption and cascade
	/// are neighbours and are not the same mistake: adoption is detected by editing after the call returns,
	/// cascade by editing before it. The second-level tests over <see cref="Transaction"/> are the ones worth
	/// keeping if any were ever lost - <see cref="Transaction"/> is the deepest graph in this project at two
	/// levels, and a copy helper written for one level passes every <see cref="User"/> test in this class and
	/// still leaks there.
	/// </para>
	/// <para>
	/// Every assertion reads through a second Data Access Layer instance for the reason
	/// <see cref="DataAccessTransactionTests"/> gives: "stored" is a claim about the store, and an
	/// implementation that merely remembers what it was handed would satisfy an assertion made through the
	/// writer.
	/// </para>
	/// <para>
	/// It writes Company, Job, Department, User and Transaction rows, and one test rolls a transaction back, so
	/// it belongs to <see cref="TestCollections.SharedStore"/> along with every other class that reaches the
	/// store. It asserts over named rows only and never over a whole-set count.
	/// </para>
	/// <para>
	/// The <c>Scope</c> trait is declared per test rather than on the class, because one test here is
	/// <c>Characterization</c> and the rest are <c>Contract</c>. xUnit accumulates traits rather than letting a
	/// method override a class, so a class-level <c>Contract</c> would leave that one test selected by
	/// <c>--filter "Scope=Contract"</c> no matter what the method declared.
	/// </para>
	/// </remarks>
	[Collection(TestCollections.SharedStore)]
	public class SnapshotDeepCopyTests : BaseUnitTests<IExampleDataAccess>
	{
		/// <summary>The value written through a navigation property. No operation in this class may ever store it.</summary>
		public const string EditedName = "Renamed through a navigation property.";

		public static Company NewCompany => CompanyDaoTests.NewCompany;

		public static Job NewJob => JobDaoTests.NewJob;

		public static Department NewDepartment => DepartmentDaoTests.NewDepartment;

		public static User NewUser => new User { Name = $"Bob {Guid.NewGuid()}" };

		public static Transaction NewTransaction => new Transaction { DateOfAction = DateTime.Now, Amount = 1234.56m };

		/// <summary>
		/// A stored user whose three navigation properties each name a row that really exists, so an
		/// implementation that leaks has something in the store to leak into.
		/// </summary>
		public static (User User, Company Company, Job Job, Department Department) InsertUserWithNavigation(IExampleDataAccess da)
		{
			var co = NewCompany;
			da.Insert(co);

			var job = NewJob;
			da.Insert(job);

			var dept = NewDepartment;
			da.Insert(dept);

			var user = NewUser;
			user.Company = co;
			user.Job = job;
			user.Department = dept;
			da.Insert(user);

			return (user, co, job, dept);
		}

		#region A retrieved instance is a deep snapshot

		public delegate void SnapshotAssertion(IExampleDataAccess reader);

		/// <summary>
		/// One level down, on read. A caller fetches a user and edits the company, job and department it reaches
		/// through it, and never calls <c>Update</c> - so nothing it did may have gone anywhere.
		/// </summary>
		public static (User Retrieved, SnapshotAssertion Assert) Setup_InsertUserWithNavigation_TestRetrievedNavigationIsASnapshot(IExampleDataAccess da)
		{
			var seed = InsertUserWithNavigation(da);
			var companyName = seed.Company.Name;
			var jobName = seed.Job.Name;
			var departmentName = seed.Department.Name;

			var retrieved = da.Get(new User { Id = seed.User.Id });

			return (retrieved, (reader) =>
			{
				var stored = reader.Get(new User { Id = seed.User.Id });

				//the navigation nodes on a snapshot are themselves snapshots, so a second retrieval hands back
				//different objects carrying the values that were stored
				stored.Company.ShouldNotBeSameAs(retrieved.Company);
				stored.Job.ShouldNotBeSameAs(retrieved.Job);
				stored.Department.ShouldNotBeSameAs(retrieved.Department);

				stored.Company.Name.ShouldBe(companyName);
				stored.Job.Name.ShouldBe(jobName);
				stored.Department.Name.ShouldBe(departmentName);

				//and the rows those navigation properties name are untouched too, which is the leak a caller
				//would actually notice
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(companyName);
				reader.Get(new Job { Id = seed.Job.Id }).Name.ShouldBe(jobName);
				reader.Get(new Department { Id = seed.Department.Id }).Name.ShouldBe(departmentName);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldNotStoreEditsMadeThroughARetrievedUsersNavigationProperties()
		{
			//setup
			var test = Setup_InsertUserWithNavigation_TestRetrievedNavigationIsASnapshot(_da);

			//act - the only thing that happens between the setup and the reads is an edit nobody submitted
			test.Retrieved.Company.Name = EditedName;
			test.Retrieved.Job.Name = EditedName;
			test.Retrieved.Department.Name = EditedName;

			//assert
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		/// <summary>
		/// Two levels down, on read, and the highest-value test in this class. A copy helper that reaches one
		/// level - copying the transaction's user but assigning that user's company by reference - passes every
		/// <see cref="User"/> test above and leaks the stored company at the second slot.
		/// </summary>
		public static (Transaction Retrieved, SnapshotAssertion Assert) Setup_InsertTransaction_TestRetrievedSecondLevelIsASnapshot(IExampleDataAccess da)
		{
			var seed = InsertUserWithNavigation(da);

			//a company of the transaction's own, so a leak through Transaction.User.Company and one through
			//Transaction.Company are told apart rather than confused for each other
			var payee = NewCompany;
			da.Insert(payee);

			var trans = NewTransaction;
			trans.User = seed.User;
			trans.Company = payee;
			da.Insert(trans);

			var userCompanyName = seed.Company.Name;
			var userJobName = seed.Job.Name;
			var userDepartmentName = seed.Department.Name;
			var payeeName = payee.Name;
			var userName = seed.User.Name;

			var retrieved = da.Get(new Transaction { Id = trans.Id });

			return (retrieved, (reader) =>
			{
				var stored = reader.Get(new Transaction { Id = trans.Id });

				//second level - the company, job and department the transaction's user names
				stored.User.Company.ShouldNotBeSameAs(retrieved.User.Company);
				stored.User.Company.Name.ShouldBe(userCompanyName);
				stored.User.Job.Name.ShouldBe(userJobName);
				stored.User.Department.Name.ShouldBe(userDepartmentName);

				//first level - the user itself, and the transaction's own company
				stored.User.Name.ShouldBe(userName);
				stored.Company.Name.ShouldBe(payeeName);

				//and none of it reached the rows those navigation properties name, in either table
				reader.Get(new User { Id = seed.User.Id }).Company.Name.ShouldBe(userCompanyName);
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(userCompanyName);
				reader.Get(new Company { Id = payee.Id }).Name.ShouldBe(payeeName);
				reader.Get(new Job { Id = seed.Job.Id }).Name.ShouldBe(userJobName);
				reader.Get(new Department { Id = seed.Department.Id }).Name.ShouldBe(userDepartmentName);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldNotStoreEditsMadeThroughARetrievedTransactionsSecondLevel()
		{
			//setup
			var test = Setup_InsertTransaction_TestRetrievedSecondLevelIsASnapshot(_da);

			//act - the second level first, because that is the slot a one-level copy leaves connected to the store
			test.Retrieved.User.Company.Name = EditedName;
			test.Retrieved.User.Job.Name = EditedName;
			test.Retrieved.User.Department.Name = EditedName;
			test.Retrieved.User.Name = EditedName;
			test.Retrieved.Company.Name = EditedName;

			//assert
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		#endregion

		#region A written instance is read, not adopted

		public delegate void AdoptionAssertion(IExampleDataAccess reader);

		/// <summary>
		/// The write half, one level down. A Data Access Layer that copies on read and then stores the graph it
		/// was handed passes every retrieval test above - it is the natural half-measure of an implementer who
		/// adds copying to <c>Get</c> and stops there - and it hands the caller a live reference into the store
		/// through <see cref="User.Company"/>.
		/// </summary>
		public static (User User, AdoptionAssertion Assert) Setup_CreateUserWithNavigation_TestInsertDoesNotAdoptItsNavigation(IExampleDataAccess da)
		{
			var co = NewCompany;
			da.Insert(co);

			var job = NewJob;
			da.Insert(job);

			var user = NewUser;
			user.Company = co;
			user.Job = job;

			var companyName = co.Name;
			var jobName = job.Name;

			return (user, (reader) =>
			{
				var stored = reader.Get(new User { Id = user.Id });

				//Insert read the graph as it stood at the moment of the call, and the rewrite that followed it
				//reached neither the user's copy of the company nor the company row itself
				stored.Company.Name.ShouldBe(companyName);
				stored.Job.Name.ShouldBe(jobName);
				reader.Get(new Company { Id = co.Id }).Name.ShouldBe(companyName);
				reader.Get(new Job { Id = job.Id }).Name.ShouldBe(jobName);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldNotStoreEditsMadeToAUsersNavigationAfterInsertReturned()
		{
			//setup
			var test = Setup_CreateUserWithNavigation_TestInsertDoesNotAdoptItsNavigation(_da);

			//act
			_da.Insert(test.User);
			test.User.Company.Name = EditedName;
			test.User.Job.Name = EditedName;

			//assert
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		/// <summary>
		/// The same rule on <c>Update</c>, where adopting the argument does the most damage: the instance a
		/// caller hands to <c>Update</c> is usually one it goes on working with, so a store holding a reference
		/// into it absorbs edits nobody submitted for as long as the caller holds it. A legitimate edit is
		/// submitted alongside, so "nothing changed" is not how this passes.
		/// </summary>
		public static (User User, AdoptionAssertion Assert) Setup_InsertUserWithNavigation_TestUpdateDoesNotAdoptItsNavigation(IExampleDataAccess da)
		{
			const string submitted = "Edited before the Update, which is the edit that is supposed to land.";

			var seed = InsertUserWithNavigation(da);
			var companyName = seed.Company.Name;
			var jobName = seed.Job.Name;
			var departmentName = seed.Department.Name;

			var edit = da.Get(new User { Id = seed.User.Id });
			edit.Whatever = submitted;

			return (edit, (reader) =>
			{
				var stored = reader.Get(new User { Id = seed.User.Id });

				//the edit submitted before the call did land
				stored.Whatever.ShouldBe(submitted);

				//and none of the rewriting done through the navigation properties after the call did
				stored.Company.Name.ShouldBe(companyName);
				stored.Job.Name.ShouldBe(jobName);
				stored.Department.Name.ShouldBe(departmentName);
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(companyName);
				reader.Get(new Job { Id = seed.Job.Id }).Name.ShouldBe(jobName);
				reader.Get(new Department { Id = seed.Department.Id }).Name.ShouldBe(departmentName);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldNotStoreEditsMadeToAUsersNavigationAfterUpdateReturned()
		{
			//setup
			var test = Setup_InsertUserWithNavigation_TestUpdateDoesNotAdoptItsNavigation(_da);

			//act
			var count = _da.Update(test.User);
			test.User.Company.Name = EditedName;
			test.User.Job.Name = EditedName;
			test.User.Department.Name = EditedName;

			//assert
			count.ShouldBe(1);

			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		/// <summary>
		/// The write half, two levels down. <c>Insert</c> here has to reach through the user the transaction
		/// names to the company that user names, and an implementation that copies the user but keeps that
		/// user's company by reference passes both write tests above.
		/// </summary>
		public static (Transaction Transaction, AdoptionAssertion Assert) Setup_CreateTransaction_TestInsertDoesNotAdoptItsSecondLevel(IExampleDataAccess da)
		{
			var seed = InsertUserWithNavigation(da);

			var trans = NewTransaction;
			trans.User = seed.User;
			trans.Company = seed.Company;

			var companyName = seed.Company.Name;
			var jobName = seed.Job.Name;
			var userName = seed.User.Name;

			return (trans, (reader) =>
			{
				var stored = reader.Get(new Transaction { Id = trans.Id });

				stored.User.Name.ShouldBe(userName);
				stored.User.Company.Name.ShouldBe(companyName);
				stored.User.Job.Name.ShouldBe(jobName);
				stored.Company.Name.ShouldBe(companyName);

				//and the rows themselves, which is where a leak two levels down finally shows up
				reader.Get(new User { Id = seed.User.Id }).Company.Name.ShouldBe(companyName);
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(companyName);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldNotStoreEditsMadeThroughATransactionsSecondLevelAfterInsertReturned()
		{
			//setup
			var test = Setup_CreateTransaction_TestInsertDoesNotAdoptItsSecondLevel(_da);

			//act
			_da.Insert(test.Transaction);
			test.Transaction.User.Company.Name = EditedName;
			test.Transaction.User.Job.Name = EditedName;
			test.Transaction.User.Name = EditedName;
			test.Transaction.Company.Name = EditedName;

			//assert
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		#endregion

		#region A write addresses one row

		public delegate void CascadeAssertion(IExampleDataAccess reader);

		/// <summary>
		/// The neighbour of the two tests above, and the one they cannot catch. Both of those edit a navigation
		/// property <i>after</i> the write returned, so they detect a store holding a reference into the caller's
		/// graph. This edits the navigation properties <i>before</i> the call, which is the ordinary thing a caller
		/// does with a retrieved entity, and asks what <c>Update</c> wrote.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The rule it traces to is the ROW COUNT RULE on <see cref="IExampleDataAccess"/>: "a write reached through
		/// this interface addresses a single row". <c>Update</c> was handed a <see cref="User"/> and so writes the
		/// user row; the <see cref="Company"/>, <see cref="Job"/> and <see cref="Department"/> rows reachable
		/// through it were never handed to a write member, and the SNAPSHOT RULE has it that stored data changes
		/// only through those. A Data Access Layer that attaches the incoming graph as modified - the natural
		/// shortcut for an Entity Framework implementation - rewrites three rows the caller never named, and does it
		/// to reference data every other user pointing at those rows shares.
		/// </para>
		/// <para>
		/// Nothing here asserts what the user's own view of those navigation properties reads back as. That is the
		/// point <see cref="ShouldReadANavigationPropertyEditBackInsideTheTransactionThatSubmittedIt"/> covers as
		/// characterization, because only a store that denormalizes can make the user's view and the row disagree.
		/// The row is what every conforming implementation has in common, so the row is what this asserts over.
		/// </para>
		/// <para>
		/// The submitted scalar is the discriminator. Without it an <c>Update</c> that did nothing whatsoever would
		/// pass, which would make this a null check rather than a test.
		/// </para>
		/// </remarks>
		public static (User Edit, CascadeAssertion Assert) Setup_EditNavigationOnARetrievedUser_TestUpdateWritesOnlyTheUserRow(IExampleDataAccess da)
		{
			const string submitted = "Edited on the root before the Update, which is the edit that is supposed to land.";

			var seed = InsertUserWithNavigation(da);
			var companyName = seed.Company.Name;
			var jobName = seed.Job.Name;
			var departmentName = seed.Department.Name;

			var edit = da.Get(new User { Id = seed.User.Id });
			edit.Whatever = submitted;
			edit.Company.Name = EditedName;
			edit.Job.Name = EditedName;
			edit.Department.Name = EditedName;

			return (edit, (reader) =>
			{
				//the three rows the caller never named, each still carrying what it carried before the Update
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(companyName);
				reader.Get(new Job { Id = seed.Job.Id }).Name.ShouldBe(jobName);
				reader.Get(new Department { Id = seed.Department.Id }).Name.ShouldBe(departmentName);

				//and the row it did name, carrying the edit it submitted - so "Update wrote nothing" is not how this passes
				reader.Get(new User { Id = seed.User.Id }).Whatever.ShouldBe(submitted);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldNotWriteRelatedRowsWhenUpdateIsGivenAnEditedNavigationGraph()
		{
			//setup
			var test = Setup_EditNavigationOnARetrievedUser_TestUpdateWritesOnlyTheUserRow(_da);

			//act - outside any transaction, so nothing that lands here is reversed afterwards
			var count = _da.Update(test.Edit);

			//assert - one row addressed, and it was the user's
			count.ShouldBe(1);

			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		#endregion

		#region No identity map

		public delegate void IdentityMapAssertion(IExampleDataAccess reader);

		/// <summary>
		/// The failure that looks most like correctness. A dictionary-backed store is naturally written to hand
		/// out the single company object it holds to everything that names that row, and every value assertion
		/// in this suite still reads correctly while it does - right up until one caller's edit turns up in
		/// another caller's object.
		/// </summary>
		public static (User First, User Second, IdentityMapAssertion Assert) Setup_RetrieveTheSameUserTwice_TestTheNavigationInstancesAreIndependent(IExampleDataAccess da)
		{
			var seed = InsertUserWithNavigation(da);
			var companyName = seed.Company.Name;

			var first = da.Get(new User { Id = seed.User.Id });
			var second = da.Get(new User { Id = seed.User.Id });

			return (first, second, (reader) =>
			{
				//two retrievals of one row are two objects, all the way down
				first.ShouldNotBeSameAs(second);
				first.Company.ShouldNotBeSameAs(second.Company);
				first.Job.ShouldNotBeSameAs(second.Job);
				first.Department.ShouldNotBeSameAs(second.Department);

				//so an edit made through one is invisible to the other, and to the store
				second.Company.Name.ShouldBe(companyName);
				reader.Get(new User { Id = seed.User.Id }).Company.Name.ShouldBe(companyName);
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(companyName);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldGiveEachRetrievedUserItsOwnNavigationInstances()
		{
			//setup
			var test = Setup_RetrieveTheSameUserTwice_TestTheNavigationInstancesAreIndependent(_da);

			//act
			test.First.Company.Name = EditedName;

			//assert
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		/// <summary>
		/// The same rule across two different rows that name one company. The users were even inserted holding
		/// the one company object, which is the shape an identity map is most tempted by.
		/// </summary>
		public static (User First, User Second, IdentityMapAssertion Assert) Setup_RetrieveTwoUsersNamingOneCompany_TestTheirCompaniesAreIndependent(IExampleDataAccess da)
		{
			var co = NewCompany;
			da.Insert(co);
			var companyName = co.Name;

			var userA = NewUser;
			userA.Company = co;
			da.Insert(userA);

			var userB = NewUser;
			userB.Company = co;
			da.Insert(userB);

			var first = da.Get(new User { Id = userA.Id });
			var second = da.Get(new User { Id = userB.Id });

			return (first, second, (reader) =>
			{
				//one stored company, two users naming it, two instances
				first.Company.ShouldNotBeSameAs(second.Company);

				second.Company.Name.ShouldBe(companyName);
				reader.Get(new User { Id = userB.Id }).Company.Name.ShouldBe(companyName);
				reader.Get(new User { Id = userA.Id }).Company.Name.ShouldBe(companyName);
				reader.Get(new Company { Id = co.Id }).Name.ShouldBe(companyName);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldGiveTwoUsersNamingOneCompanyTheirOwnCompanyInstances()
		{
			//setup
			var test = Setup_RetrieveTwoUsersNamingOneCompany_TestTheirCompaniesAreIndependent(_da);

			//act
			test.First.Company.Name = EditedName;

			//assert
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		#endregion

		#region Undo entries are deep

		public delegate void RollBackAssertion(IExampleDataAccess reader);

		/// <summary>
		/// What the store looked like from inside the open transaction, before anything was rolled back.
		/// Asserted separately from <see cref="RollBackAssertion"/> because it is not scoped the same way - see
		/// <see cref="ShouldReadANavigationPropertyEditBackInsideTheTransactionThatSubmittedIt"/>.
		/// </summary>
		public delegate void UncommittedReadAssertion();

		/// <summary>
		/// A rollback has to restore a value that was edited through a navigation property, which it can only do
		/// if the undo entry it wrote holds a copy of that node rather than a reference to it. An entry sharing
		/// the node with whatever the update then stored would record the edit rather than the state before it,
		/// and the rollback would put the edit back.
		/// </summary>
		/// <remarks>
		/// The uncommitted read this performs is asserted over by two callers with two different scopes, so it
		/// hands back two assertions rather than one. <see cref="RollBackAssertion"/> covers what the store must
		/// read as afterwards; <see cref="UncommittedReadAssertion"/> covers the one thing the update was
		/// required to have done to the navigation node before it, which no relational store can do.
		/// </remarks>
		public static (User Edit, RollBackAssertion Assert, UncommittedReadAssertion AssertUncommittedCascade) Setup_UpdateNavigationInsideTransaction_TestRollBackRestoresIt(IExampleDataAccess da)
		{
			const string committed = "Committed before the transaction opened, and what the rollback has to put back.";

			//committed before the transaction opens, so its prior state is what the rollback has to restore
			var seed = InsertUserWithNavigation(da);
			var companyName = seed.Company.Name;
			var jobName = seed.Job.Name;
			var departmentName = seed.Department.Name;

			//a scalar the rollback has to restore to a value of its own rather than to the default one - restoring
			//null is satisfied by a rollback that clears the row, which is not what restoring means
			var priming = da.Get(new User { Id = seed.User.Id });
			priming.Whatever = committed;
			da.Update(priming);

			var whateverBefore = da.Get(new User { Id = seed.User.Id }).Whatever;

			da.TransactionStart();

			var edit = da.Get(new User { Id = seed.User.Id });
			edit.Company.Name = EditedName;
			edit.Job.Name = EditedName;
			edit.Whatever = EditedName;
			da.Update(edit);

			//read back inside the transaction, so a rollback that "passes" because the update never landed is
			//told apart from one that reversed it
			var uncommitted = da.Get(new User { Id = seed.User.Id });

			return (edit, (reader) =>
			{
				//the scalar the update submitted is readable from inside the transaction that submitted it
				uncommitted.Whatever.ShouldBe(EditedName);

				var stored = reader.Get(new User { Id = seed.User.Id });

				//the row reads exactly as it did before the transaction opened, two levels of it - the value it
				//carried rather than merely "not the edited one", which a rollback writing garbage would satisfy
				stored.Whatever.ShouldBe(whateverBefore);
				stored.Company.Name.ShouldBe(companyName);
				stored.Job.Name.ShouldBe(jobName);
				stored.Department.Name.ShouldBe(departmentName);

				//and the company row was never in this transaction's way to begin with
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(companyName);
			}
			, () =>
			{
				uncommitted.Company.Name.ShouldBe(EditedName);
			}
			);
		}

		/// <summary>
		/// Named for what it asserts rather than for what its setup does. The setup does edit a navigation property
		/// and submit it; this test does not assert that the edit was ever stored, because only a denormalizing
		/// store can show that - see <see cref="ShouldReadANavigationPropertyEditBackInsideTheTransactionThatSubmittedIt"/>,
		/// which is where that assertion lives and why it is characterization. What is contract here is that after
		/// the rollback every row reads as it did before the transaction opened: the user's scalar restored to the
		/// value it carried, and the company, job and department rows unchanged throughout.
		/// </summary>
		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldReadEverythingAsItWasBeforeARolledBackTransactionThatEditedANavigationProperty()
		{
			//setup
			var test = Setup_UpdateNavigationInsideTransaction_TestRollBackRestoresIt(_da);

			//act
			_da.TransactionRollBack();

			//assert - read through an instance that had no part in writing any of it
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		/// <summary>
		/// <b>Characterization, not contract.</b> The setup edits <see cref="User.Company"/> on a retrieved user
		/// and calls <c>Update</c>; this asserts the new name reads back through the same navigation property
		/// while the <see cref="Company"/> row itself still carries the old one - which the sibling test above
		/// asserts, as contract, after the rollback.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Both at once are only satisfiable by a store that denormalizes.
		/// <c>ProphetsWay.Example.DataAccess.NoDB</c> passes because <c>UserDao.Update</c> writes a deep copy of
		/// the user into the Users table, so the user's view of a company and the Companies table are physically
		/// separate data and can legitimately disagree. A normalized relational store holds one Companies row
		/// with one name and reads it back through a join, so it cannot make the two differ - and an
		/// implementation that wrote the root and cascaded through the navigation would be rewriting Company,
		/// Job and Department rows the caller never named.
		/// </para>
		/// <para>
		/// It is therefore a property of the in-memory store's row shape rather than something the SNAPSHOT RULE
		/// on <see cref="IExampleDataAccess"/> asks of a conforming Data Access Layer, and it must not be
		/// promoted back to <c>Contract</c>. Nothing else in this class depends on it.
		/// </para>
		/// </remarks>
		[Fact]
		[Trait("Scope", "Characterization")]
		public void ShouldReadANavigationPropertyEditBackInsideTheTransactionThatSubmittedIt()
		{
			//setup - the Update this asserts over, and the read of it, both happen here
			var test = Setup_UpdateNavigationInsideTransaction_TestRollBackRestoresIt(_da);

			//assert
			test.AssertUncommittedCascade();

			//the setup left a transaction open, and none of it is meant to reach the store
			_da.TransactionRollBack();
		}

		#endregion
	}
}
