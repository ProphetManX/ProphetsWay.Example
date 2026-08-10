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

		/// <summary>
		/// Rule 5's other half. Every other assertion in this class about <c>GetAll</c> runs against a store
		/// that has something in it, so the most common way to break the rule - returning <c>null</c>, or a
		/// <c>null</c>-yielding query result, when there is nothing to return - is never reached. This is the
		/// same shape as <c>Setup_DeleteEveryDepartment_TestEmptyViews</c>, and it is safe for the same
		/// reason: the <see cref="TestCollections.CompanyResources"/> collection runs its classes one at a
		/// time, and every other test in it creates the joins it needs.
		/// </summary>
		public delegate void EmptyViewAssertion(IList<CompanyResource> all);
		public static EmptyViewAssertion Setup_DeleteEveryCompanyResource_TestEmptyGetAll(ICompanyResourceDao da)
		{
			//Give the delete something to bite on, then clear the store out.
			da.Insert(NewCompanyResource);
			da.Insert(NewCompanyResource);

			foreach (var pair in da.GetAll(null).ToList())
				da.Delete(pair);

			return (all) =>
			{
				//rule 5 - an empty list, and specifically not null
				all.ShouldNotBeNull();
				all.ShouldBeEmpty();
			};
		}

		[Fact]
		public void ShouldReturnAnEmptyListWhenNoCompanyResourcesAreStored()
		{
			//setup
			var assertion = Setup_DeleteEveryCompanyResource_TestEmptyGetAll(_da);

			//act
			var all = _da.GetAll(new CompanyResource());

			//assert
			assertion(all);
		}

		#endregion

		#region Snapshots - rule 9

		/// <summary>
		/// Rule 9, stated on its own rather than leaned on. Every count assertion in this class reads the
		/// store through <c>GetAll</c>, so if that hands back the store's own list and the store's own
		/// entities, a caller can silently rewrite the join table without ever calling <c>Insert</c> or
		/// <c>Delete</c> - and no database-backed implementation could reproduce that, which is exactly
		/// the interchangeability claim this repository makes.
		/// </summary>
		public delegate void SnapshotAssertion(IList<CompanyResource> refetched);
		public static (CompanyResource CompanyResource, SnapshotAssertion Assert) Setup_InsertCompanyResource_TestGetAllReturnsSnapshots(ICompanyResourceDao da)
		{
			var pair = NewCompanyResource;
			da.Insert(pair);

			var companyId = pair.CompanyId;
			var resourceId = pair.ResourceId;

			var all = da.GetAll(null);
			var countBefore = all.Count;

			//Rewrite a join the list handed back, into a pair that names nothing stored.
			var mine = all.Single(x => x.CompanyId == companyId && x.ResourceId == resourceId);
			mine.CompanyId = NextCompanyId;
			mine.ResourceId = Guid.NewGuid();
			var rewrittenCompanyId = mine.CompanyId;
			var rewrittenResourceId = mine.ResourceId;

			//And rewrite the list itself. An implementation is free to return a fixed-size or otherwise
			//unmodifiable list, and refusing the mutation honours rule 9 just as well as absorbing it - so
			//this one throw is tolerated, and no other in this file is.
			var added = new CompanyResource { CompanyId = NextCompanyId, ResourceId = Guid.NewGuid() };
			try
			{
				all.Add(added);
				all.Remove(mine);
			}
			catch (NotSupportedException)
			{
			}

			return (pair, (refetched) =>
			{
				//rule 9 - two lists retrieved separately are independent of each other and of the store
				refetched.ShouldNotBeNull();
				refetched.ShouldNotBeSameAs(all);

				//rule 9 - the store never saw any of it: same size, original pair intact, neither the
				//rewritten pair nor the added one anywhere in sight
				refetched.Count.ShouldBe(countBefore);
				refetched.Count(x => x.CompanyId == companyId && x.ResourceId == resourceId).ShouldBe(1);
				refetched.Any(x => x.CompanyId == rewrittenCompanyId && x.ResourceId == rewrittenResourceId).ShouldBeFalse();
				refetched.Any(x => x.CompanyId == added.CompanyId && x.ResourceId == added.ResourceId).ShouldBeFalse();
			}
			);
		}

		[Fact]
		public void ShouldNotStoreEditsMadeToTheListGetAllReturned()
		{
			//setup
			var test = Setup_InsertCompanyResource_TestGetAllReturnsSnapshots(_da);

			//act - neither Insert nor Delete is called between the edits and this read
			var refetched = _da.GetAll(new CompanyResource());

			//assert
			test.Assert(refetched);
		}

		/// <summary>
		/// Rule 9's other half. The test above proves a join coming <i>out</i> of the Data Access Layer is a
		/// copy; this one proves a join going <i>in</i> is read rather than adopted. A Data Access Layer that
		/// clones on read and then keeps the caller's own instance on write passes every other test in this
		/// class, and lets a caller move a stored row from one pair to another without ever calling
		/// <see cref="ICompanyResourceDao.Insert"/> or <see cref="ICompanyResourceDao.Delete"/>.
		/// </summary>
		public delegate void InsertAdoptionAssertion(CompanyResource rewritten);
		public static (CompanyResource CompanyResource, InsertAdoptionAssertion Assert) Setup_CreateCompanyResource_TestInsertDoesNotAdoptTheArgument(ICompanyResourceDao da)
		{
			var pair = NewCompanyResource;
			var companyId = pair.CompanyId;
			var resourceId = pair.ResourceId;

			return (pair, (rewritten) =>
			{
				//rule 9 - Insert read the pair as it stood at the moment of the call
				CountMatching(da, new CompanyResource { CompanyId = companyId, ResourceId = resourceId }).ShouldBe(1);

				//rule 9 - and the rewrite that followed it names a pair the store has never held
				CountMatching(da, rewritten).ShouldBe(0);
			}
			);
		}

		[Fact]
		public void ShouldNotStoreEditsMadeToACompanyResourceAfterInsertReturned()
		{
			//setup
			var test = Setup_CreateCompanyResource_TestInsertDoesNotAdoptTheArgument(_da);

			//act - the rewrite is deliberately after the call returns, so nothing about it may reach the store
			_da.Insert(test.CompanyResource);
			test.CompanyResource.CompanyId = NextCompanyId;
			test.CompanyResource.ResourceId = Guid.NewGuid();

			//assert
			test.Assert(test.CompanyResource);
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
