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
			var ct = CompanyDaoTests.SetupShouldGetCompany(_da);
			var ut = UserDaoTests.SetupShouldGetUser(_da);
			var jt = JobDaoTests.SetupShouldGetUser(_da);

			//act
			var u2 = _da.Get<User>(ut.UserId);
			var co2 = _da.Get<Company>(ct.CompanyId);
			var j2 = _da.Get<Job>(jt.JobId);

			//assert
			ct.Assertion(co2);
			ut.Assertion(u2);
			jt.Assertion(j2);
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
