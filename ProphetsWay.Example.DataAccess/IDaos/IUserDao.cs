using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.DataAccess.IDaos
{
	public interface IUserDao : IBaseDao<User>
	{
		void CustomUserFunctionality(User user);
	}
}
