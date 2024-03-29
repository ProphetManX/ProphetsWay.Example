﻿using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.DataAccess
{
	public interface IExampleDataAccess : IBaseDataAccess, ICompanyDao, IJobDao, IUserDao, ITransactionDao, IResourceDao
	{
		//this is just your interface of all interfaces
		//so long as any DAL implementation uses this interface as it's main input and you're defined entities in this project
		//then you can easily decouple your current DAL and switch to a new one once it's written

		//any unit tests you write for your implementation should be easily modified to target the new DAL
	}
}
