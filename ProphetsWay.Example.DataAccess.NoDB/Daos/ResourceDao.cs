using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="IResourceDao"/>, and the one Dao here whose identifier is a
	/// <see cref="Guid"/> the Dao generates rather than a counter the store hands out.
	/// </summary>
	/// <remarks>
	/// It copies on the way in and on the way out, as the snapshot rule on <see cref="IExampleDataAccess"/>
	/// requires of every Dao here. A <see cref="Resource"/> carries scalars only, so its copy has no second level
	/// to reach - see <see cref="DataStore"/> for the ones that do.
	/// </remarks>
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
				return DataStore.Resources.TryGet(item.Id, out var stored) ? Copy(stored) : null;
		}

		public IList<Resource> GetAll(Resource item)
		{
			lock (DataStore.Resources.SyncRoot)
				return DataStore.Resources.Rows.Select(Copy).ToList();
		}

		public void Insert(Resource item)
		{
			lock (DataStore.Resources.SyncRoot)
			{
				//generated inside the lock like every other identifier here, so the assignment and the write it
				//keys cannot be split by another thread
				item.Id = Guid.NewGuid();
				DataStore.Resources.Add(CurrentTransaction, item.Id, Copy(item));
			}
		}

		public int Update(Resource item)
		{
			lock (DataStore.Resources.SyncRoot)
			{
				//the count of rows the write actually changed, as a database would report it
				if (!DataStore.Resources.TryGet(item.Id, out _))
					return 0;

				DataStore.Resources.Save(CurrentTransaction, item.Id, Copy(item));

				return 1;
			}
		}

		private static Resource Copy(Resource source)
		{
			return DataStore.Resources.Copy(source);
		}
	}
}
