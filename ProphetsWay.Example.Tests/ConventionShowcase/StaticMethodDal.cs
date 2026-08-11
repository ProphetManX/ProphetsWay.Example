using System.Collections.Generic;

using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// The mistake: <c>GetAll(Company)</c> is <c>static</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the showcase worth reading twice. The name is right, the parameter is right, the return type is
	/// right, and the method is sitting in plain sight in the class - and the lookup cannot see it. The
	/// convention binds <c>public</c> and <c>instance</c> only, so a <c>static</c> method fails in exactly the
	/// same way, and with exactly the same message, as a method that was never written at all.
	/// </para>
	/// <para>
	/// That is deliberate rather than an oversight: the convention method has to be part of the Data Access
	/// Layer's public instance surface, because it is dispatched against an instance. The same rule makes
	/// <c>private</c>, <c>protected</c> and <c>internal</c> methods invisible too - which is the opposite of
	/// the rule governing an entity's identifier property, where a non-public setter is fully supported.
	/// </para>
	/// <para>
	/// The message reads:
	/// <code>
	/// Unable to find a public instance method named 'GetAll' accepting (Company) on the data access type
	/// [StaticMethodDal], required for the entity type [Company].
	/// </code>
	/// It says <c>instance</c>, and that word is the whole diagnosis.
	/// </para>
	/// </remarks>
	public class StaticMethodDal : ShowcaseDataAccess
	{
		public static IList<Company> GetAll(Company item)
		{
			return new List<Company>();
		}
	}
}
