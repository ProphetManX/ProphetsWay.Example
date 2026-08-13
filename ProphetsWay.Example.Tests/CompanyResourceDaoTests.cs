using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The contract of <see cref="ICompanyResourceDao"/> - an entity with no identifier, and a Dao that
	/// inherits <c>IBaseDao&lt;T&gt;</c> not at all. Rule numbers refer to the numbered CONTRACT list on
	/// <see cref="ICompanyResourceDao"/>. <see cref="CompanyResourceDataAccessTests"/> replays these helpers
	/// through the generic dispatcher and shows where that path stops working.
	/// </summary>
	[Collection(TestCollections.SharedStore)]
	[Trait("Scope", "Contract")]
	public class CompanyResourceDaoTests : BaseUnitTests<ICompanyResourceDao>
	{
		/// <summary>
		/// The same instance, seen as the aggregate, so an arrangement can create the company and the resource a
		/// join is about to name.
		/// </summary>
		/// <remarks>
		/// The helpers below are handed <see cref="ICompanyResourceDao"/> because that is the interface whose
		/// contract they exercise, and because <c>GetAll(null)</c> is ambiguous on the aggregate. Every Data
		/// Access Layer in this suite comes from <see cref="TestDataAccessFactory"/> and is therefore an
		/// <see cref="IExampleDataAccess"/>; the check is here so that ceasing to be one reports itself rather
		/// than surfacing as an <see cref="InvalidCastException"/> from an arrangement.
		/// </remarks>
		private static IExampleDataAccess Aggregate(ICompanyResourceDao da)
		{
			var owner = da as IExampleDataAccess;

			if (owner == null)
				throw new InvalidOperationException(
					$"{da.GetType().FullName} is not an {nameof(IExampleDataAccess)}, so this test cannot create the " +
					"company and resource rows rule 10 requires a join to name.");

			return owner;
		}

		/// <summary>A company row that really exists, and its identifier.</summary>
		public static int InsertCompany(IExampleDataAccess da)
		{
			var co = new Company { Name = $"CompanyResource host {Guid.NewGuid()}" };
			da.Insert(co);

			return co.Id;
		}

		/// <summary>A resource row that really exists, and its identifier.</summary>
		public static Guid InsertResource(IExampleDataAccess da)
		{
			var res = new Resource { Name = $"CompanyResource host {Guid.NewGuid()}" };
			da.Insert(res);

			return res.Id;
		}

		/// <summary>
		/// A join naming a company that exists and a resource that exists, which rule 10 requires of every
		/// caller. The rows are created here; the join itself is not stored until something inserts it.
		/// </summary>
		/// <remarks>
		/// Rule 10 binds the caller, not the store. This implementation is explicitly not required to check, and
		/// would accept a synthetic identifier quite happily - which is exactly why the arrangement has to create
		/// the rows anyway. A store that enforces referential integrity rejects the synthetic version, and an
		/// arrangement built on that leniency is a suite that only ever passes against one implementation.
		/// </remarks>
		public static CompanyResource NewCompanyResource(ICompanyResourceDao da)
		{
			var owner = Aggregate(da);

			return new CompanyResource { CompanyId = InsertCompany(owner), ResourceId = InsertResource(owner) };
		}

		public static int CountMatching(ICompanyResourceDao da, CompanyResource pair)
		{
			return da.GetAll(null).Count(x => x.CompanyId == pair.CompanyId && x.ResourceId == pair.ResourceId);
		}

		#region Insert - rules 1, 2, 3

		public delegate void InsertAssertion(CompanyResource pair);
		public static (CompanyResource CompanyResource, InsertAssertion Assert) Setup_CreateCompanyResource_TestInsert(ICompanyResourceDao da)
		{
			var pair = NewCompanyResource(da);
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
			var stored = NewCompanyResource(da);
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
			var owner = Aggregate(da);

			var target = NewCompanyResource(da);
			da.Insert(target);

			//Two neighbours, each sharing one side of the pair. Rule 1 says neither may be caught by a
			//delete that matches on the pair. The side each does not share is a row of its own, because rule 10
			//binds these two calls exactly as it binds the one above.
			var sameCompany = new CompanyResource { CompanyId = target.CompanyId, ResourceId = InsertResource(owner) };
			da.Insert(sameCompany);
			var sameResource = new CompanyResource { CompanyId = InsertCompany(owner), ResourceId = target.ResourceId };
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
			var pair = NewCompanyResource(da);
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
			//setup - no precondition beyond rule 10: a company and a resource that exist, never joined to each other
			var phantom = NewCompanyResource(_da);
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
			var pair = NewCompanyResource(da);
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
		/// reason: <see cref="TestCollections.SharedStore"/> runs its classes one at a time, and every other
		/// test in the suite creates the joins it needs.
		/// </summary>
		public delegate void EmptyViewAssertion(IList<CompanyResource> all);
		public static EmptyViewAssertion Setup_DeleteEveryCompanyResource_TestEmptyGetAll(ICompanyResourceDao da)
		{
			//Give the delete something to bite on, then clear the store out.
			da.Insert(NewCompanyResource(da));
			da.Insert(NewCompanyResource(da));

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
			var owner = Aggregate(da);

			var pair = NewCompanyResource(da);
			da.Insert(pair);

			var companyId = pair.CompanyId;
			var resourceId = pair.ResourceId;

			var all = da.GetAll(null);
			var countBefore = all.Count;

			//Rewrite a join the list handed back, into a pair the store does not hold. Both sides are real rows,
			//so the only thing absent from the store is the join itself - which is all the assertion is about.
			var mine = all.Single(x => x.CompanyId == companyId && x.ResourceId == resourceId);
			mine.CompanyId = InsertCompany(owner);
			mine.ResourceId = InsertResource(owner);
			var rewrittenCompanyId = mine.CompanyId;
			var rewrittenResourceId = mine.ResourceId;

			//And rewrite the list itself. An implementation is free to return a fixed-size or otherwise
			//unmodifiable list, and refusing the mutation honours rule 9 just as well as absorbing it - so
			//this one throw is tolerated, and no other in this file is.
			var added = NewCompanyResource(da);
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
			var pair = NewCompanyResource(da);
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
			var rewrite = NewCompanyResource(_da);

			//act - the rewrite is deliberately after the call returns, so nothing about it may reach the store
			_da.Insert(test.CompanyResource);
			test.CompanyResource.CompanyId = rewrite.CompanyId;
			test.CompanyResource.ResourceId = rewrite.ResourceId;

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
