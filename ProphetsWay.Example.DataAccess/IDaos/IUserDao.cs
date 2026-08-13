using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.DataAccess.IDaos
{
	/// <summary>
	/// The Data Access Layer contract for <see cref="User"/>, adding one custom member alongside the four CRUD
	/// operations.
	/// </summary>
	/// <remarks>
	/// The Data-Access-Layer-wide snapshot rule on <see cref="IExampleDataAccess"/> binds every member of this
	/// Data Access Object, <see cref="CustomUserFunctionality"/> included. An instance returned by <c>Get</c> is
	/// a snapshot; an instance handed to <c>Insert</c>, <c>Update</c>, <c>Delete</c> or
	/// <see cref="CustomUserFunctionality"/> is read rather than adopted; and stored data changes only through
	/// the write members, each reading its argument as it stands at the moment of the call. See
	/// <see cref="IExampleDataAccess"/> for the full statement and for why the rule exists.
	/// <para>
	/// <see cref="CustomUserFunctionality"/> states no behavior of its own, and none is implied here — what it
	/// does, and what if anything it writes back onto the caller's instance, is the implementation's to define.
	/// The snapshot rule binds it regardless.
	/// </para>
	/// </remarks>
	public interface IUserDao : IBaseDao<User>
	{
		void CustomUserFunctionality(User user);
	}
}
