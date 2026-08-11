using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="IJobDao"/>.
	/// </summary>
	/// <remarks>
	/// It copies on the way in and on the way out, as the snapshot rule on <see cref="IExampleDataAccess"/>
	/// requires of every Dao here. A <see cref="Job"/> carries scalars only, so its copy has no second level to
	/// reach - see <see cref="DataStore"/> for the ones that do.
	/// </remarks>
	internal class JobDao : BaseDao, IJobDao
	{
		public JobDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		public int Delete(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
				return DataStore.Jobs.Remove(CurrentTransaction, item.Id) ? 1 : 0;
		}

		public Job Get(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
				return DataStore.Jobs.TryGet(item.Id, out var stored) ? Copy(stored) : null;
		}

		public IList<Job> GetAll(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
				return DataStore.Jobs.Rows.Select(Copy).ToList();
		}

		public void Insert(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
			{
				//the generated identifier is the one value that travels back onto the caller's instance; the
				//store gets a copy, so nothing else about that instance is adopted
				item.Id = DataStore.NextJobId();
				DataStore.Jobs.Add(CurrentTransaction, item.Id, Copy(item));
			}
		}

		public int Update(Job item)
		{
			lock (DataStore.Jobs.SyncRoot)
			{
				//the count of rows the write actually changed, as a database would report it
				if (!DataStore.Jobs.TryGet(item.Id, out _))
					return 0;

				DataStore.Jobs.Save(CurrentTransaction, item.Id, Copy(item));

				return 1;
			}
		}

		private static Job Copy(Job source)
		{
			return DataStore.Jobs.Copy(source);
		}
	}
}
