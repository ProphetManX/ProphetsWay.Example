using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using ProphetsWay.Example.DataAccess.NoDB;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The soft-delete contract of <see cref="IDepartmentDao"/>, exercised directly against the DAO.
	/// The same setup helpers are replayed through the generic dispatcher by
	/// <see cref="DepartmentDataAccessTests"/>, which is how this repository shows both paths agree.
	/// Rule numbers in the comments refer to the numbered CONTRACT list on <see cref="IDepartmentDao"/>.
	/// </summary>
	[Collection(TestCollections.CoreEntities)]
	public class DepartmentDaoTests : BaseUnitTests<IDepartmentDao>
	{
		protected override IDepartmentDao GetIExampleDataAccess => new ExampleDataAccess();

		public static Department NewDepartment => new Department { Name = $"Dept {Guid.NewGuid()}", Description = "Initial description." };

		/// <summary>A value the caller assigns to a timestamp it does not own. No operation may ever store it.</summary>
		public static readonly DateTime BogusStamp = new DateTime(1999, 12, 31, 23, 59, 59);

		/// <summary>An identifier no test ever inserts, used for the "matches nothing" cases.</summary>
		public const int UnstoredId = 987654321;

		//DateTime resolution on net48 is coarse. Where a test has to tell two stamps apart, let the clock move.
		//This is a resolution problem, not an isolation one: two updates in quick succession can legitimately
		//read the same value out of the clock, and rule 2 can only be proved by two stamps that differ. Test
		//collections do not help here and removing the sleep either makes the assertion flaky or weakens it to
		//>=, which stops detecting an Update that never touched the field at all.
		public const int ClockTickMs = StampWindow.ClockTickMs;

		#region Insert - rule 1

		public delegate void InsertAssertion(Department dept, StampWindow window);
		public static (Department Department, InsertAssertion Assert) Setup_CreateDepartment_TestInsert()
		{
			//Every field Insert owns is pre-loaded with a value Insert must overwrite.
			var dept = NewDepartment;
			dept.Id = UnstoredId;
			dept.CreatedDate = BogusStamp;
			dept.UpdatedDate = BogusStamp;
			dept.DeletedDate = BogusStamp;

			return (dept, (Department d, StampWindow window) =>
			{
				//rule 1 - the generated id replaces whatever the caller assigned
				d.Id.ShouldNotBe(default);
				d.Id.ShouldNotBe(UnstoredId);

				//rule 1 and rule 18 - CreatedDate is DateTime.UtcNow read during the Insert, not carried in
				d.CreatedDate.ShouldNotBe(BogusStamp);
				window.ShouldContainStamp(d.CreatedDate);

				//rule 1 - the other two are cleared whatever the caller assigned
				d.UpdatedDate.ShouldBeNull();
				d.DeletedDate.ShouldBeNull();
			}
			);
		}

		[Fact]
		public void ShouldInsertDepartment()
		{
			//setup
			var test = Setup_CreateDepartment_TestInsert();

			//act - the clock is read either side of the call, so rule 18's stamp is bounded by the call itself
			var window = StampWindow.Around(() => _da.Insert(test.Department));

			//assert
			test.Assert(test.Department, window);
		}

		#endregion

		#region Get - rules 8, 17

		public delegate void GetAssertion(Department dept);
		public static (int DepartmentId, GetAssertion Assertion) Setup_InsertDepartment_TestGet(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));

			return (dept.Id, (Department found) =>
			{
				//rule 17 - assert on what Get returned, never on the argument handed to it
				found.ShouldNotBeNull();
				found.Id.ShouldBe(dept.Id);
				found.Name.ShouldBe(dept.Name);
				found.Description.ShouldBe(dept.Description);
				found.DeletedDate.ShouldBeNull();

				//rule 18 - the stamp survives the round trip as UTC, and is still the one Insert wrote
				found.CreatedDate.ShouldBe(dept.CreatedDate);
				insertWindow.ShouldContainStamp(found.CreatedDate);
			}
			);
		}

		[Fact]
		public void ShouldGetDepartment()
		{
			//setup
			var test = Setup_InsertDepartment_TestGet(_da);

			//act
			var found = _da.Get(new Department { Id = test.DepartmentId });

			//assert
			test.Assertion(found);
		}

		public delegate void SoftDeletedGetAssertion(Department dept);
		public static (int DepartmentId, SoftDeletedGetAssertion Assertion) Setup_DeleteDepartment_TestGetStillFindsIt(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));
			var deleteWindow = StampWindow.Around(() => da.Delete(dept), out int _);

			return (dept.Id, (Department found) =>
			{
				//rule 8 - a soft-deleted department is still retrievable, and says so
				found.ShouldNotBeNull();
				found.Id.ShouldBe(dept.Id);
				found.Name.ShouldBe(dept.Name);
				found.DeletedDate.ShouldNotBeNull();

				//rule 18 - both stamps come back as UTC, each from the call that wrote it
				insertWindow.ShouldContainStamp(found.CreatedDate);
				deleteWindow.ShouldContainStamp(found.DeletedDate);
			}
			);
		}

		[Fact]
		public void ShouldGetSoftDeletedDepartment()
		{
			//setup
			var test = Setup_DeleteDepartment_TestGetStillFindsIt(_da);

			//act - a fresh instance carrying nothing but the id, so nothing survives from before the delete
			var found = _da.Get(new Department { Id = test.DepartmentId });

			//assert
			test.Assertion(found);
		}

		[Fact]
		public void ShouldGetNullWhenDepartmentWasNeverStored()
		{
			//setup - no precondition: the point is an id that matches nothing

			//act
			var found = _da.Get(new Department { Id = UnstoredId });

			//assert - rule 8, null means "never stored", not "deleted"
			found.ShouldBeNull();
		}

		#endregion

		#region Update - rules 2, 3, 4

		public delegate void UpdateAssertion(int count, StampWindow window);
		public static (Department Department, UpdateAssertion Assert) Setup_InsertDepartment_TestUpdate(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));
			var createdDate = dept.CreatedDate;

			var edit = da.Get(new Department { Id = dept.Id });
			edit.Description = "Edited text, after the insert has completed.";

			return (edit, (count, window) =>
			{
				count.ShouldBe(1);

				var stored = da.Get(new Department { Id = dept.Id });
				stored.Id.ShouldBe(dept.Id);
				stored.Description.ShouldBe("Edited text, after the insert has completed.");
				stored.Name.ShouldBe(dept.Name);

				//rule 2 - Update owns UpdatedDate, and stamps it back onto the instance it was handed
				//rule 18 - and the stamp is DateTime.UtcNow read during that call, on both instances
				window.ShouldContainStamp(stored.UpdatedDate);
				window.ShouldContainStamp(edit.UpdatedDate);

				//rule 3 - CreatedDate survives untouched, and the department is still live
				stored.CreatedDate.ShouldBe(createdDate);
				insertWindow.ShouldContainStamp(stored.CreatedDate);
				stored.DeletedDate.ShouldBeNull();
			}
			);
		}

		[Fact]
		public void ShouldUpdateDepartment()
		{
			//setup
			var test = Setup_InsertDepartment_TestUpdate(_da);

			//act
			var window = StampWindow.Around(() => _da.Update(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		/// <summary>
		/// Rule 3 on a department that is still live. The soft-deleted case below cannot carry this on its own:
		/// there, <see cref="Department.DeletedDate"/> is what catches a whole-object replacement, and on a live
		/// row that value is <c>null</c> whether the Data Access Layer preserved the stored one or overwrote the
		/// record wholesale. <see cref="Department.CreatedDate"/> is the only field left that can tell the two
		/// apart, so this is the test that has to do it - and whole-object replacement is the house pattern in
		/// this repository, so it is what an implementer reaches for first.
		/// </summary>
		public delegate void LivePreservationAssertion(int count, StampWindow window);
		public static (Department Department, LivePreservationAssertion Assert) Setup_InsertDepartment_TestUpdatePreservesStoredCreatedDate(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));
			var createdDate = dept.CreatedDate;

			//A live row, fetched exactly as a caller would fetch it, then poisoned in the one stamp Update does
			//not own. Everything else about it is legitimate.
			var edit = da.Get(new Department { Id = dept.Id });
			edit.Description = "Edited from a live instance carrying a bogus CreatedDate.";
			edit.CreatedDate = BogusStamp;

			return (edit, (count, window) =>
			{
				count.ShouldBe(1);

				var stored = da.Get(new Department { Id = dept.Id });

				//rule 3 - the department's own data is written
				stored.Description.ShouldBe("Edited from a live instance carrying a bogus CreatedDate.");

				//rule 3 - and the stored CreatedDate is the one Insert stamped, not the one carried in. A Data
				//Access Layer that assigns the whole object fails here and nowhere else.
				stored.CreatedDate.ShouldNotBe(BogusStamp);
				stored.CreatedDate.ShouldBe(createdDate);
				insertWindow.ShouldContainStamp(stored.CreatedDate);

				//rule 2 - UpdatedDate moved, to a stamp read during this call
				window.ShouldContainStamp(stored.UpdatedDate);

				//rule 3 - and the department is still live
				stored.DeletedDate.ShouldBeNull();
			}
			);
		}

		[Fact]
		public void ShouldPreserveStoredCreatedDateWhenUpdatingALiveDepartment()
		{
			//setup
			var test = Setup_InsertDepartment_TestUpdatePreservesStoredCreatedDate(_da);

			//act
			var window = StampWindow.Around(() => _da.Update(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		/// <summary>
		/// Rule 3, and the reason it is written down. The obvious implementation of <c>Update</c> is
		/// whole-object replacement, which wipes <see cref="Department.DeletedDate"/> the moment a caller
		/// passes an instance it fetched before the delete. This is that caller.
		/// </summary>
		public delegate void PreservationAssertion(int count, StampWindow window);
		public static (Department Department, PreservationAssertion Assert) Setup_DeleteDepartment_TestUpdatePreservesStoredStamps(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));
			var createdDate = dept.CreatedDate;

			//A snapshot taken while the department was still live - exactly what a caller would be holding.
			var stale = da.Get(new Department { Id = dept.Id });

			var deleteWindow = StampWindow.Around(() => da.Delete(dept), out int _);
			var deletedDate = da.Get(new Department { Id = dept.Id }).DeletedDate;
			deleteWindow.ShouldContainStamp(deletedDate);

			//The stale instance still says "live" and carries junk in the two stamps Update does not own.
			stale.Description = "Edited from an instance fetched before the delete.";
			stale.CreatedDate = BogusStamp;
			stale.UpdatedDate = BogusStamp;
			stale.DeletedDate = null;

			return (stale, (count, window) =>
			{
				//rule 4 - updating a soft-deleted department is allowed
				count.ShouldBe(1);

				var stored = da.Get(new Department { Id = dept.Id });

				//rule 3 - the department's own data is written
				stored.Description.ShouldBe("Edited from an instance fetched before the delete.");

				//rule 3 - and the two stamps Update does not own come from the store, not from the argument
				stored.CreatedDate.ShouldBe(createdDate);
				insertWindow.ShouldContainStamp(stored.CreatedDate);
				stored.DeletedDate.ShouldBe(deletedDate);
				deleteWindow.ShouldContainStamp(stored.DeletedDate);

				//rule 2 - UpdatedDate is a fresh stamp read during the Update, not the value the caller carried in
				stored.UpdatedDate.Value.ShouldNotBe(BogusStamp);
				window.ShouldContainStamp(stored.UpdatedDate);

				//rule 4 - the department stays deleted, so it stays out of the live views
				da.GetAll(new Department()).Any(x => x.Id == dept.Id).ShouldBeFalse();
				da.GetCount(new Department()).ShouldBe(da.GetAll(new Department()).Count);
			}
			);
		}

		[Fact]
		public void ShouldPreserveStoredCreatedAndDeletedDatesWhenUpdatingASoftDeletedDepartment()
		{
			//setup
			var test = Setup_DeleteDepartment_TestUpdatePreservesStoredStamps(_da);

			//act
			var window = StampWindow.Around(() => _da.Update(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		[Fact]
		public void ShouldRestampUpdatedDateOnEveryUpdate()
		{
			//setup
			var test = Setup_InsertDepartment_TestUpdate(_da);
			_da.Update(test.Department);
			var firstStamp = _da.Get(new Department { Id = test.Department.Id }).UpdatedDate;

			//the clock has to move, or "restamped" and "left alone" look identical
			Thread.Sleep(ClockTickMs);
			test.Department.Description = "Edited a second time.";
			test.Department.UpdatedDate = BogusStamp;

			//act
			var window = StampWindow.Around(() => _da.Update(test.Department), out int count);

			//assert - rule 2, repeated updates overwrite the stamp
			count.ShouldBe(1);
			var secondStamp = _da.Get(new Department { Id = test.Department.Id }).UpdatedDate;
			StampWindow.ShouldBeUtcStamp(firstStamp);
			window.ShouldContainStamp(secondStamp);
			secondStamp.Value.ShouldBeGreaterThan(firstStamp.Value);
			secondStamp.Value.ShouldNotBe(BogusStamp);
		}

		[Fact]
		public void ShouldNotUpdateDepartmentThatWasNeverStored()
		{
			//setup - no precondition: the point is an id that matches nothing
			var phantom = NewDepartment;
			phantom.Id = UnstoredId;

			//act
			var count = _da.Update(phantom);

			//assert - rule 2, nothing matched so nothing changed
			count.ShouldBe(0);
			_da.Get(new Department { Id = UnstoredId }).ShouldBeNull();
		}

		#endregion

		#region Delete - rules 5, 6

		public delegate void DeleteAssertion(int count, StampWindow window);
		public static (Department Department, DeleteAssertion Assert) Setup_InsertDepartment_TestDelete(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));
			var createdDate = dept.CreatedDate;

			return (dept, (count, window) =>
			{
				count.ShouldBe(1);

				//rule 5 - the stamp is written back onto the instance handed to Delete
				//rule 18 - and it is DateTime.UtcNow read during that call
				window.ShouldContainStamp(dept.DeletedDate);

				//rule 5 - the row is not removed, and the other two stamps are not touched
				var stored = da.Get(new Department { Id = dept.Id });
				stored.ShouldNotBeNull();
				window.ShouldContainStamp(stored.DeletedDate);
				stored.CreatedDate.ShouldBe(createdDate);
				insertWindow.ShouldContainStamp(stored.CreatedDate);
				stored.UpdatedDate.ShouldBeNull();

				//rule 9 - and it is gone from every live view
				var all = da.GetAll(new Department());
				all.Any(x => x.Id == dept.Id).ShouldBeFalse();
				da.GetCount(new Department()).ShouldBe(all.Count);
				da.GetPaged(new Department(), 0, all.Count).Any(x => x.Id == dept.Id).ShouldBeFalse();
			}
			);
		}

		[Fact]
		public void ShouldSoftDeleteDepartment()
		{
			//setup
			var test = Setup_InsertDepartment_TestDelete(_da);

			//act
			var window = StampWindow.Around(() => _da.Delete(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		public delegate void SecondDeleteAssertion(int count);
		public static (Department Department, SecondDeleteAssertion Assert) Setup_DeleteDepartment_TestSecondDelete(IDepartmentDao da)
		{
			var dept = NewDepartment;
			da.Insert(dept);
			var deleteWindow = StampWindow.Around(() => da.Delete(dept), out int _);
			var deletedDate = da.Get(new Department { Id = dept.Id }).DeletedDate;
			deleteWindow.ShouldContainStamp(deletedDate);

			//Let the clock move, so a refreshed stamp would be a different stamp. This is DateTime resolution,
			//not test isolation - on net48 two calls inside one clock step read the same value, and a Delete that
			//wrongly re-stamped would then be indistinguishable from one that correctly did nothing.
			Thread.Sleep(ClockTickMs);

			return (dept, (count) =>
			{
				//rule 6 - already deleted, so nothing changed
				count.ShouldBe(0);

				//rule 6 - and the original stamp still reports when the department was actually deleted
				var stored = da.Get(new Department { Id = dept.Id });
				stored.DeletedDate.ShouldBe(deletedDate);
				deleteWindow.ShouldContainStamp(stored.DeletedDate);
			}
			);
		}

		[Fact]
		public void ShouldNotRefreshDeletedDateOnASecondDelete()
		{
			//setup
			var test = Setup_DeleteDepartment_TestSecondDelete(_da);

			//act
			var count = _da.Delete(test.Department);

			//assert
			test.Assert(count);
		}

		[Fact]
		public void ShouldNotDeleteDepartmentThatWasNeverStored()
		{
			//setup - no precondition: the point is an id that matches nothing
			var phantom = NewDepartment;
			phantom.Id = UnstoredId;

			//act
			var count = _da.Delete(phantom);

			//assert - rule 6
			count.ShouldBe(0);
			_da.Get(new Department { Id = UnstoredId }).ShouldBeNull();
		}

		#endregion

		#region Restore - rule 7

		public delegate void RestoreAssertion(int count);
		public static (Department Department, RestoreAssertion Assert) Setup_DeleteDepartment_TestRestore(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));
			var createdDate = dept.CreatedDate;
			da.Delete(dept);

			return (dept, (count) =>
			{
				count.ShouldBe(1);

				//rule 7 - the cleared value is written back onto the instance handed to Restore
				dept.DeletedDate.ShouldBeNull();

				var stored = da.Get(new Department { Id = dept.Id });
				stored.DeletedDate.ShouldBeNull();

				//a restore is a lifecycle change, not a modification - it stamps nothing else
				stored.UpdatedDate.ShouldBeNull();
				stored.CreatedDate.ShouldBe(createdDate);
				insertWindow.ShouldContainStamp(stored.CreatedDate);

				//and the department is back in the live views
				var all = da.GetAll(new Department());
				all.Any(x => x.Id == dept.Id).ShouldBeTrue();
				da.GetCount(new Department()).ShouldBe(all.Count);
			}
			);
		}

		[Fact]
		public void ShouldRestoreSoftDeletedDepartment()
		{
			//setup
			var test = Setup_DeleteDepartment_TestRestore(_da);

			//act
			var count = _da.Restore(test.Department);

			//assert
			test.Assert(count);
		}

		[Fact]
		public void ShouldNotRestoreDepartmentThatIsAlreadyLive()
		{
			//setup
			var test = Setup_InsertDepartment_TestGet(_da);
			var live = new Department { Id = test.DepartmentId };

			//act
			var count = _da.Restore(live);

			//assert - rule 7, restoring a live department is a no-op
			count.ShouldBe(0);
			var stored = _da.Get(new Department { Id = test.DepartmentId });
			stored.DeletedDate.ShouldBeNull();
			stored.UpdatedDate.ShouldBeNull();
		}

		[Fact]
		public void ShouldNotRestoreDepartmentThatWasNeverStored()
		{
			//setup - no precondition: the point is an id that matches nothing

			//act
			var count = _da.Restore(new Department { Id = UnstoredId });

			//assert - rule 7
			count.ShouldBe(0);
			_da.Get(new Department { Id = UnstoredId }).ShouldBeNull();
		}

		#endregion

		#region GetAll, GetCount - rules 9, 10, 15

		public delegate void GetAllAssertion(IList<Department> all, int count);
		public static (int LiveId, int DeletedId, GetAllAssertion Assert) Setup_LiveAndDeletedDepartments_TestGetAll(IDepartmentDao da)
		{
			var live = NewDepartment;
			da.Insert(live);

			var deleted = NewDepartment;
			da.Insert(deleted);
			da.Delete(deleted);

			return (live.Id, deleted.Id, (all, count) =>
			{
				//rule 10 - always a list
				all.ShouldNotBeNull();

				//rule 9 - live in, deleted out
				all.Any(x => x.Id == live.Id).ShouldBeTrue();
				all.Any(x => x.Id == deleted.Id).ShouldBeFalse();
				all.All(x => x.DeletedDate == null).ShouldBeTrue();

				//rule 9 - GetCount agrees with GetAll exactly
				count.ShouldBe(all.Count);
			}
			);
		}

		[Fact]
		public void ShouldGetAllLiveDepartmentsAndAgreeWithGetCount()
		{
			//setup
			var test = Setup_LiveAndDeletedDepartments_TestGetAll(_da);

			//act
			var all = _da.GetAll(new Department());
			var count = _da.GetCount(new Department());

			//assert
			test.Assert(all, count);
		}

		public delegate void EmptyViewAssertion(IList<Department> all, IList<Department> paged, int count);
		public static EmptyViewAssertion Setup_DeleteEveryDepartment_TestEmptyViews(IDepartmentDao da)
		{
			//Give the delete something to bite on, then clear the live set. Every other test in this class
			//creates the departments it needs, so leaving no live rows behind breaks nothing.
			da.Insert(NewDepartment);
			da.Insert(NewDepartment);

			foreach (var dept in da.GetAll(new Department()).ToList())
				da.Delete(dept);

			return (all, paged, count) =>
			{
				//rule 9 and rule 10 - empty, and still a list
				all.ShouldNotBeNull();
				all.ShouldBeEmpty();
				paged.ShouldNotBeNull();
				paged.ShouldBeEmpty();
				count.ShouldBe(0);
			};
		}

		[Fact]
		public void ShouldReturnEmptyViewsWhenEveryDepartmentIsDeleted()
		{
			//setup
			var assertion = Setup_DeleteEveryDepartment_TestEmptyViews(_da);

			//act
			var all = _da.GetAll(new Department());
			var paged = _da.GetPaged(new Department(), 0, 10);
			var count = _da.GetCount(new Department());

			//assert
			assertion(all, paged, count);
		}

		public delegate void OnlyExclusionAssertion(IList<Department> all, int count);
		public static (int DepartmentId, OnlyExclusionAssertion Assert) Setup_DepartmentWithEmptyData_TestItIsStillReturned(IDepartmentDao da)
		{
			//No name, no description. The Data Access Layer validates nothing, so this is a valid department.
			var dept = new Department { Name = null, Description = null };
			da.Insert(dept);

			return (dept.Id, (all, count) =>
			{
				//rule 15 - soft-deletion is the only reason a department is ever filtered out
				all.Any(x => x.Id == dept.Id).ShouldBeTrue();
				count.ShouldBe(all.Count);
			}
			);
		}

		[Fact]
		public void ShouldReturnDepartmentWithNoNameOrDescription()
		{
			//setup
			var test = Setup_DepartmentWithEmptyData_TestItIsStillReturned(_da);

			//act
			var all = _da.GetAll(new Department());
			var count = _da.GetCount(new Department());

			//assert
			test.Assert(all, count);
		}

		#endregion

		#region GetPaged - rules 11, 12

		public delegate void PagingAssertion(IList<Department> all, IList<Department> pagedTogether);
		public static (IList<int> InsertedIds, PagingAssertion Assert) Setup_InsertDepartments_TestPagingPartitionsGetAll(IDepartmentDao da)
		{
			var inserted = new List<int>();
			for (var i = 0; i < 5; i++)
			{
				var dept = NewDepartment;
				da.Insert(dept);
				inserted.Add(dept.Id);
			}

			return (inserted, (all, pagedTogether) =>
			{
				foreach (var id in inserted)
					all.Any(x => x.Id == id).ShouldBeTrue();

				//rule 11 - successive windows partition the GetAll set: nothing omitted, nothing twice
				pagedTogether.Count.ShouldBe(all.Count);
				pagedTogether.Select(x => x.Id).Distinct().Count().ShouldBe(all.Count);
				pagedTogether.Select(x => x.Id).OrderBy(x => x).ShouldBe(all.Select(x => x.Id).OrderBy(x => x));
			}
			);
		}

		[Fact]
		public void ShouldPartitionGetAllAcrossSuccessiveGetPagedWindows()
		{
			//setup
			var test = Setup_InsertDepartments_TestPagingPartitionsGetAll(_da);

			//act
			var all = _da.GetAll(new Department());
			var pagedTogether = new List<Department>();
			for (var skip = 0; skip < all.Count; skip += 2)
				pagedTogether.AddRange(_da.GetPaged(new Department(), skip, 2));

			//assert
			test.Assert(all, pagedTogether);
		}

		[Fact]
		public void ShouldReturnTheSameOrderOnRepeatedCalls()
		{
			//setup
			var test = Setup_InsertDepartments_TestPagingPartitionsGetAll(_da);
			test.InsertedIds.Count.ShouldBe(5);

			//act
			var first = _da.GetPaged(new Department(), 0, 3);
			var second = _da.GetPaged(new Department(), 0, 3);

			//assert - rule 11, ordering is unspecified but stable while the data is unchanged
			second.Select(x => x.Id).ShouldBe(first.Select(x => x.Id));
		}

		[Fact]
		public void ShouldReturnEmptyPageWhenSkipIsBeyondTheAvailableCount()
		{
			//setup
			Setup_InsertDepartments_TestPagingPartitionsGetAll(_da);

			//act
			var page = _da.GetPaged(new Department(), _da.GetCount(new Department()) + 10, 5);

			//assert - rule 12
			page.ShouldNotBeNull();
			page.ShouldBeEmpty();
		}

		[Fact]
		public void ShouldReturnEmptyPageWhenTakeIsZero()
		{
			//setup
			Setup_InsertDepartments_TestPagingPartitionsGetAll(_da);

			//act
			var page = _da.GetPaged(new Department(), 0, 0);

			//assert - rule 12
			page.ShouldNotBeNull();
			page.ShouldBeEmpty();
		}

		[Fact]
		public void ShouldReturnTheRemainderWhenTakeExceedsWhatIsLeft()
		{
			//setup
			Setup_InsertDepartments_TestPagingPartitionsGetAll(_da);
			var count = _da.GetCount(new Department());

			//act - ask for far more than the one record that is left
			var page = _da.GetPaged(new Department(), count - 1, count + 10);

			//assert - rule 12, the remainder rather than a throw or a padded page
			page.Count.ShouldBe(1);
		}

		[Theory]
		[InlineData(-1, 1)]
		[InlineData(0, -1)]
		[InlineData(-1, -1)]
		public void ShouldThrowWhenGetPagedIsGivenANegativeBound(int skip, int take)
		{
			//setup - no precondition: the bounds are rejected before any data is read

			//act & assert - rule 12
			Should.Throw<ArgumentOutOfRangeException>(() =>
			{
				_da.GetPaged(new Department(), skip, take);
			});
		}

		#endregion

		#region Null arguments - rules 13, 14

		[Fact]
		public void ShouldThrowWhenGetIsGivenNull()
		{
			//act & assert - rule 14
			Should.Throw<ArgumentNullException>(() =>
			{
				_da.Get(null);
			});
		}

		[Fact]
		public void ShouldThrowWhenInsertIsGivenNull()
		{
			//act & assert - rule 14
			Should.Throw<ArgumentNullException>(() => _da.Insert(null));
		}

		[Fact]
		public void ShouldThrowWhenUpdateIsGivenNull()
		{
			//act & assert - rule 14
			Should.Throw<ArgumentNullException>(() =>
			{
				_da.Update(null);
			});
		}

		[Fact]
		public void ShouldThrowWhenDeleteIsGivenNull()
		{
			//act & assert - rule 14
			Should.Throw<ArgumentNullException>(() =>
			{
				_da.Delete(null);
			});
		}

		[Fact]
		public void ShouldThrowWhenRestoreIsGivenNull()
		{
			//act & assert - rule 14
			Should.Throw<ArgumentNullException>(() =>
			{
				_da.Restore(null);
			});
		}

		[Fact]
		public void ShouldNotThrowWhenGetAllIsGivenNull()
		{
			//setup
			Setup_InsertDepartment_TestGet(_da);

			//act - rule 13, this is exactly how the generic dispatcher calls it
			var all = _da.GetAll(null);

			//assert
			all.ShouldNotBeNull();
		}

		[Fact]
		public void ShouldNotThrowWhenGetPagedIsGivenNull()
		{
			//setup
			Setup_InsertDepartment_TestGet(_da);

			//act - rule 13
			var page = _da.GetPaged(null, 0, 1);

			//assert
			page.ShouldNotBeNull();
		}

		[Fact]
		public void ShouldNotThrowWhenGetCountIsGivenNull()
		{
			//setup
			Setup_InsertDepartment_TestGet(_da);

			//act - rule 13
			var count = _da.GetCount(null);

			//assert
			count.ShouldBeGreaterThanOrEqualTo(1);
		}

		#endregion
	}
}
