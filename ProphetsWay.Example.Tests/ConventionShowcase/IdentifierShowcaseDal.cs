using ProphetsWay.BaseDataAccess;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// A correctly wired Data Access Layer, used to demonstrate the mistakes an <b>entity</b> can make.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>Get&lt;T&gt;(object id)</c> is the only member that needs an identifier property. It constructs a
	/// probe entity, writes the identifier onto it, and hands it to the derived <c>Get</c> to select the
	/// overload. <c>GetAll</c>, <c>GetPaged</c> and <c>GetCount</c> pass <c>null</c> and never look for one, so
	/// an entity with no usable identifier works perfectly everywhere except here.
	/// </para>
	/// <para>
	/// Every <c>Get</c> below is declared exactly as the convention requires. That is the point: when one of
	/// these calls throws, the Data Access Layer is not what is wrong, and the message names the entity rather
	/// than the method.
	/// </para>
	/// </remarks>
	public class IdentifierShowcaseDal : ShowcaseDataAccess
	{
		public NoIdentifierEntity Get(NoIdentifierEntity item)
		{
			return item;
		}

		public GetOnlyIdentifierEntity Get(GetOnlyIdentifierEntity item)
		{
			return item;
		}

		public PrivateSetterIdentifierEntity Get(PrivateSetterIdentifierEntity item)
		{
			return item;
		}
	}

	/// <summary>
	/// The mistake: no identifier property at all.
	/// </summary>
	/// <remarks>
	/// The identifier is resolved <b>by name</b> - <c>{TypeName}Id</c> first, then <c>Id</c> - and by nothing
	/// else. No attribute is read, no interface is consulted, and <see cref="IBaseEntity"/> is a marker with no
	/// members, so implementing it promises nothing about shape. <c>Name</c> is not an identifier however
	/// obviously it identifies something.
	/// <code>
	/// The entity type [NoIdentifierEntity] exposes neither a 'NoIdentifierEntityId' nor an 'Id' property, so
	/// no identifier can be assigned to it.
	/// </code>
	/// </remarks>
	public class NoIdentifierEntity : IBaseEntity
	{
		public string Name { get; set; }
	}

	/// <summary>
	/// The mistake: the identifier property exists but has no set accessor.
	/// </summary>
	/// <remarks>
	/// The name resolves, and then there is nowhere to put the identifier. A computed or expression-bodied
	/// identifier reads naturally in source and is unusable here for the same reason.
	/// <code>
	/// The entity type [GetOnlyIdentifierEntity] exposes an identifier property 'Id' with no set accessor, so
	/// no identifier can be assigned to it.
	/// </code>
	/// </remarks>
	public class GetOnlyIdentifierEntity : IBaseEntity
	{
		public int Id
		{
			get { return 0; }
		}
	}

	/// <summary>
	/// Not a mistake - the one that is here to show where the line actually falls.
	/// </summary>
	/// <remarks>
	/// A <c>private set</c> is resolved and invoked by reflection exactly as a public one is, as are
	/// <c>protected</c>, <c>internal</c> and <c>init</c>. The convention requires the identifier to be
	/// <i>assignable</i>, not <i>publicly</i> assignable, so an entity that hides its identifier setter from
	/// ordinary callers still works. That is the reverse of the rule governing the method lookup, where
	/// anything less than public is invisible - and having both rules in front of the reader at once is the
	/// only way that asymmetry stops looking arbitrary.
	/// </remarks>
	public class PrivateSetterIdentifierEntity : IBaseEntity
	{
		public int Id { get; private set; }
	}
}
