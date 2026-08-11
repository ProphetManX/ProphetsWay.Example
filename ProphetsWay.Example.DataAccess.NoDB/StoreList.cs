using System;
using System.Collections.Generic;

namespace ProphetsWay.Example.DataAccess.NoDB
{
	/// <summary>
	/// The unkeyed counterpart of <see cref="StoreTable{TKey, TEntity}"/>, for the one entity in this example
	/// that has no identifier to be stored under.
	/// </summary>
	/// <remarks>
	/// It exists for the same single reason: <b>every write takes a <see cref="TransactionLog"/></b>, so a row
	/// cannot be added or removed without being enrolled in whatever transaction the owning Data Access Layer
	/// instance has open. Its undo entries hold the row itself rather than a copy, because the only Dao using it
	/// copies on the way in - the instance in the list is already reachable from nowhere else.
	/// </remarks>
	/// <typeparam name="TEntity">The entity type the list holds.</typeparam>
	internal sealed class StoreList<TEntity> where TEntity : class
	{
		private readonly List<TEntity> _rows = new List<TEntity>();

		private readonly Func<TEntity, TEntity> _copy;

		/// <param name="copy">A field-for-field copy of a row, for the Dao that hands out snapshots.</param>
		public StoreList(Func<TEntity, TEntity> copy)
		{
			_copy = copy ?? throw new ArgumentNullException(nameof(copy));
		}

		/// <summary>
		/// What to lock on when a read and the write that depends on it have to happen together.
		/// </summary>
		public object SyncRoot => _rows;

		/// <summary>
		/// Every stored row, in insertion order.
		/// </summary>
		public IEnumerable<TEntity> Rows => _rows;

		/// <summary>
		/// A field-for-field copy of a row, for a Dao that hands out snapshots rather than the stored instance.
		/// </summary>
		public TEntity Copy(TEntity row)
		{
			return _copy(row);
		}

		/// <summary>
		/// Appends a row and records that undoing it means taking it back out.
		/// </summary>
		public void Add(TransactionLog log, TEntity row)
		{
			_rows.Add(row);
			log.Record(() => { lock (_rows) _rows.Remove(row); });
		}

		/// <summary>
		/// Removes a stored row and records how to put it back where it was.
		/// </summary>
		/// <returns><c>true</c> when a row was removed, <c>false</c> when it was not in the list.</returns>
		public bool Remove(TransactionLog log, TEntity row)
		{
			var index = _rows.IndexOf(row);

			if (index < 0)
				return false;

			_rows.RemoveAt(index);

			//later rows may have been removed by the time this is replayed, so the index is clamped rather than trusted
			log.Record(() => { lock (_rows) _rows.Insert(Math.Min(index, _rows.Count), row); });

			return true;
		}
	}
}
