using ProphetsWay.BaseDataAccess;

using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// The mistake: the entity parameter is typed as something the entity <i>derives from</i> rather than as the
	/// entity type itself - a base class on <see cref="Update"/>, an interface on <see cref="Delete"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Both are the same rule seen twice. The convention matches parameter types <b>exactly</b>, positionally,
	/// rather than through the ordinary binder that would accept any parameter the argument is assignable to.
	/// A single <c>Update(BaseIntEntity)</c> looks like an economical way to serve every entity at once, and it
	/// is precisely what the lookup refuses.
	/// </para>
	/// <para>
	/// The exactness is what makes the convention work at all: the entity parameter exists only to disambiguate
	/// overloads by entity type, so a Data Access Layer declares one <c>Update</c> per entity and the lookup
	/// picks the right one. A parameter widened to a base type would collapse them into a single overload and
	/// there would be nothing left to select on.
	/// </para>
	/// <para>
	/// A widened parameter is indistinguishable from a missing method, and reports as one:
	/// <code>
	/// Unable to find a public instance method named 'Update' accepting (Company) on the data access type
	/// [BaseTypeParameterDal], required for the entity type [Company].
	/// </code>
	/// </para>
	/// </remarks>
	public class BaseTypeParameterDal : ShowcaseDataAccess
	{
		public int Update(BaseIntEntity item)
		{
			return 1;
		}

		public int Delete(IBaseEntity item)
		{
			return 1;
		}
	}
}
