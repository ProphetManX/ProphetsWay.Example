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
	/// Disposal of <see cref="IExampleDataAccess"/>. The rules being asserted are stated on
	/// <c>IBaseDataAccess</c> in <c>ProphetsWay.BaseDataAccess</c>, but <c>BaseDataAccess.Dispose</c> is
	/// abstract - every implementation writes its own, so the behaviour under test belongs to this repository
	/// and is covered here rather than left to the base library.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Joins <see cref="TestCollections.CoreEntities"/> because it writes <see cref="Department"/> rows.
	/// </para>
	/// <para>
	/// <b>The first test is the important one, and it is a diagnostic before it is a contract check.</b>
	/// <see cref="BaseUnitTests{T}"/> disposes its Data Access Layer after every single test, and
	/// <see cref="TestCollections.CompanyResources"/> runs in parallel with this collection. A
	/// <c>Dispose</c> written to clear the store therefore deletes data out from underneath a test in the
	/// other collection, mid-run - and does so intermittently, on whichever test happens to be executing at
	/// the time, vanishing entirely whenever either collection is run on its own. That failure is close to
	/// undiagnosable from the outside. This test names the cause, in one place, on one thread.
	/// </para>
	/// </remarks>
	[Collection(TestCollections.CoreEntities)]
	public class DataAccessDisposalTests : BaseUnitTests<IExampleDataAccess>
	{
		protected override IExampleDataAccess GetIExampleDataAccess => new ExampleDataAccess();

		public delegate void SurvivalAssertion(Department refetched, IList<Department> all);
		public static (int DepartmentId, SurvivalAssertion Assert) Setup_InsertDepartment_TestItSurvivesAnotherInstanceBeingDisposed(IExampleDataAccess da)
		{
			var dept = DepartmentDaoTests.NewDepartment;
			da.Insert(dept);

			return (dept.Id, (refetched, all) =>
			{
				//Disposing one Data Access Layer releases what that instance was holding. It does not
				//discard stored data, any more than closing one connection empties the database.
				refetched.ShouldNotBeNull();
				refetched.Id.ShouldBe(dept.Id);
				refetched.Name.ShouldBe(dept.Name);
				refetched.CreatedDate.ShouldBe(dept.CreatedDate);
				all.Any(x => x.Id == dept.Id).ShouldBeTrue();
			}
			);
		}

		[Fact]
		public void ShouldNotDiscardStoredDataWhenAnotherDataAccessInstanceIsDisposed()
		{
			//setup
			var test = Setup_InsertDepartment_TestItSurvivesAnotherInstanceBeingDisposed(_da);

			//act - a second instance, used and then disposed, exactly as every other test class in this suite
			//disposes one after every test it runs
			using (var other = new ExampleDataAccess())
			{
				other.GetAll<Department>().Any(x => x.Id == test.DepartmentId).ShouldBeTrue();
			}

			//assert - the instance that wrote the department still finds it
			var refetched = _da.Get(new Department { Id = test.DepartmentId });
			var all = _da.GetAll<Department>();

			test.Assert(refetched, all);
		}

		[Fact]
		public void ShouldNotThrowWhenDisposedTwice()
		{
			//setup
			var da = new ExampleDataAccess();

			//act & assert - disposal is idempotent, so that a using statement nested inside a finally block
			//is safe regardless of what came before. Contrast the transaction members, which throw on a
			//second call by design.
			Should.NotThrow(() =>
			{
				da.Dispose();
				da.Dispose();
			});
		}

		[Fact]
		public void ShouldThrowWhenAMemberIsCalledAfterDispose()
		{
			//setup
			var da = new ExampleDataAccess();
			da.Dispose();

			//act & assert - every member other than Dispose refuses to run on a disposed instance.
			//ObjectDisposedException derives from InvalidOperationException, so a consumer catching the
			//latter around its transaction handling catches use-after-dispose along with it.
			Should.Throw<ObjectDisposedException>(() =>
			{
				da.GetAll<Department>();
			});
		}
	}
}
