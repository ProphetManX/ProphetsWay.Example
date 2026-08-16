using Xunit;
using System;
using ProphetsWay.Example.DataAccess.Entities;
using Shouldly;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The <see cref="IUserDao"/> contract, plus the one member of it whose behaviour is this implementation's
	/// own invention. The traits are on the methods rather than on the class for that reason - see
	/// <see cref="ShouldGetCustomFunctionality"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <c>Scope</c> trait is declared per test rather than on the class, because one test here is
	/// <c>Characterization</c> and the rest are <c>Contract</c>. xUnit accumulates traits rather than letting a
	/// method override a class, so a class-level <c>Contract</c> would leave that one test selected by
	/// <c>--filter "Scope=Contract"</c> no matter what the method declared.
	/// </para>
	/// </remarks>
	[Collection(TestCollections.SharedStore)]
	public class UserDaoTests : BaseUnitTests<IUserDao>
	{
		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldInsertUser()
		{
			//setup
			var co = new User { Name = $"Bob {Guid.NewGuid()}" };

			//act
			_da.Insert(co);

			//assert
			co.Id.ShouldNotBe(default);
		}

		public delegate void GetAssertion(User co);
		public static (int UserId, GetAssertion Assertion) SetupShouldGetUser(IUserDao da)
		{
			var co = new User { Name = $"Bob {Guid.NewGuid()}" };
			da.Insert(co);

			return (co.Id, (co2) =>
			{
				co2.Name.ShouldBe(co.Name);
			}
			);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldGetUser()
		{
			//setup
			var t = SetupShouldGetUser(_da);

			//act
			var co2 = _da.Get(new User { Id = t.UserId });

			//assert
			t.Assertion(co2);
		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldUpdateUser()
		{
			//setup
			const string editText = "blarg";
			var co = new User { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			co.Whatever = editText;
			var count = _da.Update(co);
			var co2 = _da.Get(co);

			//assert
			count.ShouldBe(1);
			co2.Whatever.ShouldBe(editText);

		}

		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldDeleteUser()
		{
			//setup
			var co = new User { Name = $"Bob {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			var count = _da.Delete(co);
			var co2 = _da.Get(co);

			//assert
			count.ShouldBe(1);
			co2.ShouldBeNull();
		}

		/// <summary>
		/// The only thing <see cref="IUserDao.CustomUserFunctionality"/> binds an implementation to: it is a
		/// member of the interface, so a conforming Data Access Layer supplies it and it can be called against a
		/// stored user. What it then does is <see cref="ShouldGetCustomFunctionality"/>'s subject, not this one's.
		/// </summary>
		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldCallCustomUserFunctionality()
		{
			//setup
			var co = new User { Name = $"Eric {Guid.NewGuid()}" };
			_da.Insert(co);

			//act & assert
			Should.NotThrow(() => _da.CustomUserFunctionality(co));
		}

		/// <summary>
		/// <see cref="IUserDao.CustomUserFunctionality"/> stands in for whatever command a real Data Access Object
		/// would add beyond the surface it inherits, and its <c>remarks</c> say in as many words that what it does,
		/// and what if anything it writes back onto the caller's instance, is the implementation's to define. This
		/// implementation stamps <see cref="User.Whatever"/> with <c>CustomFunctionalityStamp</c> - a
		/// <c>private const</c> of the in-memory <c>UserDao</c>, spelled out here because the interface names no
		/// value the test could read. An implementation that wrote nothing, or wrote something else, would be
		/// equally conforming, so both the change and the value are this implementation's characterization rather
		/// than the contract's. Do not promote this back to <c>Contract</c>.
		/// </summary>
		[Fact]
		[Trait("Scope", "Characterization")]
		public void ShouldGetCustomFunctionality()
		{
			//setup
			var co = new User { Name = $"Eric {Guid.NewGuid()}" };
			_da.Insert(co);
			var currWhatever = co.Whatever;

			//act
			_da.CustomUserFunctionality(co);
			var co2 = _da.Get(co);

			//assert
			co2.Id.ShouldBe(co.Id);
			co2.Whatever.ShouldNotBe(currWhatever);
			co2.Whatever.ShouldBe("custom functionality triggered");
		}

	}
}