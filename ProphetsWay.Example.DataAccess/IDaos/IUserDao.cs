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
	/// <para>
	/// The Data-Access-Layer-wide identifier and row count rules on <see cref="IExampleDataAccess"/> bind this
	/// Data Access Object as well. <c>Insert</c> assigns <see cref="Entities.BaseIntEntity.Id"/> onto the
	/// instance the caller passed in, so the identifier is read off that instance after the call and not before.
	/// <c>Update</c> and <c>Delete</c> return <c>1</c> when that identifier matched a stored user and <c>0</c>
	/// when it matched none — <c>1</c> from <c>Update</c> even where the incoming values are identical to the
	/// stored ones, because the count reports that a row matched rather than that a value changed. Neither rule
	/// says anything about <see cref="CustomUserFunctionality"/>, which remains the implementation's to define.
	/// </para>
	/// </remarks>
	public interface IUserDao : IBaseDao<User>
	{
		void CustomUserFunctionality(User user);
	}
}
