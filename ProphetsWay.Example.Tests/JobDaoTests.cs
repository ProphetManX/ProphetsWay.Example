using Xunit;
using System.Linq;
using System.Collections.Generic;
using System;
using ProphetsWay.Example.DataAccess.Entities;
using Shouldly;
using ProphetsWay.Example.DataAccess.IDaos;
using ProphetsWay.Example.DataAccess.NoDB;

namespace ProphetsWay.Example.Tests
{
	[Collection(TestCollections.CoreEntities)]
	public class JobDaoTests : BaseUnitTests<IJobDao>
	{
		protected override IJobDao GetIExampleDataAccess => new ExampleDataAccess();

		public static Job NewJob => new Job { Name = $"Bob {Guid.NewGuid()}" };


		public delegate void InsertAssertion(Job co);
		public static (Job Job, InsertAssertion Assert) Setup_CreateJob_TestInsert()
		{
			return (NewJob, (Job co) =>
			{
				co.Id.ShouldNotBe(default);
			}
			);
		}

		[Fact]
		public void ShouldInsertJob()
		{
			//setup
			var coTest = JobDaoTests.Setup_CreateJob_TestInsert();

			//act
			_da.Insert(coTest.Job);

			//assert
			coTest.Assert(coTest.Job);
		}

		public delegate void GetAssertion(Job co);
		public static (int JobId, GetAssertion Assertion) Setup_InsertJob_TestGet(IJobDao da)
		{
			var co = NewJob;
			da.Insert(co);

			return (co.Id, (Job co2) =>
			{
				co2.Name.ShouldBe(co.Name);
			}
			);
		}

		[Fact]
		public void ShouldGetJob()
		{
			//setup
			var t = Setup_InsertJob_TestGet(_da);

			//act
			var co2 = _da.Get(new Job { Id = t.JobId });

			//assert
			t.Assertion(co2);
		}

		public delegate void UpdateAssertion(int count);
		public static (Job Job, UpdateAssertion Assert) Setup_InsertJob_TestUpdate(IJobDao da)
		{
			const string editText = "Edited Text, after the insert has completed.";

			var co = NewJob;
			da.Insert(co);

			var newCo = da.Get(co);
			newCo.Something = editText;

			return (newCo, (count) => {
				var co2 = da.Get(co);

				count.ShouldBe(1);
				co.Id.ShouldBe(co2.Id);

				//the edit was made on newCo and submitted through Update, so newCo is the instance to assert
				//through - co was never edited, and reading the change off it only worked while Get handed back
				//the store's own instance
				newCo.Something.ShouldBe(co2.Something);
				co2.Something.ShouldBe(editText);
			}
			);
		}

		[Fact]
		public void ShouldUpdateJob()
		{
			//setup
			var test = Setup_InsertJob_TestUpdate(_da);

			//act
			var count = _da.Update(test.Job);

			//assert
			test.Assert(count);
		}

		public delegate void DeleteAssertion(int count);
		public static (Job Job, DeleteAssertion Assert) Setup_InsertJob_TestDelete(IJobDao da)
		{
			var co = NewJob;
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
		public void ShouldDeleteJob()
		{
			//setup
			var test = Setup_InsertJob_TestDelete(_da);

			//act
			var count = _da.Delete(test.Job);

			//assert
			test.Assert(count);
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
			var co = NewJob;
			da.Insert(co);
			var co1 = NewJob;
			da.Insert(co1);
			var co2 = NewJob;
			da.Insert(co2);

			return (all) =>
			{
				all.Count.ShouldBeGreaterThanOrEqualTo(3);
				all.Where(x => x.Name == co.Name).Count().ShouldBe(1);
				all.Where(x => x.Name == co1.Name).Count().ShouldBe(1);
				all.Where(x => x.Name == co2.Name).Count().ShouldBe(1);
			};
		}

	}
}
