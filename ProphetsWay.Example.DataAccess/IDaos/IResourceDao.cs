using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.DataAccess.IDaos
{
	/// <summary>
	/// The Data Access Layer contract for <see cref="Resource"/>.
	/// </summary>
	/// <remarks>
	/// The Data-Access-Layer-wide snapshot rule on <see cref="IExampleDataAccess"/> binds every member of this
	/// Data Access Object. An instance returned by <c>Get</c> or <c>GetAll</c> is a snapshot; an instance handed
	/// to <c>Insert</c>, <c>Update</c> or <c>Delete</c> is read rather than adopted; and stored data changes only
	/// through those write members, each reading its argument as it stands at the moment of the call. See
	/// <see cref="IExampleDataAccess"/> for the full statement and for why the rule exists.
	/// <para>
	/// The Data-Access-Layer-wide identifier and row count rules on <see cref="IExampleDataAccess"/> bind this
	/// Data Access Object as well, and this is the one entity where the identifier rule's unspecified point
	/// bites. <c>Insert</c> assigns <see cref="Entities.Resource.Id"/> onto the instance the caller passed in,
	/// so a resource inserted with its identifier left at <see cref="System.Guid.Empty"/> carries a real one
	/// when the call returns. Because a <see cref="System.Guid"/> is client-generated rather than handed out by
	/// a database engine, whether an implementation honors a <see cref="System.Guid"/> the caller pre-assigned
	/// or replaces it is <b>not</b> specified — see the identifier rule — so do not pass one and do not depend
	/// on either answer. <c>Update</c> and <c>Delete</c> return <c>1</c> when the identifier matched a stored
	/// resource and <c>0</c> when it matched none — <c>1</c> from <c>Update</c> even where the incoming values
	/// are identical to the stored ones, because the count reports that a row matched rather than that a value
	/// changed.
	/// </para>
	/// </remarks>
	public interface IResourceDao : IBaseGetAllDao<Resource>
	{

	}
}
