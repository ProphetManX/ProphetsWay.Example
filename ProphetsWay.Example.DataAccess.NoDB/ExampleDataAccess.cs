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
		/// <summary>
		/// This instance's transaction, and nothing else's.
		/// </summary>
		/// <remarks>
		/// Handed to every Dao below at construction, which is what enrols their writes. It lives on the instance
		/// rather than on the process-wide <see cref="DataStore"/> deliberately: <c>IBaseDataAccess</c> scopes a
		/// transaction to the Data Access Layer instance, so another instance writing to the same store at the
		/// same time is doing work this transaction has no business rolling back.
		/// </remarks>
		private readonly TransactionLog _transaction = new TransactionLog();

		private readonly ICompanyDao _companyDao;
		private readonly IJobDao _jobDao;
		private readonly IUserDao _userDao;
		private readonly ITransactionDao _transactionDao;
		private readonly IResourceDao _resourceDao;
		private readonly IDepartmentDao _departmentDao;
		private readonly ICompanyResourceDao _companyResourceDao;

		private bool _disposed;

		public ExampleDataAccess()
		{
			//every Dao is built around this instance's transaction, so there is no way for one of them to write
			//outside it
			_companyDao = new CompanyDao(_transaction);
			_jobDao = new JobDao(_transaction);
			_userDao = new UserDao(_transaction);
			_transactionDao = new TransactionDao(_transaction);
			_resourceDao = new ResourceDao(_transaction);
			_departmentDao = new DepartmentDao(_transaction);
			_companyResourceDao = new CompanyResourceDao(_transaction);
		}

		/// <summary>
		/// Releases what this instance holds, which here is an open transaction and nothing else.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The Daos above are stateless and <see cref="DataStore"/> is process-wide - it stands in for the
		/// database, not for a connection to it. So the only thing there is to release is a transaction the caller
		/// left open, and <b>an unclosed transaction is an abandoned one</b>: it is rolled back, never committed,
		/// and the rollback is not allowed to throw on the way out.
		/// </para>
		/// <para>
		/// <b>Clearing the store here would be the mistake.</b> Disposing one Data Access Layer no more empties
		/// the database than closing one connection does, and because every test in this repository disposes an
		/// instance while other tests are still running, doing it would delete rows out from under them
		/// intermittently.
		/// </para>
		/// <para>
		/// A real implementation disposes what it created - a <c>DbContext</c>, a connection, an open
		/// transaction - and leaves anything handed to it by the caller alone.
		/// </para>
		/// </remarks>
		public override void Dispose()
		{
			//idempotent: a second call is a no-op rather than an error
			if (_disposed)
				return;

			_disposed = true;
			_transaction.Abandon();
		}

		/// <summary>
		/// Every member other than <see cref="Dispose"/> refuses to run once the instance has been disposed.
		/// </summary>
		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(ExampleDataAccess));
		}

		public void CustomUserFunctionality(User user)
		{
			ThrowIfDisposed();
			_userDao.CustomUserFunctionality(user);
		}

		public int Delete(Company item)
		{
			ThrowIfDisposed();
			return _companyDao.Delete(item);
		}

		public int Delete(Job item)
		{
			ThrowIfDisposed();
			return _jobDao.Delete(item);
		}

		public int Delete(User item)
		{
			ThrowIfDisposed();
			return _userDao.Delete(item);
		}

		public int Delete(Transaction item)
		{
			ThrowIfDisposed();
			return _transactionDao.Delete(item);
		}

		public int Delete(Resource item)
		{
			ThrowIfDisposed();
			return _resourceDao.Delete(item);
		}

		public int Delete(Department item)
		{
			ThrowIfDisposed();
			return _departmentDao.Delete(item);
		}

		public int Delete(CompanyResource item)
		{
			ThrowIfDisposed();
			return _companyResourceDao.Delete(item);
		}

		public Company Get(Company item)
		{
			ThrowIfDisposed();
			return _companyDao.Get(item);
		}

		public Job Get(Job item)
		{
			ThrowIfDisposed();
			return _jobDao.Get(item);
		}

		public User Get(User item)
		{
			ThrowIfDisposed();
			return _userDao.Get(item);
		}

		public Transaction Get(Transaction item)
		{
			ThrowIfDisposed();
			return _transactionDao.Get(item);
		}

		public Resource Get(Resource item)
		{
			ThrowIfDisposed();
			return _resourceDao.Get(item);
		}

		public Department Get(Department item)
		{
			ThrowIfDisposed();
			return _departmentDao.Get(item);
		}

		public IList<Job> GetAll(Job item)
		{
			ThrowIfDisposed();
			return _jobDao.GetAll(item);
		}

		public IList<Resource> GetAll(Resource item)
		{
			ThrowIfDisposed();
			return _resourceDao.GetAll(item);
		}

		public IList<Department> GetAll(Department item)
		{
			ThrowIfDisposed();
			return _departmentDao.GetAll(item);
		}

		public IList<CompanyResource> GetAll(CompanyResource item)
		{
			ThrowIfDisposed();
			return _companyResourceDao.GetAll(item);
		}

		public int GetCount(Company item)
		{
			ThrowIfDisposed();
			return _companyDao.GetCount(item);
		}

		public int GetCount(Transaction item)
		{
			ThrowIfDisposed();
			return _transactionDao.GetCount(item);
		}

		public int GetCount(Department item)
		{
			ThrowIfDisposed();
			return _departmentDao.GetCount(item);
		}

		public Company GetCustomCompanyFunction(int id)
		{
			ThrowIfDisposed();
			return _companyDao.GetCustomCompanyFunction(id);
		}

		public IList<Company> GetPaged(Company item, int skip, int take)
		{
			ThrowIfDisposed();
			return _companyDao.GetPaged(item, skip, take);
		}

		public IList<Transaction> GetPaged(Transaction item, int skip, int take)
		{
			ThrowIfDisposed();
			return _transactionDao.GetPaged(item, skip, take);
		}

		public IList<Department> GetPaged(Department item, int skip, int take)
		{
			ThrowIfDisposed();
			return _departmentDao.GetPaged(item, skip, take);
		}

		public void Insert(Company item)
		{
			ThrowIfDisposed();
			_companyDao.Insert(item);
		}

		public void Insert(Job item)
		{
			ThrowIfDisposed();
			_jobDao.Insert(item);
		}

		public void Insert(User item)
		{
			ThrowIfDisposed();
			_userDao.Insert(item);
		}

		public void Insert(Transaction item)
		{
			ThrowIfDisposed();
			_transactionDao.Insert(item);
		}

		public void Insert(Resource item)
		{
			ThrowIfDisposed();
			_resourceDao.Insert(item);
		}

		public void Insert(Department item)
		{
			ThrowIfDisposed();
			_departmentDao.Insert(item);
		}

		public void Insert(CompanyResource item)
		{
			ThrowIfDisposed();
			_companyResourceDao.Insert(item);
		}

		/// <summary>
		/// A custom member of <see cref="IDepartmentDao"/>, reached by an ordinary interface call rather than by
		/// the generic dispatcher - which is the point it makes.
		/// </summary>
		public int Restore(Department item)
		{
			ThrowIfDisposed();
			return _departmentDao.Restore(item);
		}

		/// <summary>
		/// Commits everything written through this instance since <see cref="TransactionStart"/>, which for an
		/// undo log means discarding it - the writes are already in the store.
		/// </summary>
		public override void TransactionCommit()
		{
			ThrowIfDisposed();
			_transaction.Commit();
		}

		/// <summary>
		/// Discards everything written through this instance since <see cref="TransactionStart"/> by replaying the
		/// undo log in reverse.
		/// </summary>
		public override void TransactionRollBack()
		{
			ThrowIfDisposed();
			_transaction.RollBack();
		}

		/// <summary>
		/// Opens a transaction over every write made through this instance, whichever Dao makes it.
		/// </summary>
		public override void TransactionStart()
		{
			ThrowIfDisposed();
			_transaction.Start();
		}

		public int Update(Company item)
		{
			ThrowIfDisposed();
			return _companyDao.Update(item);
		}

		public int Update(Job item)
		{
			ThrowIfDisposed();
			return _jobDao.Update(item);
		}

		public int Update(User item)
		{
			ThrowIfDisposed();
			return _userDao.Update(item);
		}

		public int Update(Transaction item)
		{
			ThrowIfDisposed();
			return _transactionDao.Update(item);
		}

		public int Update(Resource item)
		{
			ThrowIfDisposed();
			return _resourceDao.Update(item);
		}

		public int Update(Department item)
		{
			ThrowIfDisposed();
			return _departmentDao.Update(item);
		}
	}
}
