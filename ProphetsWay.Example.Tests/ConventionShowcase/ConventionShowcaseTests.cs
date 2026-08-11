using Shouldly;

using Xunit;

using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;

namespace ProphetsWay.Example.Tests.ConventionShowcase
{
	/// <summary>
	/// What a mis-wired Data Access Layer looks like, and what the library says about it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every other test class in this repository exercises a Data Access Layer that works. This one exercises
	/// six that do not, so a reader can see the failure mode before meeting it in their own code. Each Data
	/// Access Layer in this folder makes exactly one mistake and is named for it, so the class name in the
	/// <c>act</c> phase is the whole explanation of why the call fails.
	/// </para>
	/// <para>
	/// The full specification of the convention - the method names and signatures, the visibility required, the
	/// declared return types, and how the identifier property is resolved - is the <c>&lt;remarks&gt;</c> on
	/// <see cref="DataAccessConventionException"/> in <c>ProphetsWay.BaseDataAccess</c>. This class is the
	/// illustrated companion to it, not a replacement for it.
	/// </para>
	/// <para>
	/// <b>No test here asserts on message text.</b> The wording is not part of the contract and pinning it would
	/// stop anyone improving it. The type is what is asserted; each Data Access Layer carries a real example of
	/// its message in an XML comment, where it informs without freezing.
	/// </para>
	/// <para>
	/// No <c>[Collection]</c>: none of these Data Access Layers reads or writes the store, so there is no shared
	/// state to serialise against.
	/// </para>
	/// </remarks>
	public class ConventionShowcaseTests
	{
		/// <summary>
		/// The generic type argument is not decoration - it is what routes the call through the dispatcher.
		/// <c>dal.Delete(company)</c> would bind straight to the derived <c>Delete(Company)</c> at compile time
		/// and no reflection would happen at all; <c>dal.Delete&lt;Company&gt;(company)</c> selects the generic
		/// member on <c>BaseDataAccess</c>, which is the one under test.
		/// </summary>
		[Fact]
		public void ShouldDispatchWhenTheConventionMethodIsPresent()
		{
			//setup
			using (var dal = new MissingMethodDal())
			{
				//act
				var count = dal.Delete<Company>(new Company());

				//assert
				count.ShouldBe(1);
			}
		}

		[Fact]
		public void ShouldThrowWhenTheConventionMethodIsMissing()
		{
			//setup - MissingMethodDal never declares Update(Company)
			using (var dal = new MissingMethodDal())
			{
				//act & assert
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.Update<Company>(new Company());
				});
			}
		}

		[Fact]
		public void ShouldDispatchWhenTheDeclaredReturnTypeIsCorrect()
		{
			//setup
			using (var dal = new WrongReturnTypeDal())
			{
				//act - GetCount declares int, as required
				var count = dal.GetCount<Company>();

				//assert
				count.ShouldBe(0);
			}
		}

		[Fact]
		public void ShouldThrowWhenTheDeclaredReturnTypeIsWrong()
		{
			//setup - GetAll declares IEnumerable<Company>, which is not assignable to IList<Company>
			using (var dal = new WrongReturnTypeDal())
			{
				//act & assert
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.GetAll<Company>();
				});
			}
		}

		/// <summary>
		/// The declared return type is checked before the method is invoked, so a mis-declared method never
		/// runs. This is the assertion that matters for <c>Update</c> and <c>Delete</c> in a real Data Access
		/// Layer - a wiring error cannot write to the database and only then report itself.
		/// </summary>
		[Fact]
		public void ShouldNotInvokeAMethodWhoseDeclaredReturnTypeIsWrong()
		{
			//setup
			using (var dal = new WrongReturnTypeDal())
			{
				//act
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.GetAll<Company>();
				});

				//assert - the body never ran
				dal.GetAllWasInvoked.ShouldBeFalse();
			}
		}

		[Fact]
		public void ShouldThrowWhenTheConventionMethodIsStatic()
		{
			//setup - GetAll(Company) is correct in every respect except that it is static
			using (var dal = new StaticMethodDal())
			{
				//act & assert - the lookup binds public instance methods only, so it cannot see it
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.GetAll<Company>();
				});
			}
		}

		[Fact]
		public void ShouldThrowWhenTheParameterIsABaseClassRatherThanTheEntityType()
		{
			//setup - Update takes BaseIntEntity, which Company derives from
			using (var dal = new BaseTypeParameterDal())
			{
				//act & assert - parameter types are matched exactly, not by assignability
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.Update<Company>(new Company());
				});
			}
		}

		[Fact]
		public void ShouldThrowWhenTheParameterIsAnInterfaceRatherThanTheEntityType()
		{
			//setup - Delete takes IBaseEntity, which Company implements
			using (var dal = new BaseTypeParameterDal())
			{
				//act & assert - the same rule as above, in its other flavour
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.Delete<Company>(new Company());
				});
			}
		}

		[Fact]
		public void ShouldThrowWhenTheEntityExposesNoIdentifierProperty()
		{
			//setup - the Data Access Layer is correct here; the entity is what is wrong
			using (var dal = new IdentifierShowcaseDal())
			{
				//act & assert
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.Get<NoIdentifierEntity>(1);
				});
			}
		}

		[Fact]
		public void ShouldThrowWhenTheIdentifierPropertyHasNoSetAccessor()
		{
			//setup
			using (var dal = new IdentifierShowcaseDal())
			{
				//act & assert
				Should.Throw<DataAccessConventionException>(() =>
				{
					dal.Get<GetOnlyIdentifierEntity>(1);
				});
			}
		}

		/// <summary>
		/// The counterpart to the two above, and the reason they are not simply "the identifier must be a public
		/// read-write property". A setter that exists but is not public is assignable by reflection and is fully
		/// supported.
		/// </summary>
		[Fact]
		public void ShouldDispatchWhenTheIdentifierPropertySetterIsNotPublic()
		{
			//setup
			using (var dal = new IdentifierShowcaseDal())
			{
				//act
				var found = dal.Get<PrivateSetterIdentifierEntity>(7);

				//assert - the identifier reached the probe entity through a private set accessor
				found.ShouldNotBeNull();
				found.Id.ShouldBe(7);
			}
		}
	}
}
