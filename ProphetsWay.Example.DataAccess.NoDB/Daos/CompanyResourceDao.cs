using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="ICompanyResourceDao"/> - three operations, no identifier,
	/// and every match made on the (CompanyId, ResourceId) pair.
	/// </summary>
	/// <remarks>
	/// Like every Dao here it copies on the way in and on the way out. Rule 9 of
	/// <see cref="ICompanyResourceDao"/> is why: the list <c>GetAll</c> hands back and every join in it are
	/// snapshots, so a caller rewriting either of them cannot move a stored row from one pair to another without
	/// going through <c>Insert</c> and <c>Delete</c>. Its two foreign keys are held by value, so the copy has no
	/// second level to reach.
	/// </remarks>
	internal class CompanyResourceDao : BaseDao, ICompanyResourceDao
	{
		public CompanyResourceDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		/// <inheritdoc />
		public void Insert(CompanyResource item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			lock (DataStore.CompanyResources.SyncRoot)
			{
				//rule 3 - at most one row per pair, and a duplicate is a silent no-op rather than an error
				if (Find(item.CompanyId, item.ResourceId) != null)
					return;

				//rule 9 - the store gets a copy, so the caller's instance is read and not adopted
				DataStore.CompanyResources.Add(CurrentTransaction, Copy(item));
			}
		}

		/// <inheritdoc />
		public int Delete(CompanyResource item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			lock (DataStore.CompanyResources.SyncRoot)
			{
				//rule 1 - matching is on the pair, so a join sharing only one side of it is left alone
				var stored = Find(item.CompanyId, item.ResourceId);

				if (stored == null)
					return 0;

				//rule 4 - a hard delete, and rule 3 means there was never more than one to remove
				DataStore.CompanyResources.Remove(CurrentTransaction, stored);
				return 1;
			}
		}

		/// <inheritdoc />
		public IList<CompanyResource> GetAll(CompanyResource item)
		{
			//rule 6 - item is a type selector and is null when the call arrives through the dispatcher
			lock (DataStore.CompanyResources.SyncRoot)
				return DataStore.CompanyResources.Rows.Select(Copy).ToList();
		}

		/// <summary>
		/// The stored join for a pair, or <c>null</c>. Call only while holding the store's lock.
		/// </summary>
		private static CompanyResource Find(int companyId, Guid resourceId)
		{
			return DataStore.CompanyResources.Rows.FirstOrDefault(x => x.CompanyId == companyId && x.ResourceId == resourceId);
		}

		private static CompanyResource Copy(CompanyResource source)
		{
			return DataStore.CompanyResources.Copy(source);
		}
	}
}
