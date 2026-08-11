using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	/// <summary>
	/// The in-memory implementation of <see cref="IUserDao"/>.
	/// </summary>
	/// <remarks>
	/// It copies on the way in and on the way out, as the snapshot rule on <see cref="IExampleDataAccess"/>
	/// requires of every Dao here - and a <see cref="User"/> names a company, a job and a department, so its copy
	/// reaches one level down and copies those too. That work lives in <see cref="DataStore"/> alongside every
	/// other copy, rather than here.
	/// </remarks>
	internal class UserDao : BaseDao, IUserDao
	{
		//the value the custom member writes, named once so the store and the caller cannot be given different ones
		private const string CustomFunctionalityStamp = "custom functionality triggered";

		public UserDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		/// <summary>
		/// A stand-in for whatever query or command a real Data Access Object would add beyond the surface it
		/// inherits: it stamps <see cref="User.Whatever"/> on the stored user, and on the caller's instance.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The write-back is what reconciles this member with the snapshot rule. Before that rule was extended
		/// to every Dao here, <c>Insert</c> stored the caller's instance and this method edited that instance
		/// where it lay, so a caller still holding it simply saw the change; now the store holds a copy and the
		/// caller cannot be reached by editing it. <see cref="IUserDao"/> leaves what this member writes back to
		/// the implementation, and the rule allows a stated write-back, so the value is assigned to
		/// <paramref name="user"/> as well - deliberately, and only that value.
		/// </para>
		/// <para>
		/// The stored row is still edited in place rather than replaced, which is why
		/// <see cref="StoreTable{TKey, TEntity}.EditInPlace"/> exists. Replacing it would read the same from
		/// outside, so the difference that remains is one of intent: this is a targeted edit to one field of a
		/// stored row, not a caller's version of the row overwriting it, and the undo entry still captures a deep
		/// copy of what was there beforehand.
		/// </para>
		/// </remarks>
		public void CustomUserFunctionality(User user)
		{
			lock (DataStore.Users.SyncRoot)
			{
				var edited = DataStore.Users.EditInPlace(CurrentTransaction, user.Id, stored => stored.Whatever = CustomFunctionalityStamp);

				if (edited)
					user.Whatever = CustomFunctionalityStamp;
			}
		}

		public int Delete(User item)
		{
			lock (DataStore.Users.SyncRoot)
				return DataStore.Users.Remove(CurrentTransaction, item.Id) ? 1 : 0;
		}

		public User Get(User item)
		{
			lock (DataStore.Users.SyncRoot)
				return DataStore.Users.TryGet(item.Id, out var stored) ? Copy(stored) : null;
		}

		public void Insert(User item)
		{
			lock (DataStore.Users.SyncRoot)
			{
				//the generated identifier is the one value that travels back onto the caller's instance; the
				//store gets a copy, so neither that instance nor the entities it names are adopted
				item.Id = DataStore.NextUserId();
				DataStore.Users.Add(CurrentTransaction, item.Id, Copy(item));
			}
		}

		public int Update(User item)
		{
			lock (DataStore.Users.SyncRoot)
			{
				//the count of rows the write actually changed, as a database would report it
				if (!DataStore.Users.TryGet(item.Id, out _))
					return 0;

				DataStore.Users.Save(CurrentTransaction, item.Id, Copy(item));

				return 1;
			}
		}

		private static User Copy(User source)
		{
			return DataStore.Users.Copy(source);
		}
	}
}
