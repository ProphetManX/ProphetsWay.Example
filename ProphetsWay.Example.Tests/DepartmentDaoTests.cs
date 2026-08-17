using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
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
	[Collection(TestCollections.SharedStore)]
	[Trait("Scope", "Contract")]
	public class DepartmentDaoTests : BaseUnitTests<IDepartmentDao>
	{
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
		public static (Department Department, InsertAssertion Assert) Setup_CreateDepartment_TestInsert(IDepartmentDao da)
		{
			//Every field Insert owns is pre-loaded with a value Insert must overwrite.
			var dept = NewDepartment;
			dept.Id = UnstoredId;
			dept.CreatedDate = BogusStamp;
			dept.UpdatedDate = BogusStamp;
			dept.DeletedDate = BogusStamp;

			var name = dept.Name;
			var description = dept.Description;

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

				//The write-back is only half of the rule. Everything above inspects the instance handed in, and an
				//Insert that stamps its argument and persists nothing passes all of it - so fetch the department
				//back and prove the row is really there.
				var stored = da.Get(new Department { Id = d.Id });
				stored.ShouldNotBeNull();
				stored.Id.ShouldBe(d.Id);
				stored.Name.ShouldBe(name);
				stored.Description.ShouldBe(description);
				stored.CreatedDate.ShouldBe(d.CreatedDate);
				window.ShouldContainStamp(stored.CreatedDate);
				stored.UpdatedDate.ShouldBeNull();
				stored.DeletedDate.ShouldBeNull();

				//rule 9 - a newly inserted department is live, so it is in the live views
				da.GetAll(new Department()).Any(x => x.Id == d.Id).ShouldBeTrue();
			}
			);
		}

		[Fact]
		public void ShouldInsertDepartment()
		{
			//setup
			var test = Setup_CreateDepartment_TestInsert(_da);

			//act - the clock is read either side of the call, so rule 18's stamp is bounded by the call itself
			var window = StampWindow.Around(() => _da.Insert(test.Department));

			//assert
			test.Assert(test.Department, window);
		}

		/// <summary>
		/// Rule 1's word <i>generated</i>, which nothing else in this class pins down. Every other insert
		/// assertion is satisfied by an implementation that assigns the same constant every time, and the
		/// resulting collision only surfaces later as a department that has mysteriously become another one.
		/// </summary>
		public delegate void DistinctIdAssertion(Department first, Department second);
		public static (Department First, Department Second, DistinctIdAssertion Assert) Setup_CreateTwoDepartments_TestDistinctIds(IDepartmentDao da)
		{
			var first = NewDepartment;
			var second = NewDepartment;

			return (first, second, (a, b) =>
			{
				//rule 1 - each Insert generates an identifier of its own
				a.Id.ShouldNotBe(default);
				b.Id.ShouldNotBe(default);
				b.Id.ShouldNotBe(a.Id);

				//and the two identifiers address two different rows, rather than one row twice
				da.Get(new Department { Id = a.Id }).Name.ShouldBe(a.Name);
				da.Get(new Department { Id = b.Id }).Name.ShouldBe(b.Name);
			}
			);
		}

		[Fact]
		public void ShouldAssignADistinctIdToEachInsertedDepartment()
		{
			//setup
			var test = Setup_CreateTwoDepartments_TestDistinctIds(_da);

			//act
			_da.Insert(test.First);
			_da.Insert(test.Second);

			//assert
			test.Assert(test.First, test.Second);
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

		#region Snapshots - rule 19

		/// <summary>
		/// Rule 19, stated on its own rather than leaned on. Every other assertion in this class that reads a
		/// department back is only as strong as this one: if <c>Get</c> hands out the store's own instance,
		/// then a <c>Delete</c> that stamps its argument and writes nothing to the store passes, because the
		/// assertion reads back the very object the call just mutated. Nothing is passed to <c>Update</c> here
		/// - the whole point is that an edit nobody submitted cannot reach the store.
		/// </summary>
		public delegate void SnapshotAssertion(Department refetched, IList<Department> all);
		public static (int DepartmentId, SnapshotAssertion Assert) Setup_InsertDepartment_TestRetrievedInstanceIsASnapshot(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var insertWindow = StampWindow.Around(() => da.Insert(dept));
			var name = dept.Name;
			var description = dept.Description;
			var createdDate = dept.CreatedDate;

			//A caller fetches a department, edits every field it can reach - including the two stamps it has no
			//business setting - and never calls Update.
			var edited = da.Get(new Department { Id = dept.Id });
			edited.Name = "Renamed on a retrieved instance.";
			edited.Description = "Edited on a retrieved instance that was never handed back to Update.";
			edited.CreatedDate = BogusStamp;
			edited.UpdatedDate = BogusStamp;
			edited.DeletedDate = BogusStamp;

			return (dept.Id, (refetched, all) =>
			{
				//rule 19 - two separately retrieved instances are independent of each other, which a Data
				//Access Layer handing out its own object cannot be
				refetched.ShouldNotBeNull();
				refetched.ShouldNotBeSameAs(edited);

				//rule 19 - and none of the edits above reached the store
				refetched.Name.ShouldBe(name);
				refetched.Description.ShouldBe(description);
				refetched.CreatedDate.ShouldNotBe(BogusStamp);
				refetched.CreatedDate.ShouldBe(createdDate);
				insertWindow.ShouldContainStamp(refetched.CreatedDate);
				refetched.UpdatedDate.ShouldBeNull();

				//rule 19 - in particular, assigning DeletedDate on a retrieved instance does not delete anything
				refetched.DeletedDate.ShouldBeNull();
				all.Any(x => x.Id == dept.Id).ShouldBeTrue();
			}
			);
		}

		[Fact]
		public void ShouldNotStoreEditsMadeToARetrievedDepartment()
		{
			//setup
			var test = Setup_InsertDepartment_TestRetrievedInstanceIsASnapshot(_da);

			//act - no write of any kind happens between the setup's edit and these reads
			var refetched = _da.Get(new Department { Id = test.DepartmentId });
			var all = _da.GetAll(new Department());

			//assert
			test.Assert(refetched, all);
		}

		/// <summary>
		/// Every field a caller can reach, rewritten on an instance the caller has <i>already</i> handed to the
		/// Data Access Layer. Under rule 19 none of it may reach the store, because the call it was an argument to
		/// has already returned. <see cref="Entities.BaseIntEntity.Id"/> is left alone so that the assertion still
		/// has an identifier to fetch the row back by.
		/// </summary>
		private static void EditEveryFieldAfterTheCall(Department dept)
		{
			dept.Name = "Renamed after the call returned.";
			dept.Description = "Edited after the call returned.";
			dept.CreatedDate = BogusStamp;
			dept.UpdatedDate = BogusStamp;
			dept.DeletedDate = BogusStamp;
		}

		/// <summary>
		/// Rule 19's other half, on <c>Insert</c>. The snapshot test above proves a department coming <i>out</i> of
		/// the Data Access Layer is a copy; this one proves a department going <i>in</i> is read rather than
		/// adopted. A Data Access Layer that clones on read and then stores the caller's own instance passes every
		/// other test in this class - it is the natural half-measure of an implementer who adds copying to
		/// <c>Get</c> and stops there - and it silently restores the aliasing that rule 19 exists to forbid.
		/// </summary>
		public delegate void InsertAdoptionAssertion(int departmentId);
		public static (Department Department, InsertAdoptionAssertion Assert) Setup_CreateDepartment_TestInsertDoesNotAdoptTheArgument(IDepartmentDao da)
		{
			var dept = NewDepartment;
			var name = dept.Name;
			var description = dept.Description;

			return (dept, (departmentId) =>
			{
				var stored = da.Get(new Department { Id = departmentId });
				stored.ShouldNotBeNull();

				//rule 19 - Insert read the values as they stood at the moment of the call, and the rewrite that
				//followed it reached nothing
				stored.Name.ShouldBe(name);
				stored.Description.ShouldBe(description);

				//rule 19 - including the three stamps, so CreatedDate is still the one Insert stamped and the
				//department has been neither updated nor deleted by an edit nobody submitted
				stored.CreatedDate.ShouldNotBe(BogusStamp);
				StampWindow.ShouldBeUtcStamp(stored.CreatedDate);
				stored.UpdatedDate.ShouldBeNull();
				stored.DeletedDate.ShouldBeNull();

				//rule 9 - and "still live" is asserted where it is observable, not only on the stamp
				da.GetAll(new Department()).Any(x => x.Id == departmentId).ShouldBeTrue();
			}
			);
		}

		[Fact]
		public void ShouldNotStoreEditsMadeToADepartmentAfterInsertReturned()
		{
			//setup
			var test = Setup_CreateDepartment_TestInsertDoesNotAdoptTheArgument(_da);

			//act - the identifier is captured before the rewrite, because after it the instance no longer
			//describes anything the store holds
			_da.Insert(test.Department);
			var departmentId = test.Department.Id;
			EditEveryFieldAfterTheCall(test.Department);

			//assert
			test.Assert(departmentId);
		}

		/// <summary>
		/// Rule 19's write half on <c>Update</c>, which is where adopting the argument does the most damage: the
		/// instance a caller hands to <c>Update</c> is usually one it keeps working with afterwards, so a store
		/// holding that reference goes on absorbing edits nobody submitted for as long as the caller holds it.
		/// </summary>
		public delegate void UpdateAdoptionAssertion(int count);
		public static (Department Department, UpdateAdoptionAssertion Assert) Setup_InsertDepartment_TestUpdateDoesNotAdoptTheArgument(IDepartmentDao da)
		{
			var dept = NewDepartment;
			da.Insert(dept);
			var createdDate = dept.CreatedDate;

			//An ordinary fetch-edit-submit, so that the Update has a legitimate change to land. Only what the
			//caller does to this instance after the call returns is the point.
			var edit = da.Get(new Department { Id = dept.Id });
			edit.Description = "Edited before the Update, which is the edit that is supposed to land.";

			return (edit, (count) =>
			{
				count.ShouldBe(1);

				var stored = da.Get(new Department { Id = dept.Id });

				//rule 2 - the edit submitted before the call did land, so "nothing changed" is not how this passes
				stored.Description.ShouldBe("Edited before the Update, which is the edit that is supposed to land.");
				StampWindow.ShouldBeUtcStamp(stored.UpdatedDate);
				stored.UpdatedDate.Value.ShouldNotBe(BogusStamp);

				//rule 19 - and none of the rewriting done after the call did
				stored.Name.ShouldBe(dept.Name);
				stored.CreatedDate.ShouldBe(createdDate);
				stored.DeletedDate.ShouldBeNull();
				da.GetAll(new Department()).Any(x => x.Id == dept.Id).ShouldBeTrue();
			}
			);
		}

		[Fact]
		public void ShouldNotStoreEditsMadeToADepartmentAfterUpdateReturned()
		{
			//setup
			var test = Setup_InsertDepartment_TestUpdateDoesNotAdoptTheArgument(_da);

			//act
			var count = _da.Update(test.Department);
			EditEveryFieldAfterTheCall(test.Department);

			//assert
			test.Assert(count);
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
		/// <remarks>
		/// Rule 19 is what makes this setup mean anything. The instance being poisoned came out of <c>Get</c>, so
		/// under a Data Access Layer that handed back its own object the store would already carry
		/// <see cref="BogusStamp"/> before <c>Update</c> was ever called, and the test would be asserting against
		/// damage it caused itself.
		/// </remarks>
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
		/// <remarks>
		/// Rule 19 is what makes this setup mean anything - see
		/// <see cref="Setup_InsertDepartment_TestUpdatePreservesStoredCreatedDate"/>. The stale instance is
		/// fetched while the department is live and then edited; only because a retrieved instance is a snapshot
		/// does the store still hold the delete stamp by the time <c>Update</c> runs.
		/// </remarks>
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

		/// <summary>
		/// Rule 3 in the direction the setup above cannot reach. That one poisons
		/// <see cref="Department.DeletedDate"/> with <c>null</c> on a row that is deleted; this one poisons it
		/// with a value on a row that is live. Both cells have to be filled, because an implementation written as
		/// <c>stored.DeletedDate = item.DeletedDate ?? stored.DeletedDate;</c> satisfies the first and turns
		/// <c>Update</c> into a second, undocumented way to soft-delete a department - with whatever junk stamp
		/// the caller happened to be carrying, and bypassing <c>Delete</c> entirely.
		/// </summary>
		public delegate void NoDeleteThroughUpdateAssertion(int count, StampWindow window);
		public static (Department Department, NoDeleteThroughUpdateAssertion Assert) Setup_InsertDepartment_TestUpdateCannotSoftDeleteALiveDepartment(IDepartmentDao da)
		{
			var dept = NewDepartment;
			da.Insert(dept);

			//A live row, fetched exactly as a caller would fetch it, then poisoned in the one stamp only Delete
			//may write.
			var edit = da.Get(new Department { Id = dept.Id });
			edit.Description = "Edited from a live instance carrying a bogus DeletedDate.";
			edit.DeletedDate = BogusStamp;

			return (edit, (count, window) =>
			{
				count.ShouldBe(1);

				var stored = da.Get(new Department { Id = dept.Id });

				//rule 3 - the department's own data is written
				stored.Description.ShouldBe("Edited from a live instance carrying a bogus DeletedDate.");

				//rule 2 - and the update itself really happened, so "nothing changed" is not how this passes
				window.ShouldContainStamp(stored.UpdatedDate);

				//rule 3 - an incoming DeletedDate is ignored in this direction too. Delete is the only way in.
				stored.DeletedDate.ShouldBeNull();

				//rule 9 - and "still live" is asserted where it is observable, not only on the stamp
				var all = da.GetAll(new Department());
				all.Any(x => x.Id == dept.Id).ShouldBeTrue();
				da.GetCount(new Department()).ShouldBe(all.Count);
				da.GetPaged(new Department(), 0, all.Count).Any(x => x.Id == dept.Id).ShouldBeTrue();
			}
			);
		}

		[Fact]
		public void ShouldNotSoftDeleteALiveDepartmentThroughUpdate()
		{
			//setup
			var test = Setup_InsertDepartment_TestUpdateCannotSoftDeleteALiveDepartment(_da);

			//act
			var window = StampWindow.Around(() => _da.Update(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		/// <summary>
		/// The last cell of rule 3's matrix - {live, deleted} x {null, non-null} - and the only one no other test
		/// reaches. A deleted department updated with a bogus <i>value</i> in
		/// <see cref="Department.DeletedDate"/>: an implementation written as
		/// <c>if (item.DeletedDate.HasValue &amp;&amp; stored.DeletedDate.HasValue) stored.DeletedDate = item.DeletedDate;</c>
		/// satisfies the other three and turns <c>Update</c> into a way to rewrite a deletion timestamp, leaving
		/// the row lying about when it was deleted - the same harm rule 6's "is not refreshed" forbids, arriving
		/// through a different door.
		/// </summary>
		public delegate void DeletedStampPreservationAssertion(int count, StampWindow window);
		public static (Department Department, DeletedStampPreservationAssertion Assert) Setup_DeleteDepartment_TestUpdateCannotRewriteTheDeletedDate(IDepartmentDao da)
		{
			var dept = NewDepartment;
			da.Insert(dept);
			var deleteWindow = StampWindow.Around(() => da.Delete(dept), out int _);
			var deletedDate = da.Get(new Department { Id = dept.Id }).DeletedDate;
			deleteWindow.ShouldContainStamp(deletedDate);

			//A deleted row, fetched exactly as a caller would fetch it, then poisoned in the one stamp only Delete
			//may write - with a value rather than with null, which is the direction nothing else covers.
			var edit = da.Get(new Department { Id = dept.Id });
			edit.Description = "Edited from a deleted instance carrying a bogus DeletedDate.";
			edit.DeletedDate = BogusStamp;

			return (edit, (count, window) =>
			{
				//rule 4 - updating a soft-deleted department is allowed
				count.ShouldBe(1);

				var stored = da.Get(new Department { Id = dept.Id });

				//rule 3 - the department's own data is written, so "nothing happened" is not how this passes
				stored.Description.ShouldBe("Edited from a deleted instance carrying a bogus DeletedDate.");

				//rule 2 - and the update itself really ran
				window.ShouldContainStamp(stored.UpdatedDate);

				//rule 3 - the incoming DeletedDate is ignored here too, so the row still reports when it was
				//actually deleted
				stored.DeletedDate.ShouldNotBe(BogusStamp);
				stored.DeletedDate.ShouldBe(deletedDate);
				deleteWindow.ShouldContainStamp(stored.DeletedDate);

				//rule 4 - and it stays deleted
				var all = da.GetAll(new Department());
				all.Any(x => x.Id == dept.Id).ShouldBeFalse();
				da.GetCount(new Department()).ShouldBe(all.Count);
			}
			);
		}

		[Fact]
		public void ShouldNotRewriteTheDeletedDateOfASoftDeletedDepartmentThroughUpdate()
		{
			//setup
			var test = Setup_DeleteDepartment_TestUpdateCannotRewriteTheDeletedDate(_da);

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

			//The department is modified before it is deleted, so that rule 5's "UpdatedDate is not touched" has
			//something to be true of. On a department that was never updated the field is null either way, and a
			//Delete written as a whole-object write - which erases modification history - passes unnoticed.
			dept.Description = "Edited before the delete, so the delete has a stamp to preserve.";
			da.Update(dept);
			var updatedDate = da.Get(new Department { Id = dept.Id }).UpdatedDate;
			StampWindow.ShouldBeUtcStamp(updatedDate);

			//Let the clock move, so a Delete that re-stamped UpdatedDate would write a different value than the
			//one captured above. Without this the two calls can read the same tick and the assertion cannot tell
			//"preserved" from "rewritten". Same DateTime resolution problem as the second-delete setup below.
			Thread.Sleep(ClockTickMs);

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
				stored.UpdatedDate.ShouldBe(updatedDate);
				stored.Description.ShouldBe("Edited before the delete, so the delete has a stamp to preserve.");

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

			//Modified before it is deleted, so that "Restore stamps nothing else" is asserted against a value
			//rather than against null. A Restore written as a whole-object write erases UpdatedDate, and on a
			//department that was never updated nothing notices.
			dept.Description = "Edited before the delete, so the restore has a stamp to preserve.";
			da.Update(dept);
			var updatedDate = da.Get(new Department { Id = dept.Id }).UpdatedDate;
			StampWindow.ShouldBeUtcStamp(updatedDate);

			da.Delete(dept);

			//Let the clock move, so a Restore that wrongly re-stamped UpdatedDate would write a value different
			//from the one captured above. DateTime resolution, not test isolation.
			Thread.Sleep(ClockTickMs);

			return (dept, (count) =>
			{
				count.ShouldBe(1);

				//rule 7 - the cleared value is written back onto the instance handed to Restore
				dept.DeletedDate.ShouldBeNull();

				var stored = da.Get(new Department { Id = dept.Id });
				stored.DeletedDate.ShouldBeNull();

				//a restore is a lifecycle change, not a modification - it stamps nothing else, and it erases
				//nothing either
				stored.UpdatedDate.ShouldBe(updatedDate);
				stored.CreatedDate.ShouldBe(createdDate);
				insertWindow.ShouldContainStamp(stored.CreatedDate);
				stored.Description.ShouldBe("Edited before the delete, so the restore has a stamp to preserve.");

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

				//rule 18 - the Kind survives this retrieval too, not only a Get. A projection into fresh
				//instances - the ordinary shape of a SQL-backed Data Access Layer - drops it here first.
				AssertEveryStampIsUtc(all);

				//rule 9 - GetCount agrees with GetAll exactly
				count.ShouldBe(all.Count);
			}
			);
		}

		/// <summary>
		/// Rule 18 across a whole retrieved set. Every department in the store arrived through <c>Insert</c>, so
		/// every one of them carries stamps this can be asserted on.
		/// </summary>
		private static void AssertEveryStampIsUtc(IEnumerable<Department> departments)
		{
			foreach (var dept in departments)
			{
				StampWindow.ShouldBeUtcStamp(dept.CreatedDate);

				if (dept.UpdatedDate.HasValue)
					StampWindow.ShouldBeUtcStamp(dept.UpdatedDate);

				if (dept.DeletedDate.HasValue)
					StampWindow.ShouldBeUtcStamp(dept.DeletedDate);
			}
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

				//rule 18 - a paged department carries the same Coordinated Universal Time stamps as any other
				AssertEveryStampIsUtc(pagedTogether);
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
			var test = Setup_InsertDepartment_TestGet(_da);
			var selected = _da.GetAll(new Department());

			//act - rule 13, this is exactly how the generic dispatcher calls it
			var all = _da.GetAll(null);

			//assert - rule 13 says the selector is never read, so the two calls have to agree exactly. "Did not
			//throw" is satisfied by returning an empty list, which is why the set itself is compared.
			all.ShouldNotBeNull();
			all.Any(x => x.Id == test.DepartmentId).ShouldBeTrue();
			all.Select(x => x.Id).OrderBy(x => x).ShouldBe(selected.Select(x => x.Id).OrderBy(x => x));
		}

		[Fact]
		public void ShouldNotThrowWhenGetPagedIsGivenNull()
		{
			//setup
			var test = Setup_InsertDepartment_TestGet(_da);
			var all = _da.GetAll(new Department());

			//act - rule 13
			var page = _da.GetPaged(null, 0, all.Count);

			//assert - a full window with a null selector is the whole live set, in the order rule 11 fixes
			page.ShouldNotBeNull();
			page.Count.ShouldBe(all.Count);
			page.Any(x => x.Id == test.DepartmentId).ShouldBeTrue();
			page.Select(x => x.Id).ShouldBe(all.Select(x => x.Id));
		}

		[Fact]
		public void ShouldNotThrowWhenGetCountIsGivenNull()
		{
			//setup
			var test = Setup_InsertDepartment_TestGet(_da);
			var all = _da.GetAll(new Department());
			all.Any(x => x.Id == test.DepartmentId).ShouldBeTrue();

			//act - rule 13
			var count = _da.GetCount(null);

			//assert - rule 9, the count with a null selector is the count of the live set and nothing looser
			count.ShouldBe(all.Count);
		}

		#endregion
	}
}
