using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class UserDao : BaseDao, IUserDao
	{
		public UserDao(TransactionLog currentTransaction) : base(currentTransaction)
		{
		}

		public void CustomUserFunctionality(User user)
		{
			//this example function is silly, but just used to illustrate some sort of custom query/function in your DAL
			lock (DataStore.Users.SyncRoot)
				DataStore.Users.EditInPlace(CurrentTransaction, user.Id, stored => stored.Whatever = "custom functionality triggered");
		}

		public int Delete(User item)
		{
			lock (DataStore.Users.SyncRoot)
				DataStore.Users.Remove(CurrentTransaction, item.Id);

			return 1;
		}

		public User Get(User item)
		{
			lock (DataStore.Users.SyncRoot)
				if (DataStore.Users.TryGet(item.Id, out var stored))
					return stored;

			return null;
		}

		public void Insert(User item)
		{
			lock (DataStore.Users.SyncRoot)
			{
				item.Id = Random.Next(int.MaxValue);

				DataStore.Users.Add(CurrentTransaction, item.Id, item);
			}
		}

		public int Update(User item)
		{
			lock (DataStore.Users.SyncRoot)
				DataStore.Users.Save(CurrentTransaction, item.Id, item);

			return 1;
		}
	}
}
