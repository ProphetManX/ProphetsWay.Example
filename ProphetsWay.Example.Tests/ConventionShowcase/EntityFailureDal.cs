using ProphetsWay.BaseDataAccess;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// A correctly wired Data Access Layer whose <c>Get</c> methods are never reached, because the entity
	/// throws first.
	/// </summary>
	/// <remarks>
	/// <c>Get&lt;T&gt;(object id)</c> does two things on the caller's behalf before it dispatches - it calls
	/// <c>new T()</c> and it writes the identifier onto the result - and either can run code the entity author
	/// wrote. Both are reached through reflection, and both unwrap what they catch, so an exception from an
	/// entity arrives no differently from one thrown by the Data Access Layer itself.
	/// </remarks>
	public class EntityFailureDal : ShowcaseDataAccess
	{
		public ThrowingConstructorEntity Get(ThrowingConstructorEntity item)
		{
			return item;
		}

		public ThrowingIdentifierEntity Get(ThrowingIdentifierEntity item)
		{
			return item;
		}
	}

	/// <summary>
	/// An entity whose parameterless constructor throws.
	/// </summary>
	/// <remarks>
	/// The <c>new()</c> constraint means the compiler has already guaranteed the constructor exists and is
	/// accessible, so the only thing that can go wrong at runtime is a constructor that throws. Worth a test of
	/// its own because <c>new T()</c> compiles to <c>Activator.CreateInstance&lt;T&gt;()</c>, which wraps a
	/// throwing constructor on .NET Framework and does not on .NET Core and later - the two targets fail
	/// differently underneath and are required to look identical from here.
	/// </remarks>
	public class ThrowingConstructorEntity : IBaseEntity
	{
		public int Id { get; set; }

		public ThrowingConstructorEntity()
		{
			throw new ShowcaseFailureException("Thrown by the parameterless constructor.");
		}
	}

	/// <summary>
	/// An entity whose identifier property has a set accessor that throws.
	/// </summary>
	/// <remarks>
	/// The setter exists, so this is not a convention failure - the property is assignable, it simply refuses.
	/// A validating setter is the realistic version of this: reject an identifier of zero, or a negative one,
	/// and the rejection has to reach the caller as the exception that was written rather than as a reflection
	/// artefact.
	/// </remarks>
	public class ThrowingIdentifierEntity : IBaseEntity
	{
		public int Id
		{
			get { return 0; }
			set { throw new ShowcaseFailureException("Thrown by the identifier property setter."); }
		}
	}
}
