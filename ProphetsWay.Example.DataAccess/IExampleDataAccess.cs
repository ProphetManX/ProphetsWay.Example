using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.DataAccess
{
	/// <summary>
	/// The single Data Access Layer contract a consumer injects — the interface of all interfaces, aggregating
	/// every DAO in this example alongside <see cref="IBaseDataAccess"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// So long as a DAL implementation takes this interface as its main input and the entities stay defined in
	/// this project, the current DAL can be decoupled and swapped for a newly written one. Any unit tests
	/// written against one implementation should need little or no modification to target the next — that
	/// interchangeability is the whole argument this repository makes.
	/// </para>
	/// <para>
	/// Two members are here to show the edges of the paradigm: <see cref="IDepartmentDao"/> showcases
	/// soft-delete and a custom method, and <see cref="ICompanyResourceDao"/> showcases an entity with no
	/// identifier and a DAO that inherits <see cref="IBaseDao{T}"/> not at all.
	/// </para>
	/// </remarks>
	public interface IExampleDataAccess : IBaseDataAccess, ICompanyDao, IJobDao, IUserDao, ITransactionDao, IResourceDao, IDepartmentDao, ICompanyResourceDao
	{
	}
}
