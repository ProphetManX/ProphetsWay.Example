using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.NoDB;
using Xunit;

namespace ProphetsWay.Example.Tests
{
	[Collection("Base Tests")]
	public class BaseDataAccessTests : BaseUnitTests<IExampleDataAccess>
	{
		protected override IExampleDataAccess GetIExampleDataAccess => new ExampleDataAccess();

		[Fact]
		public void ShouldGetGenericTypes()
		{
			//setup
			var ct = CompanyDaoTests.Setup_InsertCompany_TestGet(_da);
			var ut = UserDaoTests.SetupShouldGetUser(_da);
			var jt = JobDaoTests.Setup_InsertJob_TestGet(_da);
			var tr = TransactionDaoTests.SetupShouldGetTransaction(_da);
			var re = ResourceDaoTests.SetupShouldGetResource(_da);

			//act
			var u2 = _da.Get<User>(ut.UserId);
			var co2 = _da.Get<Company>(ct.CompanyId);
			var j2 = _da.Get<Job>(jt.JobId);
			var tr2 = _da.Get<Transaction>(tr.TransactionId);
			var re2 = _da.Get<Resource>(re.ResourceId);

			//assert
			ct.Assertion(co2);
			ut.Assertion(u2);
			jt.Assertion(j2);
			re.Assertion(re2);
			tr.Assertion(tr2);
		}

        [Fact]
		public void ShouldDeleteGenericTypes()
        {
			var coTest = CompanyDaoTests.Setup_InsertCompany_TestDelete(_da);
			var joTest = JobDaoTests.Setup_InsertJob_TestDelete(_da);

			var coCount = _da.Delete<Company>(coTest.Company);
			var joCount = _da.Delete<Job>(joTest.Job);

			coTest.Assert(coCount);
			joTest.Assert(joCount);
        }

        [Fact]
		public void ShouldInsertGenericTypes()
        {
			var coTest = CompanyDaoTests.Setup_CreateCompany_TestInsert();
			var joTest = JobDaoTests.Setup_CreateJob_TestInsert();

			_da.Insert<Company>(coTest.Company);
			_da.Insert<Job>(joTest.Job);

			coTest.Assert(coTest.Company);
			joTest.Assert(joTest.Job);
        }

        [Fact]
		public void ShouldUpdateGenericTypes()
        {
			var coTest = CompanyDaoTests.Setup_InsertCompany_TestUpdate(_da);
			var joTest = JobDaoTests.Setup_InsertJob_TestUpdate(_da);

			var coCount = _da.Update<Company>(coTest.Company);
			var joCount = _da.Update<Job>(joTest.Job);

			coTest.Assert(coCount);
			joTest.Assert(joCount);
        }

		[Fact]
		public void ShouldGetGenericPaged()
		{
			//setup
			var assertion = CompanyDaoTests.SetupShouldGetPagedView(_da);

			//act
			var count = _da.GetCount<Company>();
			var view = _da.GetPaged<Company>(0, count);
			var subset = _da.GetPaged<Company>(1, 1);

			//assert
			assertion(count, view, subset);
		}

		[Fact]
		public void ShouldGetGenericCount()
		{
			//setup
			var assertion = CompanyDaoTests.SetupShouldGetCount(_da);

			//act
			var count = _da.GetCount<Company>();

			//assert
			assertion(count);
		}

		[Fact]
		public void ShouldGetGenericAll()
		{
			//setup
			var assertion = JobDaoTests.SetupShouldGetAllJobs(_da);

			//act
			var all = _da.GetAll<Job>();

			//assert
			assertion(all);

		}
	}
}
