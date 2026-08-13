using ProphetsWay.Example.DataAccess.Entities;

using System;
using System.Threading;

namespace ProphetsWay.Example.DataAccess.NoDB
{
	/// <summary>
	/// Ignore this class, I created it to be an in-memory database for use in the example.
	/// 
	/// In your implementation, this could be considered your database context (ex: Entity Framework implementation)
	/// </summary>
	/// <remarks>
	/// Each table is a <see cref="StoreTable{TKey, TEntity}"/> rather than a bare dictionary. That is what makes
	/// every write in this project pass through one place and hand over a <see cref="TransactionLog"/> on the way,
	/// which is how a rollback knows what there is to reverse.
	/// </remarks>
	internal static class DataStore
	{
		public static readonly StoreTable<int, Job> Jobs = new StoreTable<int, Job>(CopyJob);

		public static readonly StoreTable<int, Company> Companies = new StoreTable<int, Company>(CopyCompany);

		public static readonly StoreTable<int, User> Users = new StoreTable<int, User>(CopyUser);

		public static readonly StoreTable<long, Transaction> Transactions = new StoreTable<long, Transaction>(CopyTransaction);

		public static readonly StoreTable<Guid, Resource> Resources = new StoreTable<Guid, Resource>(CopyResource);

		/// <summary>
		/// Departments are soft-deleted, so nothing is ever taken out of here. A dictionary that is only ever
		/// added to enumerates in insertion order, which is what gives rule 11 its stable ordering.
		/// </summary>
		public static readonly StoreTable<int, Department> Departments = new StoreTable<int, Department>(CopyDepartment);

		/// <summary>
		/// A <see cref="CompanyResource"/> has no identifier, so there is no key to hold it under - the natural
		/// key is the (CompanyId, ResourceId) pair and matching is done by scanning for it.
		/// </summary>
		public static readonly StoreList<CompanyResource> CompanyResources = new StoreList<CompanyResource>(CopyCompanyResource);

		private static int _lastCompanyId;

		private static int _lastJobId;

		private static int _lastUserId;

		private static int _lastDepartmentId;

		private static long _lastTransactionId;

		/// <summary>
		/// The generated surrogate keys a real database would hand back from an identity column. Sequential
		/// rather than random, so two inserts can never collide however many threads are inserting.
		/// </summary>
		/// <remarks>
		/// <see cref="Interlocked"/> rather than a lock because these are read by Daos holding four different
		/// table locks, and a counter guarded by whichever lock the caller happened to be holding is not guarded
		/// at all. A <see cref="Resource"/> has no counter here - its identifier is a
		/// <see cref="Guid"/> the Dao generates, which is the other shape a real database offers.
		/// </remarks>
		public static int NextCompanyId()
		{
			return Interlocked.Increment(ref _lastCompanyId);
		}

		/// <inheritdoc cref="NextCompanyId"/>
		public static int NextJobId()
		{
			return Interlocked.Increment(ref _lastJobId);
		}

		/// <inheritdoc cref="NextCompanyId"/>
		public static int NextUserId()
		{
			return Interlocked.Increment(ref _lastUserId);
		}

		/// <inheritdoc cref="NextCompanyId"/>
		public static int NextDepartmentId()
		{
			return Interlocked.Increment(ref _lastDepartmentId);
		}

		/// <inheritdoc cref="NextCompanyId"/>
		public static long NextTransactionId()
		{
			return Interlocked.Increment(ref _lastTransactionId);
		}

		#region Snapshots

		/*
		 * The copy each table above hands to its StoreTable, and the whole of this implementation's answer to the
		 * snapshot rule on IExampleDataAccess. Read these before changing a Dao.
		 *
		 * EVERY COPY BELOW IS DEEP, and that is the part worth studying. A copy one level down would leave the
		 * stored graph reachable through a navigation property on a snapshot - a caller holding a returned User
		 * could edit the stored Company through user.Company - which is what the rule denies and what no database
		 * can do. So every reference-typed property is copied at every level: CopyUser copies the three entities a
		 * user names, and CopyTransaction copies a whole user through CopyUser rather than reaching one level down
		 * and assigning that user's Company by reference.
		 *
		 * THERE IS NO IDENTITY MAP, deliberately. Two copies of one stored row are two objects. A dictionary-backed
		 * store is naturally written to hand the single Company it holds to every user that names it, and the rule
		 * forbids exactly that: editing userA.Company must change neither userB.Company nor the store, even where
		 * both name the same company.
		 *
		 * The graph is finite and acyclic - Transaction is the deepest at two levels, User at one, everything else
		 * scalars only - so a plain recursive copy terminates without a visited set to keep it honest.
		 *
		 * Each one maps null to null, because a navigation property is optional and a copy of "no company" is
		 * "no company".
		 */

		private static Job CopyJob(Job source)
		{
			if (source == null)
				return null;

			return new Job
			{
				Id = source.Id,
				Name = source.Name,
				Something = source.Something
			};
		}

		private static Company CopyCompany(Company source)
		{
			if (source == null)
				return null;

			return new Company
			{
				Id = source.Id,
				Name = source.Name,
				Other = source.Other
			};
		}

		/// <summary>
		/// One level deep: the three entities a user names are copied, not shared.
		/// </summary>
		private static User CopyUser(User source)
		{
			if (source == null)
				return null;

			return new User
			{
				Id = source.Id,
				Name = source.Name,
				Company = CopyCompany(source.Company),
				Job = CopyJob(source.Job),
				Department = CopyDepartment(source.Department),
				Whatever = source.Whatever,
				RoleStr = source.RoleStr,
				RoleInt = source.RoleInt
			};
		}

		/// <summary>
		/// Two levels deep, and the one place this can go quietly wrong.
		/// </summary>
		/// <remarks>
		/// The user is copied through <see cref="CopyUser"/>, so the company, job and department hanging off it
		/// are copied too. Copying the user's fields here instead - and assigning its <c>Company</c> by
		/// reference - would satisfy every one-level test and still leak the stored company at the second slot.
		/// </remarks>
		private static Transaction CopyTransaction(Transaction source)
		{
			if (source == null)
				return null;

			return new Transaction
			{
				Id = source.Id,
				DateOfAction = source.DateOfAction,
				Amount = source.Amount,
				User = CopyUser(source.User),
				Company = CopyCompany(source.Company)
			};
		}

		private static Resource CopyResource(Resource source)
		{
			if (source == null)
				return null;

			return new Resource
			{
				Id = source.Id,
				Name = source.Name,
				Description = source.Description
			};
		}

		/// <summary>
		/// A field-for-field copy. <see cref="DateTime"/> is a value type, so <see cref="DateTime.Kind"/>
		/// crosses with the value and a stamp is still Coordinated Universal Time on the far side - rule 18.
		/// </summary>
		private static Department CopyDepartment(Department source)
		{
			if (source == null)
				return null;

			return new Department
			{
				Id = source.Id,
				Name = source.Name,
				Description = source.Description,
				CreatedDate = source.CreatedDate,
				UpdatedDate = source.UpdatedDate,
				DeletedDate = source.DeletedDate
			};
		}

		/// <summary>
		/// The foreign keys are an <see cref="int"/> and a <see cref="Guid"/> held by value, so this join has no
		/// second level to reach - it names its company and its resource rather than referencing them.
		/// </summary>
		private static CompanyResource CopyCompanyResource(CompanyResource source)
		{
			if (source == null)
				return null;

			return new CompanyResource
			{
				CompanyId = source.CompanyId,
				ResourceId = source.ResourceId
			};
		}

		#endregion
	}
}
