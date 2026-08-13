using System.Collections.Generic;

using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// A correctly wired Data Access Layer whose every convention method throws
	/// <see cref="ShowcaseFailureException"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Nothing here is mis-wired - every method is found, every declared return type checks out, and every one
	/// of them is invoked. What is being demonstrated is what happens <i>after</i> that: an exception thrown by
	/// the body of a located method is not a convention failure, and the library does not treat it as one.
	/// </para>
	/// <para>
	/// <c>MethodBase.Invoke</c> wraps whatever the target threw in a
	/// <see cref="System.Reflection.TargetInvocationException"/>. As of <c>ProphetsWay.BaseDataAccess</c>
	/// 3.0.0 the library rethrows the inner exception with its original stack rather than handing the wrapper
	/// on, so a caller catches the exception the Data Access Layer actually threw and never has to know a
	/// reflection layer was involved.
	/// </para>
	/// </remarks>
	public class ThrowingDal : ShowcaseDataAccess
	{
		public IList<Company> GetAll(Company item)
		{
			throw new ShowcaseFailureException("Thrown by GetAll.");
		}

		public IList<Company> GetPaged(Company item, int skip, int take)
		{
			throw new ShowcaseFailureException("Thrown by GetPaged.");
		}

		public int GetCount(Company item)
		{
			throw new ShowcaseFailureException("Thrown by GetCount.");
		}

		public Company Get(Company item)
		{
			throw new ShowcaseFailureException("Thrown by Get.");
		}

		public void Insert(Company item)
		{
			throw new ShowcaseFailureException("Thrown by Insert.");
		}

		public int Update(Company item)
		{
			throw new ShowcaseFailureException("Thrown by Update.");
		}

		public int Delete(Company item)
		{
			throw new ShowcaseFailureException("Thrown by Delete.");
		}
	}
}
