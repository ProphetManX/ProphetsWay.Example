using System;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// What every Dao in this implementation needs: the transaction of the Data Access Layer instance the Dao
	/// was built for.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="CurrentTransaction"/> is required by the constructor rather than settable afterwards, so a Dao
	/// cannot exist without one - and because every write goes through <see cref="StoreTable{TKey, TEntity}"/> or
	/// <see cref="StoreList{TEntity}"/>, which will not write without being handed it, every write this project
	/// makes is enrolled in whatever its owner has open.
	/// </para>
	/// <para>
	/// Identifier generation is not here. It used to be, as one shared <see cref="Random"/> that four Daos drew
	/// from while each held its own table's lock - four locks serialising nothing, and <see cref="Random"/> is
	/// not thread safe, so concurrent draws could corrupt its state into returning the same number forever.
	/// <see cref="DataStore"/> owns the counters now and increments them atomically, which is also the truer
	/// picture: an identity column is sequential, and the database owns it rather than the Dao.
	/// </para>
	/// </remarks>
	internal abstract class BaseDao
	{
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
