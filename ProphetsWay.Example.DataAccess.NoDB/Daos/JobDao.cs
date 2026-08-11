using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class JobDao : BaseDao, IJobDao
	{
		public JobDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		public int Delete(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
				DataStore.Jobs.Remove(CurrentTransaction, item.Id);

			return 1;
		}

		public Job Get(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
				if (DataStore.Jobs.TryGet(item.Id, out var stored))
					return stored;

			return null;
		}

		public IList<Job> GetAll(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
				return DataStore.Jobs.Rows.ToList();
		}

		public void Insert(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
			{
				item.Id = Random.Next(int.MaxValue);

				DataStore.Jobs.Add(CurrentTransaction, item.Id, item);
			}
		}

		public int Update(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
				DataStore.Jobs.Save(CurrentTransaction, item.Id, item);

			return 1;
		}
	}
}
