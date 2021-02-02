using ProphetsWay.BaseDataAccess;
using System;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public class Transaction : IBaseIdEntity<long>
	{
		public long Id { get; set; }

		public DateTime DateOfAction { get; set; }

		public decimal Amount { get; set; }

		public User User { get; set; }

		public Company Company { get; set; }
	}
}
