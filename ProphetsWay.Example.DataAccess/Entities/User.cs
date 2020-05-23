using ProphetsWay.Example.DataAccess.Enums;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public class User : BaseEntity
	{
		public string Name { get; set; }

		public Company Company { get; set; }

		public Job Job { get; set; }

		public string Whatever { get; set; }

		public Roles RoleStr { get; set; }

		public Roles RoleInt { get; set; }
	}
}
