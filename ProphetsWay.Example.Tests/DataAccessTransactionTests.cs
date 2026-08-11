using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.NoDB;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The TRANSACTIONS and DISPOSAL rules stated on <c>IBaseDataAccess</c> in
	/// <c>ProphetsWay.BaseDataAccess</c>, exercised against this repository's Data Access Layer. Those three
	/// members are abstract on <c>BaseDataAccess</c> - the base library performs none of this - so every rule
	/// below is an obligation this implementation has to meet on its own, and nothing but a test proves it did.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Two entity types, deliberately.</b> <c>TransactionStart</c> is on the Data Access Layer and not on a
	/// Dao, so a transaction covers every write made through the instance whatever it wrote. A suite that only
	/// ever transacted over <see cref="Department"/> would be satisfied by a Department-only implementation, and
	/// the feature's whole point - a complex set of records that all persist or none do - would go unproven. The
	/// batch tests therefore write a <see cref="Department"/> and a <see cref="Company"/> together and assert the
	/// two of them share one fate.
	/// </para>
	/// <para>
	/// <b>Why this class joins <see cref="TestCollections.CoreEntities"/>.</b> It writes Department and Company
	/// rows and asserts over whole-set counts, which is the same reason every other class in that collection is
	/// in it. It matters more here than elsewhere: a rollback removes rows and a disposal rolls one back, so this
	/// is the most destructive class in the suite, and it must never run beside another class reading the types
	/// it is unwinding. Both types it touches already live in that collection, so nothing about the existing
	/// grouping has to change and <see cref="TestCollections.CompanyResources"/> still runs in parallel
	/// untouched. <b>Keep it that way</b> - reaching for <see cref="CompanyResource"/> in a transaction test
	/// would drag the two collections into one and cost the suite that parallelism.
	/// </para>
	/// <para>
	/// <b>Reads are done through a second instance.</b> "Persisted" and "discarded" are claims about the store,
	/// not about the instance that wrote them, and an implementation that merely remembers what it did would
	/// satisfy an assertion made through the writer. Every batch assertion below takes a reader Data Access
	/// Layer, and the tests hand it a freshly constructed one.
	/// </para>
	/// <para>
	/// <b>On uncommitted reads.</b> <see cref="ShouldExposeUncommittedWritesToAnotherInstance"/> pins the fact
	/// that a second instance can see writes this instance has not committed. That is <c>READ UNCOMMITTED</c>,
	/// and it is an accepted limitation of an in-memory store rather than a rule of <c>IBaseDataAccess</c>, which
	/// specifies no isolation level at all. It is pinned because this repository is read as documentation: an
	/// unwritten limitation leaves a reader unable to tell an accepted tradeoff from a bug, and the test states
	/// which it is in the one place a reader will believe. If isolation is ever added, that test fails - which is
	/// the correct outcome, because a change of isolation level is a change of behaviour a reader must be told
	/// about, and the failure sends whoever made it back to this comment.
	/// </para>
	/// </remarks>
	[Collection(TestCollections.CoreEntities)]
	public class DataAccessTransactionTests : BaseUnitTests<IExampleDataAccess>
	{
		protected override IExampleDataAccess GetIExampleDataAccess => new ExampleDataAccess();

		public static Company NewCompany => CompanyDaoTests.NewCompany;

		public static Department NewDepartment => DepartmentDaoTests.NewDepartment;

		#region One transaction per instance - TransactionStart

		public delegate void NestingAssertion(Action secondStart);
		public static (Department Department, NestingAssertion Assert) Setup_OpenTransaction_TestASecondStartThrows(IExampleDataAccess da)
		{
			da.TransactionStart();

			var dept = NewDepartment;
			da.Insert(dept);

			return (dept, (secondStart) =>
			{
				//One transaction per instance. Transactions do not nest, and the second call is a programming
				//error rather than a request to nest one.
				Should.Throw<InvalidOperationException>(secondStart);
			}
			);
		}

		[Fact]
		public void ShouldThrowWhenATransactionIsStartedWhileOneIsOpen()
		{
			//setup
			var test = Setup_OpenTransaction_TestASecondStartThrows(_da);

			//act & assert - the transaction left open here is rolled back when the base class disposes _da
			test.Assert(() => _da.TransactionStart());
		}

		#endregion

		#region No transaction open - TransactionCommit and TransactionRollBack

		public delegate void NoTransactionAssertion(Action ender);
		public static (Department Department, NoTransactionAssertion Assert) Setup_NoTransaction_TestEitherEnderThrows(IExampleDataAccess da)
		{
			//A write made outside a transaction, so the instance has done work but has nothing open. An
			//implementation that treats "has written something" as "has a transaction" fails here.
			var dept = NewDepartment;
			da.Insert(dept);

			return (dept, (ender) =>
			{
				Should.Throw<InvalidOperationException>(ender);
			}
			);
		}

		[Fact]
		public void ShouldThrowWhenCommittingWithNoTransactionOpen()
		{
			//setup
			var test = Setup_NoTransaction_TestEitherEnderThrows(_da);

			//act & assert
			test.Assert(() => _da.TransactionCommit());
		}

		[Fact]
		public void ShouldThrowWhenRollingBackWithNoTransactionOpen()
		{
			//setup
			var test = Setup_NoTransaction_TestEitherEnderThrows(_da);

			//act & assert
			test.Assert(() => _da.TransactionRollBack());
		}

		#endregion

		#region A closed transaction stays closed

		public delegate void ClosedTransactionAssertion(Action secondEnder, IExampleDataAccess reader);

		/// <summary>
		/// A transaction opened, written to, and then closed - by a commit or by a rollback, because the rule is
		/// the same either way and an implementation is quite capable of getting one right and the other wrong.
		/// </summary>
		public static (Department Department, ClosedTransactionAssertion Assert) Setup_ClosedTransaction_TestASecondEnderThrows(IExampleDataAccess da, bool closedByCommit)
		{
			var dept = NewDepartment;
			var name = dept.Name;

			da.TransactionStart();
			da.Insert(dept);

			if (closedByCommit)
				da.TransactionCommit();
			else
				da.TransactionRollBack();

			return (dept, (secondEnder, reader) =>
			{
				//Both enders leave the instance with no transaction open, so either one called now is a call
				//with nothing to close.
				Should.Throw<InvalidOperationException>(secondEnder);

				//and the refused call decided nothing - what the first ender did to the batch still stands
				var stored = reader.Get(new Department { Id = dept.Id });

				if (closedByCommit)
				{
					stored.ShouldNotBeNull();
					stored.Name.ShouldBe(name);
				}
				else
				{
					stored.ShouldBeNull();
				}
			}
			);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ShouldThrowWhenCommittingAfterTheTransactionIsClosed(bool closedByCommit)
		{
			//setup
			var test = Setup_ClosedTransaction_TestASecondEnderThrows(_da, closedByCommit);

			//act & assert
			using (var reader = new ExampleDataAccess())
				test.Assert(() => _da.TransactionCommit(), reader);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ShouldThrowWhenRollingBackAfterTheTransactionIsClosed(bool closedByCommit)
		{
			//setup
			var test = Setup_ClosedTransaction_TestASecondEnderThrows(_da, closedByCommit);

			//act & assert
			using (var reader = new ExampleDataAccess())
				test.Assert(() => _da.TransactionRollBack(), reader);
		}

		/// <summary>
		/// The other half of "leaves the instance with no transaction open", which the throwing tests above
		/// cannot prove on their own: an implementation that never clears its state satisfies every one of them
		/// and then refuses to open a second transaction for the rest of the instance's life.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ShouldOpenANewTransactionOnceTheLastOneIsClosed(bool closedByCommit)
		{
			//setup
			Setup_ClosedTransaction_TestASecondEnderThrows(_da, closedByCommit);

			//act & assert - the instance is reusable, and the transaction opened here is rolled back on disposal
			Should.NotThrow(() => _da.TransactionStart());
		}

		#endregion

		#region Commit persists, rollback discards - across entity types

		public delegate void BatchAssertion(IExampleDataAccess reader);

		/// <summary>
		/// One transaction, two entity types, one fate. The counts are captured before the transaction opens so
		/// that a rollback can be held to returning the store to exactly where it was, rather than merely to
		/// hiding the rows it wrote.
		/// </summary>
		public static (Department Department, Company Company, BatchAssertion Assert) Setup_TransactionWritingTwoEntityTypes_TestCommitPersistsBoth(IExampleDataAccess da)
		{
			var dept = NewDepartment;
			var deptName = dept.Name;

			var co = NewCompany;
			var coName = co.Name;

			var departmentCount = da.GetCount<Department>();
			var companyCount = da.GetCount<Company>();

			da.TransactionStart();
			da.Insert(dept);
			da.Insert(co);

			return (dept, co, (reader) =>
			{
				//Commit persists everything written since TransactionStart - both types, not whichever one the
				//implementation happened to wire up.
				var storedDept = reader.Get(new Department { Id = dept.Id });
				storedDept.ShouldNotBeNull();
				storedDept.Name.ShouldBe(deptName);

				var storedCo = reader.Get(new Company { Id = co.Id });
				storedCo.ShouldNotBeNull();
				storedCo.Name.ShouldBe(coName);

				//and they are rows in the store, not just objects the writer can still hand back
				reader.GetAll<Department>().Any(x => x.Id == dept.Id).ShouldBeTrue();
				reader.GetCount<Department>().ShouldBe(departmentCount + 1);
				reader.GetCount<Company>().ShouldBe(companyCount + 1);
			}
			);
		}

		[Fact]
		public void ShouldPersistEveryEntityTypeWrittenInACommittedTransaction()
		{
			//setup
			var test = Setup_TransactionWritingTwoEntityTypes_TestCommitPersistsBoth(_da);

			//act
			_da.TransactionCommit();

			//assert - read through an instance that had no part in writing them
			using (var reader = new ExampleDataAccess())
				test.Assert(reader);
		}

		public static (Department Department, Company Company, BatchAssertion Assert) Setup_TransactionWritingTwoEntityTypes_TestRollBackDiscardsBoth(IExampleDataAccess da)
		{
			var dept = NewDepartment;
			var co = NewCompany;

			var departmentCount = da.GetCount<Department>();
			var companyCount = da.GetCount<Company>();

			da.TransactionStart();
			da.Insert(dept);
			da.Insert(co);

			return (dept, co, (reader) =>
			{
				//Rollback discards everything written since TransactionStart. Either they all persist or none
				//of them do - a rollback that unwound the Department and left the Company behind would be the
				//exact failure this feature exists to prevent.
				reader.Get(new Department { Id = dept.Id }).ShouldBeNull();
				reader.Get(new Company { Id = co.Id }).ShouldBeNull();

				//and the store is back where it started, rather than carrying two rows that merely no longer
				//answer to their identifiers
				reader.GetAll<Department>().Any(x => x.Id == dept.Id).ShouldBeFalse();
				reader.GetCount<Department>().ShouldBe(departmentCount);
				reader.GetCount<Company>().ShouldBe(companyCount);
			}
			);
		}

		[Fact]
		public void ShouldDiscardEveryEntityTypeWrittenInARolledBackTransaction()
		{
			//setup
			var test = Setup_TransactionWritingTwoEntityTypes_TestRollBackDiscardsBoth(_da);

			//act
			_da.TransactionRollBack();

			//assert
			using (var reader = new ExampleDataAccess())
				test.Assert(reader);
		}

		/// <summary>
		/// Rollback against rows that were already there. Every other rollback test writes new rows, and an
		/// implementation that unwinds by deleting whatever it inserted passes all of them while leaving an
		/// updated or deleted row permanently changed.
		/// </summary>
		public static (Department Department, BatchAssertion Assert) Setup_UpdateAndDeleteInsideTransaction_TestRollBackRestoresPriorState(IExampleDataAccess da)
		{
			//committed before the transaction opens, so its prior state is what a rollback has to restore
			var dept = NewDepartment;
			da.Insert(dept);

			var name = dept.Name;
			var description = dept.Description;
			var createdDate = dept.CreatedDate;
			var departmentCount = da.GetCount<Department>();

			da.TransactionStart();
			da.Update(new Department { Id = dept.Id, Name = "Renamed inside the transaction.", Description = null });
			da.Delete(new Department { Id = dept.Id });

			return (dept, (reader) =>
			{
				var stored = reader.Get(new Department { Id = dept.Id });

				//the row is still there and reads exactly as it did before the transaction opened
				stored.ShouldNotBeNull();
				stored.Name.ShouldBe(name);
				stored.Description.ShouldBe(description);
				stored.CreatedDate.ShouldBe(createdDate);

				//including the stamps the discarded operations wrote - an Update that survives its rollback is
				//still a write that survived its rollback
				stored.UpdatedDate.ShouldBeNull();
				stored.DeletedDate.ShouldBeNull();

				//and the soft delete is undone in the live views too, not only in the stamp
				reader.GetAll<Department>().Any(x => x.Id == dept.Id).ShouldBeTrue();
				reader.GetCount<Department>().ShouldBe(departmentCount);
			}
			);
		}

		[Fact]
		public void ShouldRestoreUpdatedAndDeletedRowsWhenTheTransactionIsRolledBack()
		{
			//setup
			var test = Setup_UpdateAndDeleteInsideTransaction_TestRollBackRestoresPriorState(_da);

			//act
			_da.TransactionRollBack();

			//assert
			using (var reader = new ExampleDataAccess())
				test.Assert(reader);
		}

		#endregion

		#region Outside a transaction, every call auto-commits on its own

		/// <summary>
		/// A write made with nothing open stands alone. The rollback that follows it in the test belongs to a
		/// transaction opened afterwards, and must not reach back over work that was already committed.
		/// </summary>
		public static (Department Department, BatchAssertion Assert) Setup_WriteOutsideATransaction_TestALaterRollBackLeavesItAlone(IExampleDataAccess da)
		{
			var dept = NewDepartment;
			da.Insert(dept);

			var name = dept.Name;
			var createdDate = dept.CreatedDate;

			return (dept, (reader) =>
			{
				var stored = reader.Get(new Department { Id = dept.Id });

				stored.ShouldNotBeNull();
				stored.Name.ShouldBe(name);
				stored.CreatedDate.ShouldBe(createdDate);
				reader.GetAll<Department>().Any(x => x.Id == dept.Id).ShouldBeTrue();
			}
			);
		}

		[Fact]
		public void ShouldLeaveWorkDoneOutsideATransactionAloneWhenALaterTransactionIsRolledBack()
		{
			//setup
			var test = Setup_WriteOutsideATransaction_TestALaterRollBackLeavesItAlone(_da);

			//act - a whole transaction, opened after the fact and abandoned. An implementation that keeps one
			//undo log for the lifetime of the instance rather than one per transaction loses the row above.
			_da.TransactionStart();
			_da.Insert(NewDepartment);
			_da.TransactionRollBack();

			//assert
			using (var reader = new ExampleDataAccess())
				test.Assert(reader);
		}

		#endregion

		#region Scope is the instance, not the connection

		/// <summary>
		/// Two Data Access Layers over the one store: one inside a transaction, one not. This is the rule an
		/// implementation is most likely to get wrong, because the store they share is process-wide and a
		/// transaction implemented on the store instead of on the instance passes almost everything else here.
		/// </summary>
		public static (Department Transactional, Department Other, BatchAssertion Assert) Setup_TwoInstancesWriting_TestRollBackTouchesOnlyItsOwn(IExampleDataAccess transactional, IExampleDataAccess other)
		{
			var mine = NewDepartment;
			var theirs = NewDepartment;
			var theirName = theirs.Name;

			transactional.TransactionStart();
			transactional.Insert(mine);

			//the other instance has nothing open, so its write auto-commits even while a transaction is open
			//elsewhere - it was never enrolled in one it did not start
			other.Insert(theirs);

			return (mine, theirs, (reader) =>
			{
				reader.Get(new Department { Id = mine.Id }).ShouldBeNull();

				var stored = reader.Get(new Department { Id = theirs.Id });
				stored.ShouldNotBeNull();
				stored.Name.ShouldBe(theirName);
			}
			);
		}

		[Fact]
		public void ShouldNotEnrolAnotherInstancesWorkInThisInstancesTransaction()
		{
			//setup
			using (var other = new ExampleDataAccess())
			{
				var test = Setup_TwoInstancesWriting_TestRollBackTouchesOnlyItsOwn(_da, other);

				//act
				_da.TransactionRollBack();

				//assert
				using (var reader = new ExampleDataAccess())
					test.Assert(reader);
			}
		}

		/// <summary>
		/// The same rule seen from the transaction state rather than from the data: one instance having a
		/// transaction open says nothing about any other instance. A flag held anywhere but on the instance -
		/// static, or on the shared store - fails both halves of this.
		/// </summary>
		[Fact]
		public void ShouldTrackTransactionStateOnEachInstanceSeparately()
		{
			//setup
			_da.TransactionStart();

			using (var other = new ExampleDataAccess())
			{
				//act & assert - the other instance has nothing open, whatever this one is doing
				Should.Throw<InvalidOperationException>(() => other.TransactionCommit());
				Should.Throw<InvalidOperationException>(() => other.TransactionRollBack());

				//and it may open one of its own, which is not a nested transaction - it is a second one
				Should.NotThrow(() => other.TransactionStart());

				other.TransactionRollBack();
			}

			_da.TransactionRollBack();
		}

		#endregion

		#region Disposal rolls back, never commits

		[Fact]
		public void ShouldRollBackAnOpenTransactionWhenDisposed()
		{
			//setup - a separate instance, because this test is about what disposing one does
			var writer = new ExampleDataAccess();
			var test = Setup_TransactionWritingTwoEntityTypes_TestRollBackDiscardsBoth(writer);

			//act - an unclosed transaction is an abandoned one, and Dispose never throws even while unwinding it
			Should.NotThrow(() => writer.Dispose());

			//assert - the same assertion the explicit rollback makes, because that is what disposal owes
			using (var reader = new ExampleDataAccess())
				test.Assert(reader);
		}

		[Fact]
		public void ShouldThrowWhenTransactionStartIsCalledAfterDispose()
		{
			//setup
			var da = new ExampleDataAccess();
			da.Dispose();

			//act & assert
			Should.Throw<ObjectDisposedException>(() => da.TransactionStart());
		}

		[Fact]
		public void ShouldThrowWhenTransactionCommitIsCalledAfterDispose()
		{
			//setup
			var da = new ExampleDataAccess();
			da.Dispose();

			//act & assert - ObjectDisposedException, not the InvalidOperationException a closed transaction
			//would earn. It derives from it, so a consumer catching the general case still catches this one.
			Should.Throw<ObjectDisposedException>(() => da.TransactionCommit());
		}

		[Fact]
		public void ShouldThrowWhenTransactionRollBackIsCalledAfterDispose()
		{
			//setup
			var da = new ExampleDataAccess();
			da.Dispose();

			//act & assert
			Should.Throw<ObjectDisposedException>(() => da.TransactionRollBack());
		}

		#endregion

		#region Accepted limitation - READ UNCOMMITTED

		/// <summary>
		/// Pins the isolation this Data Access Layer does not provide. A write made inside a transaction is
		/// visible to another instance before it is committed, because the in-memory store applies writes as
		/// they arrive and keeps an undo log to reverse them - there is nowhere else for an uncommitted row to
		/// live. That is <c>READ UNCOMMITTED</c>, it is accepted here, and a store backed by a real database
		/// gets its isolation from the provider rather than from this code.
		/// </summary>
		/// <remarks>
		/// This is a characterization test of the implementation, not a rule of <c>IBaseDataAccess</c> - that
		/// contract specifies no isolation level. It is written down because the alternative is a reader finding
		/// the behaviour on their own and being unable to tell an accepted tradeoff from a defect.
		/// </remarks>
		[Fact]
		public void ShouldExposeUncommittedWritesToAnotherInstance()
		{
			//setup
			var dept = NewDepartment;

			_da.TransactionStart();
			_da.Insert(dept);

			//act & assert
			using (var reader = new ExampleDataAccess())
			{
				//no isolation: the row is already there, uncommitted
				reader.Get(new Department { Id = dept.Id }).ShouldNotBeNull();

				_da.TransactionRollBack();

				//and it is gone again once the transaction that wrote it is abandoned
				reader.Get(new Department { Id = dept.Id }).ShouldBeNull();
			}
		}

		#endregion
	}
}
