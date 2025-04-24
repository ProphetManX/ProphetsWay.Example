using FluentAssertions;

using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using ProphetsWay.Example.DataAccess.NoDB;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace ProphetsWay.Example.Tests
{
    [Collection("Transaction Dao Tests")]
	public class TransactionDaoTests : BaseUnitTests<ITransactionDao>
	{
		protected override ITransactionDao GetIExampleDataAccess => new ExampleDataAccess();

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
			t.Id.Should().NotBe(default);
		}

		public static (long TransactionId, Func<Transaction, int> Assertion) SetupShouldGetTransaction(ITransactionDao dao)
		{
			var t = NewTransaction;
			dao.Insert(t);

			return (t.Id, (t2) =>
			{
				//checking with error threshold, because of accuracy differences in how DB stores datetime values
				var diff = t2.DateOfAction - t.DateOfAction;
				var errThreshold = TimeSpan.FromMilliseconds(10);
				diff.Should().BeLessThanOrEqualTo(errThreshold);
				t2.Amount.Should().Be(t.Amount);

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
			count.Should().Be(1);
			t2.Amount.Should().Be(editAmount);
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
			count.Should().Be(1);
			co2.Should().BeNull();
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
				count.Should().BeGreaterThanOrEqualTo(3);
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
				all.Count.Should().Be(count);
				subset.Count().Should().Be(1);
				subset.First().Id.Should().Be(all.Skip(1).First().Id);
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
