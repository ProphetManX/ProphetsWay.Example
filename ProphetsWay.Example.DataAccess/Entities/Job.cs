using ProphetsWay.BaseDataAccess;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public class Job : IBaseEntity
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Something { get; set; }

	}
}
