using Xunit;
using System.Linq;
using System.Collections.Generic;
using System;
using ProphetsWay.Example.DataAccess.Entities;
using Shouldly;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The <see cref="ICompanyDao"/> contract, plus the one member of it whose behaviour is this
	/// implementation's own invention. The traits are on the methods rather than on the class for that reason -
	/// see <see cref="ShouldGetCustomCompanyFunction"/>.
	/// </summary>
	[Collection(TestCollections.SharedStore)]
	public class CompanyDaoTests : BaseUnitTests<ICompanyDao>
	{
		public static Company NewCompany => new Company{ Name = $"Bob {Guid.NewGuid()}" };

		public delegate void InsertAssertion(Company co);
		public static (Company Company, InsertAssertion Assert) Setup_CreateCompany_TestInsert()
        {
			return (NewCompany, (Company co) =>
			{
				co.Id.ShouldNotBe(default);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
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
				co2.Name.ShouldBe(co.Name);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
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
			const string editText = "Edited Text, after the insert has completed.";

			var co = NewCompany;
			da.Insert(co);

			var newCo = da.Get(co);
			newCo.Other = editText;

			return (newCo, (count) => {
				var co2 = da.Get(co);

				count.ShouldBe(1);
				co.Id.ShouldBe(co2.Id);

				//the edit was made on newCo and submitted through Update, so newCo is the instance to assert
				//through - co was never edited, and reading the change off it only worked while Get handed back
				//the store's own instance
				newCo.Other.ShouldBe(co2.Other);
				co2.Other.ShouldBe(editText);
			}
			);
        }

		[Fact]
		[Trait("Scope", "Contract")]
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
				count.ShouldBe(1);
				var freshQueryCo = da.Get(co);
				freshQueryCo.ShouldBeNull();
			}
			);
		}


		[Fact]
		[Trait("Scope", "Contract")]
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
				count.ShouldBeGreaterThanOrEqualTo(3);
			};
		}

		[Fact]
		[Trait("Scope", "Contract")]
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
				all.Count.ShouldBe(count);
				subset.Count.ShouldBe(1);
				subset.First().Id.ShouldBe(all.Skip(1).First().Id);
			};
		}

		[Fact]
		[Trait("Scope", "Contract")]
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

		/// <summary>
		/// <see cref="ICompanyDao.GetCustomCompanyFunction"/> stands in for whatever query a real Data Access
		/// Object would add beyond the surface it inherits, and the interface says nothing whatsoever about what
		/// its argument means. This implementation reads it as a position in the set and wraps round the end, so
		/// asking for 100 against three stored companies returns one of them; an implementation that read it as
		/// an identifier would return <c>null</c> here and be equally conforming.
		/// </summary>
		[Fact]
		[Trait("Scope", "Characterization")]
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
			custom.ShouldNotBeNull();
		}
	}
}
