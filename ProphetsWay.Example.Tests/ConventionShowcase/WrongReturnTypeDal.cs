using System.Collections.Generic;

using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// The mistake: <c>GetAll(Company)</c> declares <see cref="IEnumerable{T}"/> where the convention requires a
	/// return type assignable to <see cref="IList{T}"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the easiest of the four to write by accident, because <see cref="IEnumerable{T}"/> is the reflex
	/// return type for a query and the value actually returned - a <see cref="List{T}"/> - would have been
	/// perfectly usable. The convention checks the type the method <i>declares</i>, not the type of the object
	/// it hands back, so the value is never examined and the method is never called.
	/// </para>
	/// <para>
	/// <see cref="GetCount"/> below is declared correctly and dispatches, so the failure is clearly the return
	/// type of one method rather than anything about the Data Access Layer as a whole.
	/// </para>
	/// <para>
	/// The message reads:
	/// <code>
	/// The method named 'GetAll' on the data access type [WrongReturnTypeDal], required for the entity type
	/// [Company], declares a return type of [IEnumerable&lt;Company&gt;] which cannot be used as
	/// [IList&lt;Company&gt;].
	/// </code>
	/// </para>
	/// </remarks>
	public class WrongReturnTypeDal : ShowcaseDataAccess
	{
		/// <summary>
		/// Records whether the mis-declared method below ever ran, so a test can show that it did not.
		/// </summary>
		/// <remarks>
		/// The check happens before invocation, which is what stops a mis-declared <c>Update</c> or
		/// <c>Delete</c> from writing to the database and only then reporting the defect.
		/// </remarks>
		public bool GetAllWasInvoked { get; private set; }

		public IEnumerable<Company> GetAll(Company item)
		{
			GetAllWasInvoked = true;

			return new List<Company>();
		}

		public int GetCount(Company item)
		{
			return 0;
		}
	}
}
