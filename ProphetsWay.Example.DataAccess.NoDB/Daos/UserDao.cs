﻿using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using System.Linq;

namespace ProphetsWay.Example.DataAccess.NoDB.Daos
{
	internal class UserDao : BaseDao, IUserDao
	{
		public void CustomUserFunctionality(User user)
		{
			//this example function is silly, but just used to illustrate some sort of custom query/function in your DAL
			lock (DataStore.Users)
				DataStore.Users[user.Id].Whatever = "custom functionality triggered";
		}

		public int Delete(User item)
		{
			lock (DataStore.Users)
				DataStore.Users.Remove(item.Id);

			return 1;
		}

		public User Get(User item)
		{
			lock (DataStore.Users)
				if (DataStore.Users.ContainsKey(item.Id))
					return DataStore.Users[item.Id];

			return null;
		}

		public void Insert(User item)
		{
			lock (DataStore.Users)
			{
				item.Id = Random.Next(int.MaxValue);

				DataStore.Users.Add(item.Id, item);
			}
		}

		public int Update(User item)
		{
			DataStore.Users[item.Id] = item;
			return 1;
		}
	}
}
