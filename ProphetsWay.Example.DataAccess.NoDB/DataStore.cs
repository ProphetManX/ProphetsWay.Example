using ProphetsWay.Example.DataAccess.Entities;

using System;
using System.Collections.Generic;
using System.Threading;

namespace ProphetsWay.Example.DataAccess.NoDB
{
	/// <summary>
	/// Ignore this class, I created it to be an in-memory database for use in the example.
	/// 
	/// In your implementation, this could be considered your database context (ex: Entity Framework implementation)
	/// </summary>
	internal static class DataStore
	{
		public static readonly Dictionary<int, Job> Jobs = new Dictionary<int, Job>();

		public static readonly Dictionary<int, Company> Companies = new Dictionary<int, Company>();

		public static readonly Dictionary<int, User> Users = new Dictionary<int, User>();

		public static readonly Dictionary<long, Transaction> Transactions = new Dictionary<long, Transaction>();

		public static readonly Dictionary<Guid, Resource> Resources = new Dictionary<Guid, Resource>();

		/// <summary>
		/// Departments are soft-deleted, so nothing is ever taken out of here. A dictionary that is only ever
		/// added to enumerates in insertion order, which is what gives rule 11 its stable ordering.
		/// </summary>
		public static readonly Dictionary<int, Department> Departments = new Dictionary<int, Department>();

		/// <summary>
		/// A <see cref="CompanyResource"/> has no identifier, so there is no key to hold it under - the natural
		/// key is the (CompanyId, ResourceId) pair and matching is done by scanning for it.
		/// </summary>
		public static readonly List<CompanyResource> CompanyResources = new List<CompanyResource>();

		private static int _lastDepartmentId;

		/// <summary>
		/// The generated surrogate key a real database would hand back from an identity column. Sequential
		/// rather than random, so two inserts can never collide.
		/// </summary>
		public static int NextDepartmentId()
		{
			return Interlocked.Increment(ref _lastDepartmentId);
		}
	}
}
