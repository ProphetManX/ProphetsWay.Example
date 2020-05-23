using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.DataAccess.IDaos
{
	public interface ICompanyDao : IBasePagedDao<Company>
	{
		Company GetCustomCompanyFunction(int id);
	}
}
