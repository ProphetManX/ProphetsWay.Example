using Xunit;
using System.Linq;
using System.Collections.Generic;
using System;
using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using FluentAssertions;
using ProphetsWay.Example.DataAccess.IDaos;
using ProphetsWay.Example.DataAccess.NoDB;

namespace ProphetsWay.Example.Tests
{
	public class CompanyDaoTests : BaseUnitTests<ICompanyDao>
	{
		protected override ICompanyDao GetIExampleDataAccess => new ExampleDataAccess();

		[Fact]
		public void ShouldInsertCompany()
		{
			//setup
			var co = new Company { Name = $"Bob {Guid.NewGuid()}" };

			//act
			_da.Insert(co);

			//assert
			co.Id.Should().NotBe(default);
		}

		public delegate void GetAssertion(Company co);
		public static (int CompanyId, GetAssertion Assertion) SetupShouldGetCompany(ICompanyDao da)
		{
			var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
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
			var t = SetupShouldGetCompany(_da);

			//act
			var co2 = _da.Get(new Company { Id = t.CompanyId });

			//assert
			t.Assertion(co2);
		}

		[Fact]
		public void ShouldUpdateCompany()
		{
			//setup
			const string editText = "blarg";
			var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			co.Other = editText;
			var count = _da.Update(co);
			var co2 = _da.Get(co);

			//assert
			count.Should().Be(1);
			co2.Other.Should().Be(editText);

		}

		[Fact]
		public void ShouldDeleteCompany()
		{
			//setup
			var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			var count = _da.Delete(co);
			var co2 = _da.Get(co);

			//assert
			count.Should().Be(1);
			co2.Should().BeNull();
		}

		public delegate void CountAssertion(int count);
		public static CountAssertion SetupShouldGetCount(ICompanyDao da)
		{
			var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
			da.Insert(co);
			var co1 = new Company { Name = $"Sam {Guid.NewGuid()}" };
			da.Insert(co1);
			var co2 = new Company { Name = $"Jim {Guid.NewGuid()}" };
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
			var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
			da.Insert(co);
			var co1 = new Company { Name = $"Sam {Guid.NewGuid()}" };
			da.Insert(co1);
			var co2 = new Company { Name = $"Jim {Guid.NewGuid()}" };
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
			var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);
			var co1 = new Company { Name = $"Sam {Guid.NewGuid()}" };
			_da.Insert(co1);
			var co2 = new Company { Name = $"Jim {Guid.NewGuid()}" };
			_da.Insert(co2);

			//act
			var custom = _da.GetCustomCompanyFunction(100);

			//assert
			custom.Should().NotBeNull();
		}
	}
}
