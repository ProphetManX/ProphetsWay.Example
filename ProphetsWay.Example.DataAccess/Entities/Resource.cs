using ProphetsWay.BaseDataAccess;
using System;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public class Resource : IBaseIdEntity<Guid>
	{
		public Resource()
        {
			Id = Guid.NewGuid();
        }

		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }
	}
}
