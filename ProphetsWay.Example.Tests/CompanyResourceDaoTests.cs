using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using ProphetsWay.Example.DataAccess.NoDB;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The contract of <see cref="ICompanyResourceDao"/> - an entity with no identifier, and a Dao that
	/// inherits <c>IBaseDao&lt;T&gt;</c> not at all. Rule numbers refer to the numbered CONTRACT list on
	/// <see cref="ICompanyResourceDao"/>. <see cref="CompanyResourceDataAccessTests"/> replays these helpers
	/// through the generic dispatcher and shows where that path stops working.
	/// </summary>
	[Collection(TestCollections.CompanyResources)]
	public class CompanyResourceDaoTests : BaseUnitTests<ICompanyResourceDao>
	{
		protected override ICompanyResourceDao GetIExampleDataAccess => new ExampleDataAccess();

		private static int _lastCompanyId = 9000;

		/// <summary>
		/// A company identifier no other test uses. The Data Access Layer does not verify that the company or
		/// the resource named on a join actually exists, so a test never has to create either one.
		/// </summary>
		public static int NextCompanyId => Interlocked.Increment(ref _lastCompanyId);

		public static CompanyResource NewCompanyResource => new CompanyResource { CompanyId = NextCompanyId, ResourceId = Guid.NewGuid() };

		public static int CountMatching(ICompanyResourceDao da, CompanyResource pair)
		{
			return da.GetAll(null).Count(x => x.CompanyId == pair.CompanyId && x.ResourceId == pair.ResourceId);
		}

		#region Insert - rules 1, 2, 3

		public delegate void InsertAssertion(CompanyResource pair);
		public static (CompanyResource CompanyResource, InsertAssertion Assert) Setup_CreateCompanyResource_TestInsert(ICompanyResourceDao da)
		{
			var pair = NewCompanyResource;
			var companyId = pair.CompanyId;
			var resourceId = pair.ResourceId;

			return (pair, (inserted) =>
			{
				//rule 2 - there is no generated identifier, so nothing is assigned back onto the argument
				inserted.CompanyId.ShouldBe(companyId);
				inserted.ResourceId.ShouldBe(resourceId);

				//rule 1 - and the pair is now stored, exactly once
				CountMatching(da, inserted).ShouldBe(1);
			}
			);
		}

		[Fact]
		public void ShouldInsertCompanyResource()
		{
			//setup
			var test = Setup_CreateCompanyResource_TestInsert(_da);

			//act
			_da.Insert(test.CompanyResource);

			//assert
			test.Assert(test.CompanyResource);
		}

		public delegate void DuplicateInsertAssertion();
		public static (CompanyResource CompanyResource, DuplicateInsertAssertion Assert) Setup_InsertCompanyResource_TestDuplicateIsNoOp(ICompanyResourceDao da)
		{
			var stored = NewCompanyResource;
			da.Insert(stored);
			var totalBefore = da.GetAll(null).Count;

			//A separate instance describing the same pair - identity is the two values, not the reference.
			var duplicate = new CompanyResource { CompanyId = stored.CompanyId, ResourceId = stored.ResourceId };

			return (duplicate, () =>
			{
				//rule 3 - the store is unchanged and at most one row exists per pair
				da.GetAll(null).Count.ShouldBe(totalBefore);
				CountMatching(da, duplicate).ShouldBe(1);
			}
			);
		}

		[Fact]
		public void ShouldIgnoreInsertOfCompanyResourceThatIsAlreadyStored()
		{
			//setup
			var test = Setup_InsertCompanyResource_TestDuplicateIsNoOp(_da);

			//act - rule 3, a duplicate insert throws nothing
			Should.NotThrow(() => _da.Insert(test.CompanyResource));

			//assert
			test.Assert();
		}

		#endregion

		#region Delete - rules 1, 4

		public delegate void DeleteAssertion(int count);
		public static (CompanyResource CompanyResource, DeleteAssertion Assert) Setup_InsertCompanyResource_TestDelete(ICompanyResourceDao da)
		{
			var target = NewCompanyResource;
			da.Insert(target);

			//Two neighbours, each sharing one side of the pair. Rule 1 says neither may be caught by a
			//delete that matches on the pair.
			var sameCompany = new CompanyResource { CompanyId = target.CompanyId, ResourceId = Guid.NewGuid() };
			da.Insert(sameCompany);
			var sameResource = new CompanyResource { CompanyId = NextCompanyId, ResourceId = target.ResourceId };
			da.Insert(sameResource);

			var totalBefore = da.GetAll(null).Count;

			return (target, (count) =>
			{
				//rule 4 - one row removed, and never more than one
				count.ShouldBe(1);
				da.GetAll(null).Count.ShouldBe(totalBefore - 1);

				//rule 4 - a hard delete, so it is genuinely gone
				CountMatching(da, target).ShouldBe(0);

				//rule 1 - matching is on the pair, so the neighbours survive
				CountMatching(da, sameCompany).ShouldBe(1);
				CountMatching(da, sameResource).ShouldBe(1);
			}
			);
		}

		[Fact]
		public void ShouldDeleteCompanyResource()
		{
			//setup
			var test = Setup_InsertCompanyResource_TestDelete(_da);

			//act
			var count = _da.Delete(test.CompanyResource);

			//assert
			test.Assert(count);
		}

		public delegate void SecondDeleteAssertion(int count);
		public static (CompanyResource CompanyResource, SecondDeleteAssertion Assert) Setup_DeleteCompanyResource_TestSecondDelete(ICompanyResourceDao da)
		{
			var pair = NewCompanyResource;
			da.Insert(pair);
			da.Delete(pair);
			var totalBefore = da.GetAll(null).Count;

			return (pair, (count) =>
			{
				//rule 4 - deleting a join that is not there returns 0 and throws nothing
				count.ShouldBe(0);
				da.GetAll(null).Count.ShouldBe(totalBefore);
				CountMatching(da, pair).ShouldBe(0);
			}
			);
		}

		[Fact]
		public void ShouldReturnZeroOnASecondDeleteOfTheSameCompanyResource()
		{
			//setup
			var test = Setup_DeleteCompanyResource_TestSecondDelete(_da);

			//act
			var count = _da.Delete(test.CompanyResource);

			//assert
			test.Assert(count);
		}

		[Fact]
		public void ShouldReturnZeroWhenDeletingACompanyResourceThatWasNeverStored()
		{
			//setup - no precondition: the point is a pair that matches nothing
			var phantom = NewCompanyResource;
			var totalBefore = _da.GetAll(null).Count;

			//act
			var count = _da.Delete(phantom);

			//assert - rule 4
			count.ShouldBe(0);
			_da.GetAll(null).Count.ShouldBe(totalBefore);
		}

		#endregion

		#region GetAll - rules 5, 6

		public delegate void GetAllAssertion(IList<CompanyResource> all);
		public static (CompanyResource CompanyResource, GetAllAssertion Assert) Setup_InsertCompanyResource_TestGetAll(ICompanyResourceDao da)
		{
			var pair = NewCompanyResource;
			da.Insert(pair);

			return (pair, (all) =>
			{
				//rule 5 - always a list
				all.ShouldNotBeNull();
				all.Count(x => x.CompanyId == pair.CompanyId && x.ResourceId == pair.ResourceId).ShouldBe(1);
			}
			);
		}

		[Fact]
		public void ShouldGetAllCompanyResources()
		{
			//setup
			var test = Setup_InsertCompanyResource_TestGetAll(_da);

			//act
			var all = _da.GetAll(new CompanyResource());

			//assert
			test.Assert(all);
		}

		[Fact]
		public void ShouldGetAllCompanyResourcesWhenGivenNull()
		{
			//setup
			var test = Setup_InsertCompanyResource_TestGetAll(_da);

			//act - rule 6, this is exactly how the generic dispatcher calls it
			var all = _da.GetAll(null);

			//assert
			test.Assert(all);
		}

		#endregion

		#region Null arguments - rules 6, 7

		[Fact]
		public void ShouldThrowWhenInsertIsGivenNull()
		{
			//act & assert - rule 7
			Should.Throw<ArgumentNullException>(() => _da.Insert(null));
		}

		[Fact]
		public void ShouldThrowWhenDeleteIsGivenNull()
		{
			//act & assert - rule 7
			Should.Throw<ArgumentNullException>(() =>
			{
				_da.Delete(null);
			});
		}

		#endregion
	}
}
