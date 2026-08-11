using System;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// A type nothing in the framework, the library, or this repository throws, so a test asserting it arrived
	/// is asserting that <i>this</i> exception travelled - not that something in the general shape of one did.
	/// </summary>
	/// <remarks>
	/// It exists for the passthrough showcase. Before <c>ProphetsWay.BaseDataAccess</c> 3.0.0, an exception
	/// thrown inside a derived Data Access Layer's convention method reached the caller wrapped in a
	/// <see cref="System.Reflection.TargetInvocationException"/>, because that is what
	/// <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/> does. Callers had to unwrap it
	/// themselves, and the original stack was buried. As of 3.0.0 the exception arrives as itself.
	/// </remarks>
	public class ShowcaseFailureException : Exception
	{
		public ShowcaseFailureException(string message)
			: base(message)
		{
		}
	}
}
