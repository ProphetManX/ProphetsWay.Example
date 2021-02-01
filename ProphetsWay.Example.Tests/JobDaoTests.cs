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
	[Collection("Job Dao Tests")]
	public class JobDaoTests : BaseUnitTests<IJobDao>
	{
		protected override IJobDao GetIExampleDataAccess => new ExampleDataAccess();

		[Fact]
		public void ShouldInsertJob()
		{
			//setup
			var co = new Job { Name = $"Bob {Guid.NewGuid()}" };

			//act
			_da.Insert(co);

			//assert
			co.Id.Should().NotBe(default);
		}

		public delegate void GetAssertion(Job co);
		public static (int JobId, GetAssertion Assertion) SetupShouldGetUser(IJobDao da)
		{
			var co = new Job { Name = $"Bob {Guid.NewGuid()}" };
			da.Insert(co);

			return (co.Id, (co2) =>
			{
				co2.Name.Should().Be(co.Name);
			}
			);
		}

		[Fact]
		public void ShouldGetJob()
		{
			//setup
			var t = SetupShouldGetUser(_da);

			//act
			var co2 = _da.Get(new Job { Id = t.JobId });

			//assert
			t.Assertion(co2);
		}

		[Fact]
		public void ShouldUpdateJob()
		{
			//setup
			const string editText = "blarg";
			var co = new Job { Name = $"Bob {Guid.NewGuid()}" };
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
		public void ShouldDeleteJob()
		{
			//setup
			var co = new Job { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			var count = _da.Delete(co);
			var co2 = _da.Get(co);

			//assert
			count.Should().Be(1);
			co2.Should().BeNull();
		}

		[Fact]
		public void ShouldGetAllJobs()
		{
			//setup
			var assertion = SetupShouldGetAllJobs(_da);

			//act
			var all = _da.GetAll(new Job());

			//assert
			assertion(all);
		}

		public delegate void Assertion(IList<Job> all);
		public static Assertion SetupShouldGetAllJobs(IJobDao da)
		{
			var co = new Job { Name = $"Eric {Guid.NewGuid()}" };
			da.Insert(co);
			var co1 = new Job { Name = $"Sam {Guid.NewGuid()}" };
			da.Insert(co1);
			var co2 = new Job { Name = $"Jim {Guid.NewGuid()}" };
			da.Insert(co2);

			return (all) =>
			{
				all.Count.Should().BeGreaterOrEqualTo(3);
				all.Where(x => x.Name == co.Name).Count().Should().Be(1);
				all.Where(x => x.Name == co1.Name).Count().Should().Be(1);
				all.Where(x => x.Name == co2.Name).Count().Should().Be(1);
			};
		}

	}
}
