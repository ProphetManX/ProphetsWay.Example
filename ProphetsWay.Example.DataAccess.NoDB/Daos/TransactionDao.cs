using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class TransactionDao : BaseDao, ITransactionDao
	{
		public TransactionDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		public int Delete(Transaction item)
		{
			lock (DataStore.Transactions.SyncRoot)
				return DataStore.Transactions.Remove(CurrentTransaction, item.Id) ? 1 : 0;
		}

		public Transaction Get(Transaction item)
		{
			lock (DataStore.Transactions.SyncRoot)
				if (DataStore.Transactions.TryGet(item.Id, out var stored))
					return stored;

			return null;
		}

		public int GetCount(Transaction item)
		{
			return DataStore.Transactions.Count;
		}

		public IList<Transaction> GetPaged(Transaction item, int skip, int take)
		{
			return DataStore.Transactions.Rows.Skip(skip).Take(take).ToList();
		}

		public void Insert(Transaction item)
		{
			lock (DataStore.Transactions.SyncRoot)
			{
				item.Id = Random.Next(int.MaxValue);

				DataStore.Transactions.Add(CurrentTransaction, item.Id, item);
			}
		}

		public int Update(Transaction item)
		{
			lock (DataStore.Transactions.SyncRoot)
				DataStore.Transactions.Save(CurrentTransaction, item.Id, item);

			return 1;
		}
	}
}
