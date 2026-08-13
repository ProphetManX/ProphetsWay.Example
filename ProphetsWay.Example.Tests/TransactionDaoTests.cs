using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;

using Shouldly;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace ProphetsWay.Example.Tests
{
	[Collection(TestCollections.SharedStore)]
	[Trait("Scope", "Contract")]
	public class TransactionDaoTests : BaseUnitTests<ITransactionDao>
	{
		protected static Random Random = new Random();

		public static Transaction NewTransaction => new Transaction { DateOfAction = DateTime.Now, Amount = Random.Next() };

		[Fact]
		public void ShouldInsertTransaction()
		{
			//setup 
			var t = new Transaction { DateOfAction = DateTime.Now };

			//act
			_da.Insert(t);

			//assert
			t.Id.ShouldNotBe(default);
		}

		public static (long TransactionId, Func<Transaction, int> Assertion) SetupShouldGetTransaction(ITransactionDao dao)
		{
			var t = NewTransaction;
			dao.Insert(t);

			return (t.Id, (t2) =>
			{
				//checking with error threshold, because of accuracy differences in how DB stores datetime values.
				//Duration() makes it a tolerance in both directions - a signed difference lets a retrieved stamp
				//that is earlier than the original through however far off it is.
				var diff = t2.DateOfAction - t.DateOfAction;
				var errThreshold = TimeSpan.FromMilliseconds(10);
				diff.Duration().ShouldBeLessThanOrEqualTo(errThreshold);
				t2.Amount.ShouldBe(t.Amount);

				return 1;
			}
			);
		}

		[Fact]
		public void ShouldGetTransaction()
		{
			//setup 
			var t = SetupShouldGetTransaction(_da);

			//act 
			var t2 = _da.Get(new Transaction { Id = t.TransactionId });

			//assert
			t.Assertion(t2);
		}

		[Fact]
		public void ShouldUpdateTransaction()
		{
			//setup
			decimal editAmount = Random.Next();
			var t = NewTransaction;
			_da.Insert(t);

			//act
			t.Amount = editAmount;
			var count = _da.Update(t);
			var t2 = _da.Get(t);

			//assert
			count.ShouldBe(1);
			t2.Amount.ShouldBe(editAmount);
		}

		[Fact]
		public void ShouldDeleteJob()
		{
			//setup
			var co = NewTransaction;
			_da.Insert(co);

			//act
			var count = _da.Delete(co);
			var co2 = _da.Get(co);

			//assert
			count.ShouldBe(1);
			co2.ShouldBeNull();
		}

		public static Func<int, int> SetupShouldGetCount(ITransactionDao dao)
		{
			var t = NewTransaction;
			dao.Insert(t);
			var t2 = NewTransaction;
			dao.Insert(t2);
			var t3 = NewTransaction;
			dao.Insert(t3);

			return (count) =>
			{
				count.ShouldBeGreaterThanOrEqualTo(3);
				return 1;
			};
		}

		[Fact]
		public void ShouldGetCount()
		{
			//setup
			var assertion = SetupShouldGetCount(_da);

			//act
			var count = _da.GetCount(NewTransaction);

			//assert
			assertion(count);
		}

		public static Func<int, IList<Transaction>, IList<Transaction>, int> SetupShouldGetPagedView(ITransactionDao dao)
		{
			var t = NewTransaction;
			dao.Insert(t);
			var t2 = NewTransaction;
			dao.Insert(t2);
			var t3 = NewTransaction;
			dao.Insert(t3);

			return (count, all, subset) =>
			{
				all.Count.ShouldBe(count);
				subset.Count().ShouldBe(1);
				subset.First().Id.ShouldBe(all.Skip(1).First().Id);
				return 1;
			};
		}

		[Fact]
		public void ShouldGetPagedView()
		{
			//setup
			var assertion = SetupShouldGetPagedView(_da);

			//act
			var count = _da.GetCount(NewTransaction);
			var view = _da.GetPaged(NewTransaction, 0, count);
			var subset = _da.GetPaged(NewTransaction, 1, 1);

			//assert
			assertion(count, view, subset);
		}
	}
}
