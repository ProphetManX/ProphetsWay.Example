using ProphetsWay.BaseDataAccess;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public abstract class BaseIntEntity : IBaseIdEntity<int>
	{
		public int Id { get; set; }
	}
}
