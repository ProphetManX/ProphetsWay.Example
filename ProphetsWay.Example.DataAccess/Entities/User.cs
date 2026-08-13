using ProphetsWay.Example.DataAccess.Enums;

namespace ProphetsWay.Example.DataAccess.Entities
{
	public class User : BaseIntEntity
	{
		public string Name { get; set; }

		public Company Company { get; set; }

		public Job Job { get; set; }

		/// <summary>
		/// The <see cref="Entities.Department"/> this user belongs to, if any.
		/// </summary>
		/// <value>
		/// The user's department, or <c>null</c> when the user is not assigned to one. <c>null</c> on a newly
		/// constructed instance.
		/// </value>
		/// <remarks>
		/// A navigation property, the same shape as <see cref="Company"/> and <see cref="Job"/> above, set and
		/// cleared by the caller — the Data Access Layer never assigns it and never validates that the department
		/// it names exists or is live. Because a department is soft-deleted rather than removed, this reference
		/// never dangles; see <see cref="IDaos.IDepartmentDao"/> for what that guarantee actually asserts and how
		/// it is demonstrated.
		/// </remarks>
		public Department Department { get; set; }

		public string Whatever { get; set; }

		public Roles RoleStr { get; set; }

		public Roles RoleInt { get; set; }
	}
}
