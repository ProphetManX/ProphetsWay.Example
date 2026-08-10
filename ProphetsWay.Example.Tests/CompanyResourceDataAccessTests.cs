using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.NoDB;
using Shouldly;
using Xunit;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// <see cref="CompanyResource"/> through the aggregate <see cref="IExampleDataAccess"/>. This is the
	/// boundary the showcase exists to draw: an entity with no identifier still dispatches through
	/// <c>Insert</c>, <c>Delete</c> and <c>GetAll</c>, because none of those resolve an identifier - and it
	/// can never dispatch through <c>Get</c>, because that one does.
	/// </summary>
	[Collection(TestCollections.CompanyResources)]
	public class CompanyResourceDataAccessTests : BaseUnitTests<IExampleDataAccess>
	{
		protected override IExampleDataAccess GetIExampleDataAccess => new ExampleDataAccess();

		[Fact]
		public void ShouldInsertGenericCompanyResource()
		{
			//setup
			var test = CompanyResourceDaoTests.Setup_CreateCompanyResource_TestInsert(_da);

			//act
			_da.Insert<CompanyResource>(test.CompanyResource);

			//assert
			test.Assert(test.CompanyResource);
		}

		[Fact]
		public void ShouldIgnoreGenericInsertOfCompanyResourceThatIsAlreadyStored()
		{
			//setup
			var test = CompanyResourceDaoTests.Setup_InsertCompanyResource_TestDuplicateIsNoOp(_da);

			//act - rule 3 holds whichever path the call arrives on
			Should.NotThrow(() => _da.Insert<CompanyResource>(test.CompanyResource));

			//assert
			test.Assert();
		}

		[Fact]
		public void ShouldDeleteGenericCompanyResource()
		{
			//setup
			var test = CompanyResourceDaoTests.Setup_InsertCompanyResource_TestDelete(_da);

			//act
			var count = _da.Delete<CompanyResource>(test.CompanyResource);

			//assert
			test.Assert(count);
		}

		[Fact]
		public void ShouldGetAllGenericCompanyResources()
		{
			//setup
			var test = CompanyResourceDaoTests.Setup_InsertCompanyResource_TestGetAll(_da);

			//act - rule 6, the dispatcher hands GetAll a null type selector and it works anyway.
			//This is the half of the boundary that does work.
			var all = _da.GetAll<CompanyResource>();

			//assert
			test.Assert(all);
		}

		[Theory]
		[InlineData(1)]
		[InlineData("abc")]
		[InlineData(new object[] { null })]
		public void ShouldThrowWhenGenericGetIsUsedOnAnEntityWithNoIdentifier(object id)
		{
			//setup - no precondition: this call can never succeed, whatever is stored

			//act & assert - rule 8. It fails for two independent reasons, the entity exposes no identifier
			//property and the Dao declares no Get, and which one is reported is unspecified - so the type
			//of the exception is the only thing a test may assert on.
			Should.Throw<DataAccessConventionException>(() =>
			{
				_da.Get<CompanyResource>(id);
			});
		}
	}
}
