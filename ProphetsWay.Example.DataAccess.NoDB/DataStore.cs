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

		private static int _lastDepartmentId;

		/// <summary>
		/// The generated surrogate key a real database would hand back from an identity column. Sequential
		/// rather than random, so two inserts can never collide.
		/// </summary>
		public static int NextDepartmentId()
		{
			return Interlocked.Increment(ref _lastDepartmentId);
		}

		private static Job CopyJob(Job source)
		{
			return new Job
			{
				Id = source.Id,
				Name = source.Name,
				Something = source.Something
			};
		}

		private static Company CopyCompany(Company source)
		{
			return new Company
			{
				Id = source.Id,
				Name = source.Name,
				Other = source.Other
			};
		}

		private static User CopyUser(User source)
		{
			return new User
			{
				Id = source.Id,
				Name = source.Name,
				Company = source.Company,
				Job = source.Job,
				Department = source.Department,
				Whatever = source.Whatever,
				RoleStr = source.RoleStr,
				RoleInt = source.RoleInt
			};
		}

		private static Transaction CopyTransaction(Transaction source)
		{
			return new Transaction
			{
				Id = source.Id,
				DateOfAction = source.DateOfAction,
				Amount = source.Amount,
				User = source.User,
				Company = source.Company
			};
		}

		private static Resource CopyResource(Resource source)
		{
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

		private static CompanyResource CopyCompanyResource(CompanyResource source)
		{
			return new CompanyResource
			{
				CompanyId = source.CompanyId,
				ResourceId = source.ResourceId
			};
		}
	}
}
