using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class CompanyDao : BaseDao, ICompanyDao
	{
		public CompanyDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		public int Delete(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
				DataStore.Companies.Remove(CurrentTransaction, item.Id);

			return 1;
		}

		public Company Get(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
				if (DataStore.Companies.TryGet(item.Id, out var stored))
					return stored;

			return null;
		}

		public Company GetCustomCompanyFunction(int id)
		{
			lock (DataStore.Companies.SyncRoot)
			{
				var index = id % DataStore.Companies.Count;
				return DataStore.Companies.Rows.Skip(index).First();
			}
		}

		public IList<Company> GetPaged(Company item, int skip, int take)
		{
			lock (DataStore.Companies.SyncRoot)
				return DataStore.Companies.Rows.Skip(skip).Take(take).ToList();
		}

		public void Insert(Company item)
		{
			lock (DataStore.Companies.SyncRoot)
			{
				item.Id = Random.Next();
				DataStore.Companies.Add(CurrentTransaction, item.Id, item);
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
				DataStore.Companies.Save(CurrentTransaction, item.Id, item);

			return 1;
		}
	}
}
