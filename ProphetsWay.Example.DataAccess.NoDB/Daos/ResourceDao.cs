using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class ResourceDao : BaseDao, IResourceDao
	{
		public ResourceDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		public int Delete(Resource item)
		{
			lock (DataStore.Resources.SyncRoot)
				return DataStore.Resources.Remove(CurrentTransaction, item.Id) ? 1 : 0;
		}

		public Resource Get(Resource item)
		{
			lock (DataStore.Resources.SyncRoot)
				if (DataStore.Resources.TryGet(item.Id, out var stored))
					return stored;

			return null;
		}

		public IList<Resource> GetAll(Resource item)
		{
			lock (DataStore.Resources.SyncRoot)
				return DataStore.Resources.Rows.ToList();
		}

		public void Insert(Resource item)
		{
			item.Id = Guid.NewGuid();

			lock (DataStore.Resources.SyncRoot)
				DataStore.Resources.Add(CurrentTransaction, item.Id, item);
		}

		public int Update(Resource item)
		{
			lock (DataStore.Resources.SyncRoot)
				DataStore.Resources.Save(CurrentTransaction, item.Id, item);

			return 1;
		}
	}
}
