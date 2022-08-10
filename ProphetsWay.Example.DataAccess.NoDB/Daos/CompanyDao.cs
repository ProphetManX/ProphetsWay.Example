using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class CompanyDao : BaseDao, ICompanyDao
	{
		public int Delete(Company item)
		{
			lock (DataStore.Companies)
				DataStore.Companies.Remove(item.Id);

			return 1;
		}

		public Company Get(Company item)
		{
			lock (DataStore.Companies)
				if (DataStore.Companies.ContainsKey(item.Id))
					return DataStore.Companies[item.Id];

			return null;
		}

		public Company GetCustomCompanyFunction(int id)
		{
			lock (DataStore.Companies)
			{
				var index = id % DataStore.Companies.Count;
				return DataStore.Companies.Values.Skip(index).First();
			}
		}

		public IList<Company> GetPaged(Company item, int skip, int take)
		{
			lock (DataStore.Companies)
				return DataStore.Companies.Values.Skip(skip).Take(take).ToList();
		}

		public void Insert(Company item)
		{
			lock (DataStore.Companies)
			{
				item.Id = Random.Next();
				DataStore.Companies.Add(item.Id, item);
			}
		}

		public int GetCount(Company item)
		{
			lock (DataStore.Companies)
				return DataStore.Companies.Count;
		}

		public int Update(Company item)
		{
			DataStore.Companies[item.Id] = item;
			return 1;
		}
	}
}
