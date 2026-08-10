using ProphetsWay.BaseDataAccess;
using System;

namespace ProphetsWay.Example.DataAccess.Entities
{
	/// <summary>
	/// A department within the example organization, and the repository's showcase for the soft-delete
	/// capability of <see cref="IBaseSoftIdEntity{T}"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>What it is.</b> An int-keyed entity — <see cref="BaseIntEntity"/> supplies
	/// <see cref="BaseIntEntity.Id"/> — that additionally carries the three
	/// <see cref="IBaseSoftEntity"/> timestamps, so it is never removed from the store, only stamped as
	/// deleted. <see cref="IDaos.IDepartmentDao"/> carries the full behavioral contract and the argument for
	/// why the shape is worth demonstrating; this type is only the data.
	/// </para>
	/// <para>
	/// <b><see cref="BaseIntEntity.Id"/> is generated, not supplied.</b> It is zero on a newly constructed
	/// instance and is assigned by <c>Insert</c>, which writes the generated value back onto the instance the
	/// caller passed in, replacing anything the caller had assigned. A caller sets it itself only to say which
	/// department to fetch, update, delete, or restore.
	/// </para>
	/// <para>
	/// <b>Each timestamp is owned by the operation that stamps it.</b> <c>Insert</c> owns
	/// <see cref="CreatedDate"/>, <c>Update</c> owns <see cref="UpdatedDate"/>, and <c>Delete</c> and
	/// <c>Restore</c> own <see cref="DeletedDate"/>. A value the caller assigns to a timestamp is overwritten
	/// by that timestamp's own operation and is neither read nor written by any other operation — with one
	/// exception: <c>Insert</c> clears all three to their initial state, stamping <see cref="CreatedDate"/> and
	/// setting <see cref="UpdatedDate"/> and <see cref="DeletedDate"/> to <c>null</c> whatever the caller had
	/// assigned. The three are settable only because <see cref="IBaseSoftEntity"/> requires them to be.
	/// </para>
	/// <para>
	/// <b>Every stamp is Coordinated Universal Time.</b> A stamped value is <see cref="DateTime.UtcNow"/> and
	/// carries a <see cref="DateTime.Kind"/> of <see cref="DateTimeKind.Utc"/>; see rule 18 on
	/// <see cref="IDaos.IDepartmentDao"/> for the binding statement.
	/// </para>
	/// <para>
	/// <b>This type carries no behavior.</b> It is a data-carrying object with automatic properties and a
	/// default parameterless constructor. It performs no validation, so any property may hold any value the
	/// compiler allows, including <c>null</c> strings; enforcing rules about what makes a valid department is
	/// the job of a business-logic layer, not of an entity in the Data Access Layer.
	/// </para>
	/// <para>
	/// Equality is the default reference equality inherited from <see cref="object"/>. Two separately
	/// retrieved instances describing the same department row are not equal to one another; compare
	/// <see cref="BaseIntEntity.Id"/> instead.
	/// </para>
	/// </remarks>
	public class Department : BaseIntEntity, IBaseSoftIdEntity<int>
	{
		/// <summary>
		/// The display name of the department.
		/// </summary>
		/// <value>
		/// The department's name. <c>null</c> on a newly constructed instance, and the Data Access Layer
		/// neither rejects nor rewrites a <c>null</c> or empty name.
		/// </value>
		/// <remarks>
		/// Not required to be unique. Nothing in the Data Access Layer keys off this value, so two departments
		/// may share a name, and a soft-deleted department does not release its name for reuse.
		/// </remarks>
		public string Name { get; set; }

		/// <summary>
		/// Free-form text describing what the department does.
		/// </summary>
		/// <value>
		/// The description, or <c>null</c> when none has been supplied. <c>null</c> on a newly constructed
		/// instance.
		/// </value>
		/// <remarks>
		/// Present so that the example can demonstrate updating one field while leaving another untouched —
		/// a change to <see cref="Description"/> alone still stamps <see cref="UpdatedDate"/>.
		/// </remarks>
		public string Description { get; set; }

		/// <summary>
		/// The moment this department was first stored.
		/// </summary>
		/// <value>
		/// The creation timestamp. <see cref="DateTime.MinValue"/> on a newly constructed instance that has not
		/// yet been inserted; never <see cref="DateTime.MinValue"/> on an instance returned from the Data
		/// Access Layer.
		/// </value>
		/// <remarks>
		/// Stamped by <c>Insert</c> onto the instance the caller passed in, overwriting whatever the caller had
		/// assigned. Never changed afterwards — not by <c>Update</c>, not by <c>Delete</c>, and not by
		/// <c>Restore</c>. A value carried on an instance handed to <c>Update</c> is ignored, and the stored
		/// value is preserved.
		/// </remarks>
		public DateTime CreatedDate { get; set; }

		/// <summary>
		/// The moment this department was last modified, if it ever has been.
		/// </summary>
		/// <value>
		/// The timestamp of the most recent <c>Update</c> that returned <c>1</c>, or <c>null</c> when the
		/// department has never been updated since it was inserted.
		/// </value>
		/// <remarks>
		/// Stamped by <c>Update</c> only. <c>Insert</c> clears it to <c>null</c>, so a department that has been
		/// created but never modified is distinguishable from one that has been modified. <c>Delete</c> and
		/// <c>Restore</c> do not touch it — they are not modifications of the department's own data.
		/// </remarks>
		public DateTime? UpdatedDate { get; set; }

		/// <summary>
		/// The moment this department was soft-deleted, if it currently is.
		/// </summary>
		/// <value>
		/// The timestamp of the <c>Delete</c> that removed this department from ordinary circulation, or
		/// <c>null</c> when the department is live. A non-<c>null</c> value is the single authoritative signal
		/// that a department is soft-deleted.
		/// </value>
		/// <remarks>
		/// Stamped by <c>Delete</c> and cleared back to <c>null</c> by <c>Restore</c>. A second <c>Delete</c>
		/// against an already-deleted department leaves the existing value untouched rather than refreshing it,
		/// so this timestamp always reports when the department was actually deleted. A department with a
		/// non-<c>null</c> value here is still retrievable by <c>Get</c>, and is excluded from <c>GetAll</c>,
		/// <c>GetPaged</c>, and <c>GetCount</c>. A value carried on an instance handed to <c>Update</c> is
		/// ignored, and the stored value is preserved.
		/// </remarks>
		public DateTime? DeletedDate { get; set; }
	}
}
