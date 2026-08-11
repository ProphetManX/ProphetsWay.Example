using System;
using System.Reflection;

using Shouldly;

using Xunit;

using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// An exception thrown inside a Data Access Layer reaches the caller as itself.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>BaseDataAccess</c> dispatches through <c>MethodBase.Invoke</c>, and reflection's default behaviour is
	/// to wrap anything the target threw in a <see cref="TargetInvocationException"/>. That wrapper leaked to
	/// callers before <c>ProphetsWay.BaseDataAccess</c> 3.0.0, and removing it was the loudest breaking change
	/// in that release: business logic could stop writing <c>catch (TargetInvocationException ex) when
	/// (ex.InnerException is SomethingIActuallyCareAbout)</c> and simply catch what the Data Access Layer
	/// threw.
	/// </para>
	/// <para>
	/// <b>This class is the regression guard for that, and the assertion that makes it one is
	/// <c>ShouldNotBeOfType&lt;TargetInvocationException&gt;</c>.</b> Asserting only that
	/// <see cref="ShowcaseFailureException"/> is reachable would pass under the old behaviour too, because the
	/// exception was always in there - it was just one <c>InnerException</c> down. Asserting the wrapper is
	/// absent is what would fail if a future version reintroduced it.
	/// </para>
	/// <para>
	/// The three places reflection is used are covered: the convention method itself, the <c>new T()</c> that
	/// <c>Get&lt;T&gt;(object)</c> makes to build its probe entity, and the identifier property setter it then
	/// calls. Each wraps for a different reason and each has to be unwrapped separately - and the constructor
	/// case behaves differently on .NET Framework and .NET Core underneath, which is why this suite runs on
	/// <c>net48</c> as well as the modern targets.
	/// </para>
	/// </remarks>
	public class ExceptionPassthroughShowcaseTests
	{
		/// <summary>
		/// The shared assertion, spelled out once so that each test below is a single line naming the member it
		/// covers.
		/// </summary>
		/// <remarks>
		/// The order is deliberate: the absence of the wrapper is asserted before the identity of the exception,
		/// so a regression reports as "was TargetInvocationException" rather than as a type mismatch the reader
		/// has to interpret. <see cref="Exception.InnerException"/> is checked because a wrapper that had been
		/// re-thrown as the right type would still carry the original underneath.
		/// </remarks>
		private static void ShouldArriveUnwrapped(Action act)
		{
			var thrown = Record.Exception(act);

			thrown.ShouldNotBeNull();
			thrown.ShouldNotBeOfType<TargetInvocationException>();
			thrown.ShouldBeOfType<ShowcaseFailureException>();
			thrown.InnerException.ShouldBeNull();
		}

		[Fact]
		public void ShouldNotWrapAnExceptionThrownByGetAll()
		{
			//setup
			using (var dal = new ThrowingDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.GetAll<Company>());
			}
		}

		[Fact]
		public void ShouldNotWrapAnExceptionThrownByGetPaged()
		{
			//setup
			using (var dal = new ThrowingDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.GetPaged<Company>(0, 10));
			}
		}

		[Fact]
		public void ShouldNotWrapAnExceptionThrownByGetCount()
		{
			//setup
			using (var dal = new ThrowingDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.GetCount<Company>());
			}
		}

		[Fact]
		public void ShouldNotWrapAnExceptionThrownByGet()
		{
			//setup
			using (var dal = new ThrowingDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.Get<Company>(1));
			}
		}

		[Fact]
		public void ShouldNotWrapAnExceptionThrownByInsert()
		{
			//setup
			using (var dal = new ThrowingDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.Insert<Company>(new Company()));
			}
		}

		[Fact]
		public void ShouldNotWrapAnExceptionThrownByUpdate()
		{
			//setup
			using (var dal = new ThrowingDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.Update<Company>(new Company()));
			}
		}

		[Fact]
		public void ShouldNotWrapAnExceptionThrownByDelete()
		{
			//setup
			using (var dal = new ThrowingDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.Delete<Company>(new Company()));
			}
		}

		/// <summary>
		/// The probe entity <c>Get&lt;T&gt;(object)</c> builds is constructed by <c>new T()</c>, which compiles
		/// to <c>Activator.CreateInstance&lt;T&gt;()</c> - and that wraps a throwing constructor on .NET
		/// Framework while rethrowing it untouched on .NET Core and later. The library normalises the two, so
		/// this test is only meaningful because the suite runs on <c>net48</c> as well.
		/// </summary>
		[Fact]
		public void ShouldNotWrapAnExceptionThrownByTheEntityConstructor()
		{
			//setup - the Get method on this Data Access Layer is correctly wired and is never reached
			using (var dal = new EntityFailureDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.Get<ThrowingConstructorEntity>(1));
			}
		}

		/// <summary>
		/// A setter that exists but refuses the value is not a convention failure - the identifier is
		/// assignable, the entity simply rejected what was assigned. A validating setter is the realistic case,
		/// and its complaint has to reach the caller intact.
		/// </summary>
		[Fact]
		public void ShouldNotWrapAnExceptionThrownByTheIdentifierPropertySetter()
		{
			//setup
			using (var dal = new EntityFailureDal())
			{
				//act & assert
				ShouldArriveUnwrapped(() => dal.Get<ThrowingIdentifierEntity>(1));
			}
		}
	}
}
