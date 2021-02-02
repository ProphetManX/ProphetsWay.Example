using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class TransactionDao : ITransactionDao
	{
		public int Delete(Transaction item)
		{
			lock (DataStore.Transactions)
				return DataStore.Transactions.Remove(item.Id) ? 1 : 0;
		}

		public Transaction Get(Transaction item)
		{
			lock (DataStore.Transactions)
				if (DataStore.Transactions.ContainsKey(item.Id))
					return DataStore.Transactions[item.Id];

			return null;
		}

		public int GetCount(Transaction item)
		{
			return DataStore.Transactions.Count;
		}

		public IList<Transaction> GetPaged(Transaction item, int skip, int take)
		{
			return DataStore.Transactions.Values.Skip(skip).Take(take).ToList();
		}

		public void Insert(Transaction item)
		{
			lock (DataStore.Transactions)
			{
				item.Id = DataStore.Transactions.Keys.Count > 0
					? DataStore.Transactions.Keys.Max() + 1
					: 1;

				DataStore.Transactions.Add(item.Id, item);
			}
		}

		public int Update(Transaction item)
		{
			DataStore.Transactions[item.Id] = item;
			return 1;
		}
	}
}
