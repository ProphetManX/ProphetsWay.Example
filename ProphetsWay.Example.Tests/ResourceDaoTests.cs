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
	[Collection("Resource Dao Tests")]
	public class ResourceDaoTests : BaseUnitTests<IResourceDao>
	{
		protected override IResourceDao GetIExampleDataAccess => new ExampleDataAccess();

		[Fact]
		public void ShouldInsertResource()
		{
			//setup
			var co = new Resource { Name = $"Bob {Guid.NewGuid()}" };

			//act
			_da.Insert(co);

			//assert
			co.Id.Should().NotBe(default(Guid));
		}

		public delegate void GetAssertion(Resource co);
		public static (Guid ResourceId, GetAssertion Assertion) SetupShouldGetResource(IResourceDao da)
		{
			var co = new Resource { Name = $"Bob {Guid.NewGuid()}" };
			da.Insert(co);

			return (co.Id, (co2) =>
			{
				co2.Name.Should().Be(co.Name);
			}
			);
		}

		[Fact]
		public void ShouldGetResource()
		{
			//setup
			var t = SetupShouldGetResource(_da);

			//act
			var co2 = _da.Get(new Resource { Id = t.ResourceId });

			//assert
			t.Assertion(co2);
		}

		[Fact]
		public void ShouldUpdateResource()
		{
			//setup
			const string editText = "blarg";
			var co = new Resource { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			co.Name = editText;
			var count = _da.Update(co);
			var co2 = _da.Get(co);

			//assert
			count.Should().Be(1);
			co2.Name.Should().Be(editText);

		}

		[Fact]
		public void ShouldDeleteResource()
		{
			//setup
			var co = new Resource { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			var count = _da.Delete(co);
			var co2 = _da.Get(co);

			//assert
			count.Should().Be(1);
			co2.Should().BeNull();
		}

		[Fact]
		public void ShouldGetAllResources()
		{
			//setup
			var assertion = SetupShouldGetAllResources(_da);

			//act
			var all = _da.GetAll(new Resource());

			//assert
			assertion(all);
		}

		public delegate void Assertion(IList<Resource> all);
		public static Assertion SetupShouldGetAllResources(IResourceDao da)
		{
			var co = new Resource { Name = $"Eric {Guid.NewGuid()}" };
			da.Insert(co);
			var co1 = new Resource { Name = $"Sam {Guid.NewGuid()}" };
			da.Insert(co1);
			var co2 = new Resource { Name = $"Jim {Guid.NewGuid()}" };
			da.Insert(co2);

			return (all) =>
			{
				all.Count.Should().BeGreaterThanOrEqualTo(3);
				all.Where(x => x.Name == co.Name).Count().Should().Be(1);
				all.Where(x => x.Name == co1.Name).Count().Should().Be(1);
				all.Where(x => x.Name == co2.Name).Count().Should().Be(1);
			};
		}

	}
}
