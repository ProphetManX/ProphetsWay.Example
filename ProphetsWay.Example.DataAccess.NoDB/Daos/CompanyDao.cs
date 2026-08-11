using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="ICompanyDao"/>.
	/// </summary>
	/// <remarks>
	/// It copies on the way in and on the way out, as the snapshot rule on <see cref="IExampleDataAccess"/>
	/// requires of every Dao here: <c>Get</c>, <c>GetPaged</c> and <c>GetCustomCompanyFunction</c> hand out
	/// snapshots, and <c>Insert</c>, <c>Update</c> and <c>Delete</c> read their argument rather than adopting it.
	/// A <see cref="Company"/> carries scalars only, so its copy has no second level to reach - see
	/// <see cref="DataStore"/> for the ones that do.
	/// </remarks>
	internal class CompanyDao : BaseDao, ICompanyDao
	{
		public CompanyDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		public int Delete(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
				return DataStore.Companies.Remove(CurrentTransaction, item.Id) ? 1 : 0;
		}

		public Company Get(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
				return DataStore.Companies.TryGet(item.Id, out var stored) ? Copy(stored) : null;
		}

		/// <summary>
		/// Stands in for whatever query a real Data Access Object would add beyond the surface it inherits:
		/// <paramref name="id"/> picks a company by position rather than by identifier, wrapping round the end of
		/// the set.
		/// </summary>
		/// <returns>The selected company as a snapshot, or <c>null</c> when no company is stored.</returns>
		public Company GetCustomCompanyFunction(int id)
		{
			lock (DataStore.Companies.SyncRoot)
			{
				var count = DataStore.Companies.Count;

				//nothing stored means nothing to pick, rather than a division by zero
				if (count == 0)
					return null;

				//the remainder of a negative id is negative, and a negative Skip is silently a zero - so the
				//wrap is brought back into range here rather than left to look like it worked
				var index = ((id % count) + count) % count;

				return Copy(DataStore.Companies.Rows.Skip(index).First());
			}
		}

		public IList<Company> GetPaged(Company item, int skip, int take)
		{
			lock (DataStore.Companies.SyncRoot)
				return DataStore.Companies.Rows.Skip(skip).Take(take).Select(Copy).ToList();
		}

		public void Insert(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
			{
				//the generated identifier is the one value that travels back onto the caller's instance; the
				//store gets a copy, so nothing else about that instance is adopted
				item.Id = DataStore.NextCompanyId();
				DataStore.Companies.Add(CurrentTransaction, item.Id, Copy(item));
			}
		}

		public int GetCount(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
				return DataStore.Companies.Count;
		}

		public int Update(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
			{
				//the count of rows the write actually changed, as a database would report it
				if (!DataStore.Companies.TryGet(item.Id, out _))
					return 0;

				DataStore.Companies.Save(CurrentTransaction, item.Id, Copy(item));

				return 1;
			}
		}

		private static Company Copy(Company source)
		{
			return DataStore.Companies.Copy(source);
		}
	}
}
