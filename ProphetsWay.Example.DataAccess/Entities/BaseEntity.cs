using ProphetsWay.BaseDataAccess;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public abstract class BaseEntity : IBaseIdEntity<int>
	{
		public int Id { get; set; }
	}
}
