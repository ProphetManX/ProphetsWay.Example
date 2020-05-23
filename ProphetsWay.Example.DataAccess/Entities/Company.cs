using ProphetsWay.BaseDataAccess;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public class Company : IBaseEntity
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Other { get; set; }
	}
}
