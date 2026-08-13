using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// The mistake: <c>Update(Company)</c> was never written.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>BaseDataAccess.Update&lt;T&gt;(T)</c> looks for a public instance method named <c>Update</c> taking a
	/// single parameter of exactly the entity type. There is no compiler error for leaving it out - the generic
	/// member exists on the base class, so the call site compiles, and the defect appears the first time that
	/// entity type is updated.
	/// </para>
	/// <para>
	/// <c>Delete(Company)</c> below is written correctly, and is here so the contrast is visible in one class:
	/// the same Data Access Layer dispatches one member and fails on the other.
	/// </para>
	/// <para>
	/// The message reads:
	/// <code>
	/// Unable to find a public instance method named 'Update' accepting (Company) on the data access type
	/// [MissingMethodDal], required for the entity type [Company].
	/// </code>
	/// </para>
	/// </remarks>
	public class MissingMethodDal : ShowcaseDataAccess
	{
		public int Delete(Company item)
		{
			return 1;
		}
	}
}
