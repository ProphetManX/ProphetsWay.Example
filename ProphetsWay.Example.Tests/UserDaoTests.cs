using Xunit;
using System;
using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.NoDB;
using ProphetsWay.Example.DataAccess.Entities;
using FluentAssertions;

namespace ProphetsWay.Example.Tests
{
	public class UserDaoTests
	{
		IExampleDataAccess _da;

		public UserDaoTests(IExampleDataAccess da = null)
		{
			//single instantiation of the data access class, makes for easy changing to test a separate implementation
			_da = da ?? new ExampleDataAccess();
		}

		[Fact]
		public void ShouldInsertUser()
		{
			//setup
			var co = new User { Name = $"Bob {Guid.NewGuid()}" };

			//act
			_da.Insert(co);

			//assert
			co.Id.Should().NotBe(default);
		}

		public delegate void GetAssertion(User co);
		public static (int UserId, GetAssertion Assertion) SetupShouldGetUser(IExampleDataAccess da)
		{
			var co = new User { Name = $"Bob {Guid.NewGuid()}" };
			da.Insert(co);

			return (co.Id, (co2) =>
			{
				co2.Name.Should().Be(co.Name);
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
			count.Should().Be(1);
			co2.Whatever.Should().Be(editText);

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
			count.Should().Be(1);
			co2.Should().BeNull();
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
			co2.Id.Should().Be(co.Id);
			co2.Whatever.Should().NotBe(currWhatever);
			co2.Whatever.Should().Be("custom functionality triggered");
		}

	}
}