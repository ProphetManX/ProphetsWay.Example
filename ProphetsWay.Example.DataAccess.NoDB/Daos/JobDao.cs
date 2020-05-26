using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class JobDao : IJobDao
	{
		public int Delete(Job item)
		{
			lock (DataStore.Jobs)
				DataStore.Jobs.Remove(item.Id);

			return 1;
		}

		public Job Get(Job item)
		{
			lock (DataStore.Jobs)
				if (DataStore.Jobs.ContainsKey(item.Id))
					return DataStore.Jobs[item.Id];

			return null;
		}

		public IList<Job> GetAll(Job item)
		{
			lock (DataStore.Jobs)
				return DataStore.Jobs.Values.ToList();
		}

		public void Insert(Job item)
		{
			lock (DataStore.Jobs)
			{
				item.Id = DataStore.Jobs.Keys.Count > 0
				? DataStore.Jobs.Keys.Max() + 1
				: 1;

				DataStore.Jobs.Add(item.Id, item);
			}
		}

		public int Update(Job item)
		{
			DataStore.Jobs[item.Id] = item;
			return 1;
		}
	}
}
