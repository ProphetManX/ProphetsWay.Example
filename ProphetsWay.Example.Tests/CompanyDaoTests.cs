using Xunit;
using System.Linq;
using System.Collections.Generic;
using System;
using ProphetsWay.Example.DataAccess.Entities;
using FluentAssertions;
using ProphetsWay.Example.DataAccess.IDaos;
using ProphetsWay.Example.DataAccess.NoDB;

namespace ProphetsWay.Example.Tests
{
	[Collection("Company Dao Tests")]
	public class CompanyDaoTests : BaseUnitTests<ICompanyDao>
	{
		protected override ICompanyDao GetIExampleDataAccess => new ExampleDataAccess();

		public static Company NewCompany => new Company{ Name = $"Bob {Guid.NewGuid()}" };

		public delegate void InsertAssertion(Company co);
		public static (Company Company, InsertAssertion Assert) Setup_CreateCompany_TestInsert()
        {
			return (NewCompany, (Company co) =>
			{
				co.Id.Should().NotBe(default);
			}
			);
		}

		[Fact]
		public void ShouldInsertCompany()
		{
			//setup
			var coTest = CompanyDaoTests.Setup_CreateCompany_TestInsert();

			//act
			_da.Insert(coTest.Company);

			//assert
			coTest.Assert(coTest.Company);
		}

		public delegate void GetAssertion(Company co);
		public static (int CompanyId, GetAssertion Assertion) Setup_InsertCompany_TestGet(ICompanyDao da)
		{
			var co = NewCompany;
			da.Insert(co);

			return (co.Id, (Company co2) =>
			{
				co2.Name.Should().Be(co.Name);
			}
			);
		}

		[Fact]
		public void ShouldGetCompany()
		{
			//setup
			var t = Setup_InsertCompany_TestGet(_da);

			//act
			var co2 = _da.Get(new Company { Id = t.CompanyId });

			//assert
			t.Assertion(co2);
		}

		public delegate void UpdateAssertion(int count);
		public static (Company Company, UpdateAssertion Assert) Setup_InsertCompany_TestUpdate(ICompanyDao da)
        {
			var co = NewCompany;
			da.Insert(co);
			co.Other = "Edited Text, after the insert has completed.";

			return (co, (count) => {
				var co2 = da.Get(co);

				count.Should().Be(1);
				co.Id.Should().Be(co2.Id);
				co.Other.Should().Be(co2.Other);
			}
			);
        }

		[Fact]
		public void ShouldUpdateCompany()
		{
			//setup
			var test = Setup_InsertCompany_TestUpdate(_da);

			//act
			var count = _da.Update(test.Company);

			//assert
			test.Assert(count);
		}

		public delegate void DeleteAssertion(int count);
		public static (Company Company, DeleteAssertion Assert) Setup_InsertCompany_TestDelete(ICompanyDao da)
        {
			var co = NewCompany;
			da.Insert(co);

			return (co, (int count) =>
			{
				count.Should().Be(1);
				var freshQueryCo = da.Get(co);
				freshQueryCo.Should().BeNull();
			}
			);
		}


		[Fact]
		public void ShouldDeleteCompany()
		{
			//setup
			var test = Setup_InsertCompany_TestDelete(_da);

			//act
			var count = _da.Delete(test.Company);

			//assert
			test.Assert(count);
		}

		public delegate void CountAssertion(int count);
		public static CountAssertion SetupShouldGetCount(ICompanyDao da)
		{
			var co = NewCompany;
			da.Insert(co);
			var co1 = NewCompany;
			da.Insert(co1);
			var co2 = NewCompany;
			da.Insert(co2);

			return (count) =>
			{
				count.Should().BeGreaterOrEqualTo(3);
			};
		}

		[Fact]
		public void ShouldGetCount()
		{
			//setup
			var assertion = SetupShouldGetCount(_da);

			//act
			var count = _da.GetCount(new Company());

			//assert
			assertion(count);
		}

		public delegate void PagedAssertion(int count, IList<Company> all, IList<Company> subset);
		public static PagedAssertion SetupShouldGetPagedView(ICompanyDao da)
		{
			var co = NewCompany;
			da.Insert(co);
			var co1 = NewCompany;
			da.Insert(co1);
			var co2 = NewCompany;
			da.Insert(co2);

			return (count, all, subset) =>
			{
				all.Count.Should().Be(count);
				subset.Count.Should().Be(1);
				subset.First().Id.Should().Be(all.Skip(1).First().Id);
			};
		}

		[Fact]
		public void ShouldGetPagedView()
		{
			//setup
			var assertion = SetupShouldGetPagedView(_da);

			//act
			var count = _da.GetCount(new Company());
			var view = _da.GetPaged(new Company(), 0, count);
			var subset = _da.GetPaged(new Company(), 1, 1);

			//assert
			assertion(count, view, subset);
		}

		[Fact]
		public void ShouldGetCustomCompanyFunction()
		{
			//setup
			var co = NewCompany;
			_da.Insert(co);
			var co1 = NewCompany;
			_da.Insert(co1);
			var co2 = NewCompany;
			_da.Insert(co2);

			//act
			var custom = _da.GetCustomCompanyFunction(100);

			//assert
			custom.Should().NotBeNull();
		}
	}
}
