using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.NoDB;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The same <see cref="Department"/> contract as <see cref="DepartmentDaoTests"/>, reached through the
	/// aggregate <see cref="IExampleDataAccess"/> - the generic dispatcher on <c>BaseDataAccess</c> for the
	/// inherited members, and a plain interface call for the custom <c>Restore</c>. Replaying the same setup
	/// helpers is how this repository shows the two paths agree.
	/// </summary>
	[Collection(TestCollections.CoreEntities)]
	public class DepartmentDataAccessTests : BaseUnitTests<IExampleDataAccess>
	{
		protected override IExampleDataAccess GetIExampleDataAccess => new ExampleDataAccess();

		[Fact]
		public void ShouldInsertGenericDepartment()
		{
			//setup
			var test = DepartmentDaoTests.Setup_CreateDepartment_TestInsert();

			//act
			var window = StampWindow.Around(() => _da.Insert<Department>(test.Department));

			//assert
			test.Assert(test.Department, window);
		}

		[Fact]
		public void ShouldGetGenericDepartment()
		{
			//setup
			var test = DepartmentDaoTests.Setup_InsertDepartment_TestGet(_da);

			//act
			var found = _da.Get<Department>(test.DepartmentId);

			//assert
			test.Assertion(found);
		}

		[Fact]
		public void ShouldGetGenericSoftDeletedDepartment()
		{
			//setup
			var test = DepartmentDaoTests.Setup_DeleteDepartment_TestGetStillFindsIt(_da);

			//act - rule 8 holds through the dispatcher too
			var found = _da.Get<Department>(test.DepartmentId);

			//assert
			test.Assertion(found);
		}

		[Fact]
		public void ShouldUpdateGenericDepartment()
		{
			//setup
			var test = DepartmentDaoTests.Setup_InsertDepartment_TestUpdate(_da);

			//act
			var window = StampWindow.Around(() => _da.Update<Department>(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		[Fact]
		public void ShouldPreserveStoredCreatedDateWhenUpdatingGenericLiveDepartment()
		{
			//setup
			var test = DepartmentDaoTests.Setup_InsertDepartment_TestUpdatePreservesStoredCreatedDate(_da);

			//act - whole-object replacement is just as tempting on the dispatched path as on the direct one
			var window = StampWindow.Around(() => _da.Update<Department>(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		[Fact]
		public void ShouldPreserveStoredStampsWhenUpdatingGenericSoftDeletedDepartment()
		{
			//setup
			var test = DepartmentDaoTests.Setup_DeleteDepartment_TestUpdatePreservesStoredStamps(_da);

			//act - rule 3 is a property of the Data Access Layer, not of the path taken to reach it
			var window = StampWindow.Around(() => _da.Update<Department>(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		[Fact]
		public void ShouldDeleteGenericDepartment()
		{
			//setup
			var test = DepartmentDaoTests.Setup_InsertDepartment_TestDelete(_da);

			//act
			var window = StampWindow.Around(() => _da.Delete<Department>(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}

		[Fact]
		public void ShouldGetGenericAllDepartmentsAndAgreeWithGenericCount()
		{
			//setup
			var test = DepartmentDaoTests.Setup_LiveAndDeletedDepartments_TestGetAll(_da);

			//act - rule 13, the dispatcher passes a null type selector into both of these
			var all = _da.GetAll<Department>();
			var count = _da.GetCount<Department>();

			//assert
			test.Assert(all, count);
		}

		[Fact]
		public void ShouldPartitionGenericGetAllAcrossSuccessiveGenericPages()
		{
			//setup
			var test = DepartmentDaoTests.Setup_InsertDepartments_TestPagingPartitionsGetAll(_da);

			//act
			var all = _da.GetAll<Department>();
			var pagedTogether = new List<Department>();
			for (var skip = 0; skip < all.Count; skip += 2)
				pagedTogether.AddRange(_da.GetPaged<Department>(skip, 2));

			//assert
			test.Assert(all, pagedTogether);
		}

		[Fact]
		public void ShouldRestoreDepartmentThroughTheAggregateInterface()
		{
			//setup
			var test = DepartmentDaoTests.Setup_DeleteDepartment_TestRestore(_da);

			//act - Restore is a custom member of IDepartmentDao, so it is called directly rather than
			//dispatched. That it sits alongside the dispatched members is the point it makes.
			var count = _da.Restore(test.Department);

			//assert
			test.Assert(count);
		}

		[Fact]
		public void ShouldThrowWhenGenericGetIsGivenANullId()
		{
			//setup - no precondition: the probe entity is built before any data is read

			//act & assert - rule 16, the reflective setter rejects it before the Dao is ever reached
			Should.Throw<ArgumentException>(() =>
			{
				_da.Get<Department>(null);
			});
		}

		[Theory]
		[InlineData("42")]
		[InlineData(4.2)]
		[InlineData(42L)]
		public void ShouldThrowWhenGenericGetIsGivenAnIdThatIsNotAnInt(object id)
		{
			//setup - no precondition: the id never reaches the store

			//act & assert - rule 16, an ArgumentException and specifically not a DataAccessConventionException
			Should.Throw<ArgumentException>(() =>
			{
				_da.Get<Department>(id);
			});
		}

		/// <summary>
		/// Rule 8's real payload: a <see cref="User"/> pointing at a department that gets soft-deleted is
		/// never left pointing at nothing.
		/// </summary>
		public delegate void SurvivalAssertion(int count, StampWindow window);
		public static (Department Department, SurvivalAssertion Assert) Setup_UserInDepartment_TestDepartmentSurvivesDelete(IExampleDataAccess da)
		{
			var dept = DepartmentDaoTests.NewDepartment;
			da.Insert(dept);

			var user = new User { Name = $"User {Guid.NewGuid()}", Department = dept };
			da.Insert(user);

			return (dept, (count, window) =>
			{
				count.ShouldBe(1);

				//The assertion has to be a round trip. User.Department is a navigation property held in
				//memory, so it would still be non-null even if the row had been ripped out of the store -
				//checking it would prove nothing about the Data Access Layer. Re-fetch by id instead,
				//from a fresh instance carrying nothing but the identifier.
				var stored = da.Get(new Department { Id = dept.Id });
				stored.ShouldNotBeNull();
				stored.Id.ShouldBe(dept.Id);
				stored.Name.ShouldBe(dept.Name);

				//rule 18 - the stamp comes back as UTC, read during the Delete
				window.ShouldContainStamp(stored.DeletedDate);

				//and the department is out of the live views while remaining perfectly reachable
				da.GetAll<Department>().Any(x => x.Id == dept.Id).ShouldBeFalse();
			}
			);
		}

		[Fact]
		public void ShouldNotOrphanUserWhenItsDepartmentIsSoftDeleted()
		{
			//setup
			var test = Setup_UserInDepartment_TestDepartmentSurvivesDelete(_da);

			//act
			var window = StampWindow.Around(() => _da.Delete<Department>(test.Department), out int count);

			//assert
			test.Assert(count, window);
		}
	}
}
