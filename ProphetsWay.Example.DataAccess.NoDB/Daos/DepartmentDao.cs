using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="IDepartmentDao"/>, and the Dao the snapshot rule was written
	/// against before it was extended to the rest of them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Rule 19 of <see cref="IDepartmentDao"/> is that rule stated per-Dao, so every value crossing this boundary
	/// is copied: <c>Get</c>, <c>GetAll</c> and <c>GetPaged</c> hand out snapshots, and <c>Insert</c>,
	/// <c>Update</c>, <c>Delete</c> and <c>Restore</c> read their argument rather than adopting it. The instances
	/// inside <see cref="DataStore.Departments"/> are therefore reachable from nowhere else, and no caller can
	/// see or influence them. A <see cref="Department"/> carries scalars only, so its copy has no second level to
	/// reach.
	/// </para>
	/// </remarks>
	internal class DepartmentDao : BaseDao, IDepartmentDao
	{
		public DepartmentDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		/// <inheritdoc />
		public void Insert(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			var stamp = DateTime.UtcNow;

			lock (DataStore.Departments.SyncRoot)
			{
				//rule 19 - the store gets a copy, so the caller's instance is read and not adopted
				var stored = Copy(item);
				stored.Id = DataStore.NextDepartmentId();
				stored.CreatedDate = stamp;
				stored.UpdatedDate = null;
				stored.DeletedDate = null;

				DataStore.Departments.Add(CurrentTransaction, stored.Id, stored);

				//rule 1 - and the generated id and the stamps travel back onto the caller's instance
				item.Id = stored.Id;
				item.CreatedDate = stored.CreatedDate;
				item.UpdatedDate = null;
				item.DeletedDate = null;
			}
		}

		/// <inheritdoc />
		public Department Get(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			lock (DataStore.Departments.SyncRoot)
			{
				//rule 8 - a soft-deleted department is found here just like a live one
				return DataStore.Departments.TryGet(item.Id, out var stored) ? Copy(stored) : null;
			}
		}

		/// <inheritdoc />
		public int Update(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			var stamp = DateTime.UtcNow;

			lock (DataStore.Departments.SyncRoot)
			{
				if (!DataStore.Departments.TryGet(item.Id, out var stored))
					return 0;

				//rule 3 - the department's own data, and nothing else. CreatedDate and DeletedDate are left as
				//the store holds them, so an Update can neither rewrite history nor soft-delete a department
				//behind Delete's back.
				var edited = Copy(stored);
				edited.Name = item.Name;
				edited.Description = item.Description;

				//rule 2 - Update owns UpdatedDate
				edited.UpdatedDate = stamp;

				Save(edited);
				item.UpdatedDate = stamp;

				return 1;
			}
		}

		/// <inheritdoc />
		public int Delete(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			var stamp = DateTime.UtcNow;

			lock (DataStore.Departments.SyncRoot)
			{
				if (!DataStore.Departments.TryGet(item.Id, out var stored))
					return 0;

				//rule 6 - already deleted, so the existing stamp stands and still reports when it happened
				if (stored.DeletedDate.HasValue)
					return 0;

				//rule 5 - a soft delete: the record stays, only the stamp is written
				var deleted = Copy(stored);
				deleted.DeletedDate = stamp;

				Save(deleted);
				item.DeletedDate = stamp;

				return 1;
			}
		}

		/// <inheritdoc />
		public int Restore(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			lock (DataStore.Departments.SyncRoot)
			{
				if (!DataStore.Departments.TryGet(item.Id, out var stored))
					return 0;

				if (!stored.DeletedDate.HasValue)
					return 0;

				//a lifecycle change, not a modification - UpdatedDate is deliberately left alone
				var restored = Copy(stored);
				restored.DeletedDate = null;

				Save(restored);
				item.DeletedDate = null;

				return 1;
			}
		}

		/// <inheritdoc />
		public IList<Department> GetAll(Department item)
		{
			//rule 13 - item is a type selector and is null when the call arrives through the dispatcher
			lock (DataStore.Departments.SyncRoot)
				return Live().Select(Copy).ToList();
		}

		/// <inheritdoc />
		public IList<Department> GetPaged(Department item, int skip, int take)
		{
			//rule 12 - the bounds are rejected before any data is read
			if (skip < 0)
				throw new ArgumentOutOfRangeException(nameof(skip), skip, "A page cannot skip a negative number of departments.");

			if (take < 0)
				throw new ArgumentOutOfRangeException(nameof(take), take, "A page cannot take a negative number of departments.");

			lock (DataStore.Departments.SyncRoot)
				return Live().Skip(skip).Take(take).Select(Copy).ToList();
		}

		/// <inheritdoc />
		public int GetCount(Department item)
		{
			lock (DataStore.Departments.SyncRoot)
				return Live().Count();
		}

		/// <summary>
		/// The live set, in the one order rule 11 needs it to keep. Call only while holding the store's lock.
		/// </summary>
		private static IEnumerable<Department> Live()
		{
			return DataStore.Departments.Rows.Where(x => !x.DeletedDate.HasValue);
		}

		/// <summary>
		/// Replaces the stored department, enrolling the write in whatever transaction the owning Data Access Layer
		/// has open.
		/// </summary>
		/// <remarks>
		/// The stamped operations above build an edited copy and put it here rather than reaching into the stored
		/// instance and changing it, so that the store still holds what it held until the moment of the write and
		/// the transaction log has something to capture. Call only while holding the store's lock.
		/// </remarks>
		private void Save(Department department)
		{
			DataStore.Departments.Save(CurrentTransaction, department.Id, department);
		}

		/// <summary>
		/// A field-for-field copy. <see cref="DateTime"/> is a value type, so <see cref="DateTime.Kind"/>
		/// crosses with the value and a stamp is still Coordinated Universal Time on the far side - rule 18.
		/// </summary>
		private static Department Copy(Department source)
		{
			return DataStore.Departments.Copy(source);
		}
	}
}
