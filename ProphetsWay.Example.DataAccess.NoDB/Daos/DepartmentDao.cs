using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="IDepartmentDao"/>, and the one Dao in this project that
	/// copies on the way in and on the way out.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The other Daos here hand back the very object they are holding, which is the shortest thing that works
	/// for an in-memory store and is exactly what a database cannot do. Rule 19 of
	/// <see cref="IDepartmentDao"/> forbids it, so every value crossing this boundary is copied:
	/// <c>Get</c>, <c>GetAll</c> and <c>GetPaged</c> hand out snapshots, and <c>Insert</c>, <c>Update</c>,
	/// <c>Delete</c> and <c>Restore</c> read their argument rather than adopting it. The instances inside
	/// <see cref="DataStore.Departments"/> are therefore reachable from nowhere else, and can be edited in
	/// place with no way for a caller to see or influence them.
	/// </para>
	/// </remarks>
	internal class DepartmentDao : BaseDao, IDepartmentDao
	{
		/// <inheritdoc />
		public void Insert(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			var stamp = DateTime.UtcNow;

			lock (DataStore.Departments)
			{
				//rule 19 - the store gets a copy, so the caller's instance is read and not adopted
				var stored = Copy(item);
				stored.Id = DataStore.NextDepartmentId();
				stored.CreatedDate = stamp;
				stored.UpdatedDate = null;
				stored.DeletedDate = null;

				DataStore.Departments.Add(stored.Id, stored);

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

			lock (DataStore.Departments)
			{
				//rule 8 - a soft-deleted department is found here just like a live one
				return DataStore.Departments.TryGetValue(item.Id, out var stored) ? Copy(stored) : null;
			}
		}

		/// <inheritdoc />
		public int Update(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			var stamp = DateTime.UtcNow;

			lock (DataStore.Departments)
			{
				if (!DataStore.Departments.TryGetValue(item.Id, out var stored))
					return 0;

				//rule 3 - the department's own data, and nothing else. CreatedDate and DeletedDate are left as
				//the store holds them, so an Update can neither rewrite history nor soft-delete a department
				//behind Delete's back.
				stored.Name = item.Name;
				stored.Description = item.Description;

				//rule 2 - Update owns UpdatedDate
				stored.UpdatedDate = stamp;
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

			lock (DataStore.Departments)
			{
				if (!DataStore.Departments.TryGetValue(item.Id, out var stored))
					return 0;

				//rule 6 - already deleted, so the existing stamp stands and still reports when it happened
				if (stored.DeletedDate.HasValue)
					return 0;

				//rule 5 - a soft delete: the record stays, only the stamp is written
				stored.DeletedDate = stamp;
				item.DeletedDate = stamp;

				return 1;
			}
		}

		/// <inheritdoc />
		public int Restore(Department item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			lock (DataStore.Departments)
			{
				if (!DataStore.Departments.TryGetValue(item.Id, out var stored))
					return 0;

				if (!stored.DeletedDate.HasValue)
					return 0;

				//a lifecycle change, not a modification - UpdatedDate is deliberately left alone
				stored.DeletedDate = null;
				item.DeletedDate = null;

				return 1;
			}
		}

		/// <inheritdoc />
		public IList<Department> GetAll(Department item)
		{
			//rule 13 - item is a type selector and is null when the call arrives through the dispatcher
			lock (DataStore.Departments)
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

			lock (DataStore.Departments)
				return Live().Skip(skip).Take(take).Select(Copy).ToList();
		}

		/// <inheritdoc />
		public int GetCount(Department item)
		{
			lock (DataStore.Departments)
				return Live().Count();
		}

		/// <summary>
		/// The live set, in the one order rule 11 needs it to keep. Call only while holding the store's lock.
		/// </summary>
		private static IEnumerable<Department> Live()
		{
			return DataStore.Departments.Values.Where(x => !x.DeletedDate.HasValue);
		}

		/// <summary>
		/// A field-for-field copy. <see cref="DateTime"/> is a value type, so <see cref="DateTime.Kind"/>
		/// crosses with the value and a stamp is still Coordinated Universal Time on the far side - rule 18.
		/// </summary>
		private static Department Copy(Department source)
		{
			return new Department
			{
				Id = source.Id,
				Name = source.Name,
				Description = source.Description,
				CreatedDate = source.CreatedDate,
				UpdatedDate = source.UpdatedDate,
				DeletedDate = source.DeletedDate
			};
		}
	}
}
