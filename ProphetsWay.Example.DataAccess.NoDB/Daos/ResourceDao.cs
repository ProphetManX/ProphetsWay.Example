using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class ResourceDao : IResourceDao
	{
		public int Delete(Resource item)
		{
			lock (DataStore.Resources)
				return DataStore.Resources.Remove(item.Id) ? 1 : 0;
		}

		public Resource Get(Resource item)
		{
			lock (DataStore.Resources)
				if (DataStore.Resources.ContainsKey(item.Id))
					return DataStore.Resources[item.Id];

			return null;
		}

		public IList<Resource> GetAll(Resource item)
		{
			lock (DataStore.Resources)
				return DataStore.Resources.Values.ToList();
		}

		public void Insert(Resource item)
		{
			item.Id = Guid.NewGuid();

			lock (DataStore.Resources)
				DataStore.Resources.Add(item.Id, item);
		}

		public int Update(Resource item)
		{
			DataStore.Resources[item.Id] = item;
			return 1;
		}
	}
}
