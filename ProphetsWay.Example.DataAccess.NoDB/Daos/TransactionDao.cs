using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="ITransactionDao"/>, and the deepest object graph in this
	/// project.
	/// </summary>
	/// <remarks>
	/// It copies on the way in and on the way out, as the snapshot rule on <see cref="IExampleDataAccess"/>
	/// requires of every Dao here - and a <see cref="Transaction"/> is two levels deep, so its copy has to reach
	/// through the user it names to the company, job and department that user names in turn. That work lives in
	/// <see cref="DataStore"/> alongside every other copy, rather than here.
	/// </remarks>
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
				return DataStore.Transactions.TryGet(item.Id, out var stored) ? Copy(stored) : null;
		}

		public int GetCount(Transaction item)
		{
			//a read takes the lock like every other read here: a rollback replaying its undo log is writing to
			//this table, and counting or paging an unlocked dictionary while it does throws
			lock (DataStore.Transactions.SyncRoot)
				return DataStore.Transactions.Count;
		}

		public IList<Transaction> GetPaged(Transaction item, int skip, int take)
		{
			lock (DataStore.Transactions.SyncRoot)
				return DataStore.Transactions.Rows.Skip(skip).Take(take).Select(Copy).ToList();
		}

		public void Insert(Transaction item)
		{
			lock (DataStore.Transactions.SyncRoot)
			{
				//the generated identifier is the one value that travels back onto the caller's instance; the
				//store gets a copy, so neither that instance nor the user and company it names are adopted
				item.Id = DataStore.NextTransactionId();
				DataStore.Transactions.Add(CurrentTransaction, item.Id, Copy(item));
			}
		}

		public int Update(Transaction item)
		{
			lock (DataStore.Transactions.SyncRoot)
			{
				//the count of rows the write actually changed, as a database would report it
				if (!DataStore.Transactions.TryGet(item.Id, out _))
					return 0;

				DataStore.Transactions.Save(CurrentTransaction, item.Id, Copy(item));

				return 1;
			}
		}

		private static Transaction Copy(Transaction source)
		{
			return DataStore.Transactions.Copy(source);
		}
	}
}
