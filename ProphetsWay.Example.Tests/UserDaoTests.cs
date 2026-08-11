using Xunit;
using System;
using ProphetsWay.Example.DataAccess.Entities;
using Shouldly;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.Tests
{
	[Collection(TestCollections.SharedStore)]
	[Trait("Scope", "Contract")]
	public class UserDaoTests : BaseUnitTests<IUserDao>
	{
		[Fact]
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

		[Fact]
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