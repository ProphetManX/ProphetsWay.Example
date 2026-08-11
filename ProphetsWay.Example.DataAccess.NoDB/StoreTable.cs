using System;
using System.Collections.Generic;

namespace ProphetsWay.Example.DataAccess.NoDB
{
	/// <summary>
	/// One keyed table of the in-memory store, and the only way to change a row in it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Reads go straight through. <b>Every write takes a <see cref="TransactionLog"/></b>, and that is the whole
	/// reason this type exists: a Dao cannot change a stored row without handing over the log belonging to the
	/// Data Access Layer instance it was built for, so there is no way to write and forget to enrol the write in
	/// whatever transaction that instance has open. Before the table existed the writes were inline dictionary
	/// assignments scattered across seven Daos, and enrolling them all would have been a matter of remembering to.
	/// </para>
	/// <para>
	/// An undo entry always holds a <b>copy</b> of the row it puts back. Several Daos here store the very instance
	/// the caller passed in and hand that same instance back from <c>Get</c>; an undo entry holding a reference to
	/// one of those would restore an object the caller is still free to edit, and the rollback would then restore
	/// whatever the caller had since done to it rather than what the store actually held.
	/// </para>
	/// </remarks>
	/// <typeparam name="TKey">The identifier the rows are stored under.</typeparam>
	/// <typeparam name="TEntity">The entity type the table holds.</typeparam>
	internal sealed class StoreTable<TKey, TEntity> where TEntity : class
	{
		private readonly Dictionary<TKey, TEntity> _rows = new Dictionary<TKey, TEntity>();

		private readonly Func<TEntity, TEntity> _copy;

		/// <param name="copy">
		/// A field-for-field copy of a row. Used for every undo entry, and offered to the Daos that hand out
		/// snapshots rather than the stored instance.
		/// </param>
		public StoreTable(Func<TEntity, TEntity> copy)
		{
			_copy = copy ?? throw new ArgumentNullException(nameof(copy));
		}

		/// <summary>
		/// What to lock on when a read and the write that depends on it have to happen together.
		/// </summary>
		public object SyncRoot => _rows;

		/// <summary>
		/// The number of rows stored, including any a soft delete has stamped.
		/// </summary>
		public int Count => _rows.Count;

		/// <summary>
		/// Every stored row, in insertion order.
		/// </summary>
		public IEnumerable<TEntity> Rows => _rows.Values;

		/// <summary>
		/// The row stored under <paramref name="key"/>, if there is one.
		/// </summary>
		public bool TryGet(TKey key, out TEntity row)
		{
			return _rows.TryGetValue(key, out row);
		}

		/// <summary>
		/// A field-for-field copy of a row, for a Dao that hands out snapshots rather than the stored instance.
		/// </summary>
		public TEntity Copy(TEntity row)
		{
			return _copy(row);
		}

		/// <summary>
		/// Stores a row under a key nothing is stored under, and records that undoing it means removing it.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown when a row is already stored under that key, as a primary key would.</exception>
		public void Add(TransactionLog log, TKey key, TEntity row)
		{
			_rows.Add(key, row);
			log.Record(() => { lock (_rows) _rows.Remove(key); });
		}

		/// <summary>
		/// Replaces the row stored under a key - adding it if none is - and records how to put back what was there.
		/// </summary>
		public void Save(TransactionLog log, TKey key, TEntity row)
		{
			RecordUndo(log, key);
			_rows[key] = row;
		}

		/// <summary>
		/// Removes the row stored under a key and records how to put it back.
		/// </summary>
		/// <returns><c>true</c> when a row was removed, <c>false</c> when there was none to remove.</returns>
		public bool Remove(TransactionLog log, TKey key)
		{
			if (!_rows.ContainsKey(key))
				return false;

			RecordUndo(log, key);
			return _rows.Remove(key);
		}

		/// <summary>
		/// Edits the stored instance where it lies, rather than replacing it, and records how to put it back.
		/// </summary>
		/// <remarks>
		/// For the Dao whose caller may be holding that same instance and is expected to see the change. Replacing
		/// the row would silently break that, so the edit stays in place and only the undo entry gets a copy.
		/// </remarks>
		/// <returns><c>true</c> when a row was edited, <c>false</c> when there was none under that key.</returns>
		public bool EditInPlace(TransactionLog log, TKey key, Action<TEntity> edit)
		{
			if (!_rows.TryGetValue(key, out var row))
				return false;

			RecordUndo(log, key);
			edit(row);

			return true;
		}

		private void RecordUndo(TransactionLog log, TKey key)
		{
			if (_rows.TryGetValue(key, out var current))
			{
				var priorState = _copy(current);
				log.Record(() => { lock (_rows) _rows[key] = priorState; });
			}
			else
			{
				log.Record(() => { lock (_rows) _rows.Remove(key); });
			}
		}
	}
}
