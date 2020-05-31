using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.NoDB;

namespace ProphetsWay.Example.Tests
{
	public abstract class BaseUnitTests
	{
		protected IExampleDataAccess _da;

		public BaseUnitTests()
		{
			_da = GetIExampleDataAccess;
		}

		protected virtual IExampleDataAccess GetIExampleDataAccess => new ExampleDataAccess();
	}
}
