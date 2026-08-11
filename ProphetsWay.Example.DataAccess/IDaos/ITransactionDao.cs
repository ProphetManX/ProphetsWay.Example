using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.DataAccess.IDaos
{
	/// <summary>
	/// The Data Access Layer contract for <see cref="Transaction"/>.
	/// </summary>
	/// <remarks>
	/// The Data-Access-Layer-wide snapshot rule on <see cref="IExampleDataAccess"/> binds every member of this
	/// Data Access Object. An instance returned by <c>Get</c>, <c>GetAll</c> or <c>GetPaged</c> is a snapshot; an
	/// instance handed to <c>Insert</c>, <c>Update</c> or <c>Delete</c> is read rather than adopted; and stored
	/// data changes only through those write members, each reading its argument as it stands at the moment of the
	/// call. See <see cref="IExampleDataAccess"/> for the full statement and for why the rule exists.
	/// </remarks>
	public interface ITransactionDao : IBasePagedDao<Transaction>
	{

	}
}
