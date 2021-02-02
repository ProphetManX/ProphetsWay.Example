using System;
using System.Collections.Generic;
using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.IDaos;
using ProphetsWay.Example.DataAccess.NoDB.Daos;

namespace ProphetsWay.Example.DataAccess.NoDB
{
	/// <summary>
	/// This is the main entry point for the DAL implementation.  In this example, each of the individual DAOs 
	/// are created internally and each call is mapped to the internal DAO
	/// This class has hardly any functional/logical code within it
	/// 
	/// If you choose to do so, you can put all your actual code within this one file and not bother with each separate DAO
	/// but that is not recommended
	/// </summary>
	public class ExampleDataAccess : BaseDataAccess.BaseDataAccess, IExampleDataAccess
	{
		private readonly ICompanyDao _companyDao = new CompanyDao();
		private readonly IJobDao _jobDao = new JobDao();
		private readonly IUserDao _userDao = new UserDao();
		private readonly ITransactionDao _transactionDao = new TransactionDao();
		private readonly IResourceDao _resourceDao = new ResourceDao();

		public void CustomUserFunctionality(User user)
		{
			_userDao.CustomUserFunctionality(user);
		}

		public int Delete(Company item)
		{
			return _companyDao.Delete(item);
		}

		public int Delete(Job item)
		{
			return _jobDao.Delete(item);
		}

		public int Delete(User item)
		{
			return _userDao.Delete(item);
		}

		public int Delete(Transaction item)
		{
			return _transactionDao.Delete(item);
		}

		public int Delete(Resource item)
		{
			return _resourceDao.Delete(item);
		}

		public Company Get(Company item)
		{
			return _companyDao.Get(item);
		}

		public Job Get(Job item)
		{
			return _jobDao.Get(item);
		}

		public User Get(User item)
		{
			return _userDao.Get(item);
		}

		public Transaction Get(Transaction item)
		{
			return _transactionDao.Get(item);
		}

		public Resource Get(Resource item)
		{
			return _resourceDao.Get(item);
		}

		public IList<Job> GetAll(Job item)
		{
			return _jobDao.GetAll(item);
		}

		public IList<Resource> GetAll(Resource item)
		{
			return _resourceDao.GetAll(item);
		}

		public int GetCount(Company item)
		{
			return _companyDao.GetCount(item);
		}

		public int GetCount(Transaction item)
		{
			return _transactionDao.GetCount(item);
		}

		public Company GetCustomCompanyFunction(int id)
		{
			return _companyDao.GetCustomCompanyFunction(id);
		}

		public IList<Company> GetPaged(Company item, int skip, int take)
		{
			return _companyDao.GetPaged(item, skip, take);
		}

		public IList<Transaction> GetPaged(Transaction item, int skip, int take)
		{
			return _transactionDao.GetPaged(item, skip, take);
		}

		public void Insert(Company item)
		{
			_companyDao.Insert(item);
		}

		public void Insert(Job item)
		{
			_jobDao.Insert(item);
		}

		public void Insert(User item)
		{
			_userDao.Insert(item);
		}

		public void Insert(Transaction item)
		{
			_transactionDao.Insert(item);
		}

		public void Insert(Resource item)
		{
			_resourceDao.Insert(item);
		}

		public override void TransactionCommit()
		{
			//not implementing these for the example, but you could do something like "context.CommitTransaction"
			throw new NotImplementedException();
		}

		public override void TransactionRollBack()
		{
			//not implementing these for the example, but you could do something like "context.RollbackTransaction"
			throw new NotImplementedException();
		}

		public override void TransactionStart()
		{
			//not implementing these for the example, but you could do something like "context.BeginTransaction"
			throw new NotImplementedException();
		}

		public int Update(Company item)
		{
			return _companyDao.Update(item);
		}

		public int Update(Job item)
		{
			return _jobDao.Update(item);
		}

		public int Update(User item)
		{
			return _userDao.Update(item);
		}

		public int Update(Transaction item)
		{
			return _transactionDao.Update(item);
		}

		public int Update(Resource item)
		{
			return _resourceDao.Update(item);
		}
	}
}
