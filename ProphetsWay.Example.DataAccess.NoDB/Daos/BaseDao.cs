using System;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// What every Dao in this implementation needs: the identifier generator a real database would own, and the
	/// transaction of the Data Access Layer instance the Dao was built for.
	/// </summary>
	/// <remarks>
	/// <see cref="CurrentTransaction"/> is required by the constructor rather than settable afterwards, so a Dao
	/// cannot exist without one - and because every write goes through <see cref="StoreTable{TKey, TEntity}"/> or
	/// <see cref="StoreList{TEntity}"/>, which will not write without being handed it, every write this project
	/// makes is enrolled in whatever its owner has open.
	/// </remarks>
	internal abstract class BaseDao
	{
		protected static Random Random = new Random(DateTime.Now.Millisecond);

		protected BaseDao(TransactionLog currentTransaction)
		{
			CurrentTransaction = currentTransaction ?? throw new ArgumentNullException(nameof(currentTransaction));
		}

		/// <summary>
		/// The owning Data Access Layer instance's transaction log. It discards what it is handed whenever no
		/// transaction is open, which is what makes a write outside one commit on its own.
		/// </summary>
		protected TransactionLog CurrentTransaction { get; }
	}
}
