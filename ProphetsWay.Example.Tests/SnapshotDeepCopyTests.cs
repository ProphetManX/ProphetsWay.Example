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
	/// Four separate failures are covered, because they are four different mistakes rather than four views of
	/// one: a copy that stops after the first level, a write that adopts the graph handed to it, a store that
	/// hands the one object it holds to every entity naming that row, and an undo entry that captured a shared
	/// node rather than a copy of it. The second-level tests over <see cref="Transaction"/> are the ones worth
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
	/// </remarks>
	[Collection(TestCollections.SharedStore)]
	[Trait("Scope", "Contract")]
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
		/// A rollback has to restore a value that was edited through a navigation property, which it can only do
		/// if the undo entry it wrote holds a copy of that node rather than a reference to it. An entry sharing
		/// the node with whatever the update then stored would record the edit rather than the state before it,
		/// and the rollback would put the edit back.
		/// </summary>
		public static (User Edit, RollBackAssertion Assert) Setup_UpdateNavigationInsideTransaction_TestRollBackRestoresIt(IExampleDataAccess da)
		{
			//committed before the transaction opens, so its prior state is what the rollback has to restore
			var seed = InsertUserWithNavigation(da);
			var companyName = seed.Company.Name;
			var jobName = seed.Job.Name;

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
				uncommitted.Company.Name.ShouldBe(EditedName);
				uncommitted.Whatever.ShouldBe(EditedName);

				var stored = reader.Get(new User { Id = seed.User.Id });

				//the row reads exactly as it did before the transaction opened, two levels of it
				stored.Whatever.ShouldNotBe(EditedName);
				stored.Company.Name.ShouldBe(companyName);
				stored.Job.Name.ShouldBe(jobName);
				stored.Department.Name.ShouldBe(seed.Department.Name);

				//and the company row was never in this transaction's way to begin with
				reader.Get(new Company { Id = seed.Company.Id }).Name.ShouldBe(companyName);
			}
			);
		}

		[Fact]
		public void ShouldRestoreANavigationPropertyEditedInsideARolledBackTransaction()
		{
			//setup
			var test = Setup_UpdateNavigationInsideTransaction_TestRollBackRestoresIt(_da);

			//act
			_da.TransactionRollBack();

			//assert - read through an instance that had no part in writing any of it
			using (var reader = TestDataAccessFactory.Create())
				test.Assert(reader);
		}

		#endregion
	}
}
