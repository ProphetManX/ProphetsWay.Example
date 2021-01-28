namespace ProphetsWay.Example.Tests
{
	public abstract class BaseUnitTests<T>
	{
		protected T _da;

		public BaseUnitTests()
		{
			_da = GetIExampleDataAccess;
		}

		protected abstract T GetIExampleDataAccess { get; }
	}
}
