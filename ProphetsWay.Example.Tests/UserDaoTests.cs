using Xunit;
using System;
using ProphetsWay.Example.DataAccess.Entities;
using Shouldly;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.Tests
{
	/// <summary>
	/// The <see cref="IUserDao"/> contract, plus the two things about its custom member that are this
	/// implementation's own rather than the interface's. The traits are on the methods rather than on the class
	/// for that reason - see <see cref="ShouldGetCustomFunctionality"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <c>Scope</c> trait is declared per test rather than on the class, because two tests here are
	/// <c>Characterization</c> and the rest are <c>Contract</c>. xUnit accumulates traits rather than letting a
	/// method override a class, so a class-level <c>Contract</c> would leave those two selected by
	/// <c>--filter "Scope=Contract"</c> no matter what the methods declared.
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
		/// <b>Characterization, not contract.</b> <see cref="IUserDao"/> declares
		/// <see cref="IUserDao.CustomUserFunctionality"/> and then says in as many words that it "states no behavior
		/// of its own, and none is implied here". It therefore promises no outcome for a call against a stored user,
		/// and in particular promises nothing about throwing.
		/// </summary>
		/// <remarks>
		/// <para>
		/// That the member exists and can be called is guaranteed by the compiler rather than by this test - the
		/// project would not build without it - so all this adds is the observation that the in-memory
		/// implementation completes without throwing. That is a property of this implementation, and it is kept as
		/// its own test rather than folded into
		/// <see cref="ShouldNotAdoptTheInstanceHandedToCustomUserFunctionality"/> so that an implementation which
		/// does throw is reported as what it is rather than as a contract failure.
		/// </para>
		/// <para>
		/// It was <c>Contract</c> until this pass. Nothing in <see cref="IUserDao"/> or
		/// <see cref="ProphetsWay.Example.DataAccess.IExampleDataAccess"/> states a no-throw promise for this member,
		/// so as <c>Contract</c> it placed an obligation on every future implementer that no rule here makes. Do not
		/// promote it back without a rule to point at.
		/// </para>
		/// </remarks>
		[Fact]
		[Trait("Scope", "Characterization")]
		public void ShouldCallCustomUserFunctionality()
		{
			//setup
			var co = new User { Name = $"Eric {Guid.NewGuid()}" };
			_da.Insert(co);

			//act & assert
			Should.NotThrow(() => _da.CustomUserFunctionality(co));
		}

		/// <summary>
		/// The one thing <see cref="IUserDao"/> does state about
		/// <see cref="IUserDao.CustomUserFunctionality"/>, quoted from its <c>remarks</c>: "an instance handed to
		/// <c>Insert</c>, <c>Update</c>, <c>Delete</c> or <see cref="IUserDao.CustomUserFunctionality"/> is read
		/// rather than adopted". Nothing else in this suite calls the member and then writes to the instance it was
		/// handed, so an implementation of it as <c>_users[user.Id] = user</c> - keeping the caller's object as the
		/// stored row - satisfies every other assertion here.
		/// </summary>
		/// <remarks>
		/// <para>
		/// What the call itself does to the store is deliberately not asserted, because the same <c>remarks</c>
		/// leave that to the implementation. The store is read once immediately after the call and again after the
		/// caller's instance has been rewritten, and the two reads have to agree - which holds whatever the member
		/// chose to do, and fails only if the store is holding the caller's object.
		/// </para>
		/// <para>
		/// Both reads go through a second Data Access Layer instance, for the reason
		/// <see cref="DataAccessTransactionTests"/> gives: "stored" is a claim about the store, and an
		/// implementation that merely remembers what it was handed would satisfy an assertion made through the
		/// writer.
		/// </para>
		/// </remarks>
		[Fact]
		[Trait("Scope", "Contract")]
		public void ShouldNotAdoptTheInstanceHandedToCustomUserFunctionality()
		{
			//setup
			const string writtenAfterTheCall = "Written onto the caller's instance after the call returned.";

			var co = new User { Name = $"Eric {Guid.NewGuid()}" };
			_da.Insert(co);

			//act
			_da.CustomUserFunctionality(co);

			string nameAfterTheCall;
			string whateverAfterTheCall;

			using (var reader = TestDataAccessFactory.Create())
			{
				var settled = reader.Get(new User { Id = co.Id });
				nameAfterTheCall = settled.Name;
				whateverAfterTheCall = settled.Whatever;
			}

			co.Name = writtenAfterTheCall;
			co.Whatever = writtenAfterTheCall;

			//assert
			nameAfterTheCall.ShouldNotBe(writtenAfterTheCall);
			whateverAfterTheCall.ShouldNotBe(writtenAfterTheCall);

			using (var reader = TestDataAccessFactory.Create())
			{
				var stored = reader.Get(new User { Id = co.Id });

				stored.Name.ShouldBe(nameAfterTheCall);
				stored.Whatever.ShouldBe(whateverAfterTheCall);
			}
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