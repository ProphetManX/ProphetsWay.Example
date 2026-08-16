using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using System;

namespace ProphetsWay.Example.DataAccess.IDaos
{
	/// <summary>
	/// The Data Access Layer contract for <see cref="Department"/>, and the repository's showcase for
	/// soft-delete semantics.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The surface is <c>Get</c>, <c>Insert</c>, <c>Update</c>, <c>Delete</c>, <c>GetAll</c>, <c>GetPaged</c>,
	/// <c>GetCount</c>, and <see cref="Restore"/>.
	/// </para>
	///
	/// <para><b>CONTRACT.</b> Every numbered rule below is binding on any implementation.</para>
	///
	/// <para>
	/// <b>1.</b> <c>Insert</c> stamps <see cref="Department.CreatedDate"/> and assigns the generated
	/// <see cref="Entities.BaseIntEntity.Id"/>, both written back onto the instance the caller passed in. Any
	/// <see cref="Entities.BaseIntEntity.Id"/> the caller pre-assigned is overwritten with the generated one.
	/// <see cref="Department.UpdatedDate"/> and <see cref="Department.DeletedDate"/> are <c>null</c> when the
	/// call returns, whatever the caller had assigned to them.
	/// </para>
	///
	/// <para>
	/// <b>2.</b> <c>Update</c> stamps <see cref="Department.UpdatedDate"/>, written back onto the instance the
	/// caller passed in, and returns <c>1</c> when a department with that
	/// <see cref="Entities.BaseIntEntity.Id"/> is stored. Otherwise it returns <c>0</c> and changes nothing.
	/// Repeated updates overwrite <see cref="Department.UpdatedDate"/> each time.
	/// </para>
	///
	/// <para>
	/// <b>3.</b> <c>Update</c> writes the department's own data only. Values carried on the incoming instance
	/// for <see cref="Department.CreatedDate"/>, <see cref="Department.UpdatedDate"/> and
	/// <see cref="Department.DeletedDate"/> are ignored; the stored <see cref="Department.CreatedDate"/> and
	/// <see cref="Department.DeletedDate"/> are preserved.
	/// </para>
	///
	/// <para>
	/// <b>4.</b> <c>Update</c> on a soft-deleted department is allowed and behaves exactly as rules 2 and 3
	/// describe. The department stays deleted.
	/// </para>
	///
	/// <para>
	/// <b>5.</b> <c>Delete</c> does not remove the record. It stamps <see cref="Department.DeletedDate"/>,
	/// written back onto the instance the caller passed in, and returns <c>1</c> when a live department with
	/// that <see cref="Entities.BaseIntEntity.Id"/> is stored. <see cref="Department.CreatedDate"/> and
	/// <see cref="Department.UpdatedDate"/> are not touched, and the department remains retrievable by
	/// <c>Get</c>.
	/// </para>
	///
	/// <para>
	/// <b>6.</b> <c>Delete</c> returns <c>0</c> and changes nothing when the department is already deleted or
	/// when no department with that <see cref="Entities.BaseIntEntity.Id"/> is stored. An existing
	/// <see cref="Department.DeletedDate"/> is left exactly as it was and is <i>not</i> refreshed, so
	/// <c>Delete</c> is idempotent.
	/// </para>
	///
	/// <para>
	/// <b>7.</b> <see cref="Restore"/> clears <see cref="Department.DeletedDate"/> back to <c>null</c>,
	/// written back onto the instance the caller passed in. See the member for its full contract.
	/// </para>
	///
	/// <para>
	/// <b>8.</b> <c>Get</c> returns the department even when it is soft-deleted, carrying a non-<c>null</c>
	/// <see cref="Department.DeletedDate"/>. It returns <c>null</c> only when no department with that
	/// <see cref="Entities.BaseIntEntity.Id"/> was ever stored.
	/// </para>
	///
	/// <para>
	/// <b>9.</b> <c>GetAll</c>, <c>GetPaged</c> and <c>GetCount</c> all omit soft-deleted departments and must
	/// agree with each other: <c>GetCount</c> returns exactly the number of departments <c>GetAll</c> returns,
	/// and paging through <c>GetPaged</c> enumerates exactly that same set. With every department deleted,
	/// <c>GetAll</c> and <c>GetPaged</c> return an empty list and <c>GetCount</c> returns <c>0</c>.
	/// </para>
	///
	/// <para>
	/// <b>10.</b> <c>GetAll</c> and <c>GetPaged</c> always return a list, never <c>null</c>.
	/// </para>
	///
	/// <para>
	/// <b>11.</b> Ordering is unspecified but stable across calls for as long as the stored data is unchanged.
	/// Successive <c>GetPaged</c> windows therefore partition the <c>GetAll</c> set with no overlap and no
	/// omission.
	/// </para>
	///
	/// <para>
	/// <b>12.</b> <c>GetPaged</c> boundaries: a <c>skip</c> beyond the available count returns an empty list, a
	/// <c>take</c> of <c>0</c> returns an empty list, a <c>take</c> larger than the number of departments
	/// remaining after <c>skip</c> returns that remainder rather than throwing or padding, and a negative
	/// <c>skip</c> or <c>take</c> throws <see cref="ArgumentOutOfRangeException"/>.
	/// </para>
	///
	/// <para>
	/// <b>13.</b> The <c>item</c> parameter of <c>GetAll</c>, <c>GetPaged</c> and <c>GetCount</c> is a type
	/// selector only. Reached through the generic dispatcher on <c>BaseDataAccess</c> it is <c>null</c>, so an
	/// implementation must never read it.
	/// </para>
	///
	/// <para>
	/// <b>14.</b> <c>Get</c>, <c>Insert</c>, <c>Update</c>, <c>Delete</c> and <see cref="Restore"/> throw
	/// <see cref="ArgumentNullException"/> when passed <c>null</c>. <c>GetAll</c>, <c>GetPaged</c> and
	/// <c>GetCount</c> do not — see rule 13.
	/// </para>
	///
	/// <para>
	/// <b>15.</b> Soft-deletion is the only exclusion rule. Nothing else about a department's data causes it to
	/// be filtered out of any retrieval method.
	/// </para>
	///
	/// <para>
	/// <b>16.</b> <c>Get&lt;Department&gt;(object id)</c> on the generic dispatcher requires an
	/// <see cref="int"/>. An <c>id</c> that is <c>null</c>, or of any type other than <see cref="int"/>, throws
	/// <see cref="ArgumentException"/> from the reflective setter — not
	/// <see cref="DataAccessConventionException"/>.
	/// </para>
	///
	/// <para>
	/// <b>17.</b> <c>Get</c> returns the department it found as a snapshot under rule 19, never the stored
	/// instance itself. Whether that snapshot is the instance the caller passed in, populated in place, or a
	/// separate instance is unspecified — assert on the return value, never on the argument. Populating the
	/// argument in place is permitted only because the argument belongs to the caller; it is never the store's
	/// own object.
	/// </para>
	///
	/// <para>
	/// <b>18.</b> Every timestamp this contract stamps is the value of <see cref="DateTime.UtcNow"/> read during
	/// the call that stamps it — <see cref="Department.CreatedDate"/> by <c>Insert</c>,
	/// <see cref="Department.UpdatedDate"/> by <c>Update</c>, <see cref="Department.DeletedDate"/> by
	/// <c>Delete</c>. Local time is never used. Each stamped value has a <see cref="DateTime.Kind"/> of
	/// <see cref="DateTimeKind.Utc"/> on the instance written back to the caller, and again on an instance
	/// later retrieved by <c>Get</c>, <c>GetAll</c> or <c>GetPaged</c> <i>on this interface</i>. It does not
	/// bind a <see cref="Department"/> reached as a navigation property of an entity retrieved through another
	/// Data Access Object — <see cref="User.Department"/> on a user returned by <see cref="IUserDao"/>, say —
	/// which carries whatever <see cref="DateTime.Kind"/> the provider supplied, typically
	/// <see cref="DateTimeKind.Unspecified"/> from a relational one, because restoring a kind the provider does
	/// not persist is a per-Data-Access-Object mechanism and the Data Access Object that ran the read has none
	/// for these three timestamps. A caller reading a timestamp off an included department must therefore treat
	/// it as Coordinated Universal Time explicitly, with
	/// <see cref="DateTime.SpecifyKind(DateTime, DateTimeKind)"/>, rather than trust the
	/// <see cref="DateTime.Kind"/> it finds: an <see cref="DateTimeKind.Unspecified"/> value handed to
	/// <see cref="DateTime.ToLocalTime"/> is taken for local time and shifted by the machine's offset, so the
	/// failure is a silently wrong value and not an exception. This is the same shape as rule 9 — an include is
	/// outside the mechanisms the retrieving Data Access Object applies to its own reads, which is equally why
	/// a department reached through <see cref="User.Department"/> comes back populated even when it is
	/// soft-deleted.
	/// </para>
	///
	/// <para>
	/// <b>19.</b> An instance returned by <c>Get</c>, <c>GetAll</c> or <c>GetPaged</c> is a snapshot, and an
	/// instance handed to <c>Insert</c>, <c>Update</c>, <c>Delete</c> or <see cref="Restore"/> is read rather
	/// than adopted: mutating a returned instance does not change stored data, and mutating an argument after
	/// the call returns does not reach the store. Stored data changes only through <c>Insert</c>,
	/// <c>Update</c>, <c>Delete</c> and <see cref="Restore"/>, each of which reads its argument's values as
	/// they stand at the moment of the call. Fetching a department, editing the fetched instance and then
	/// calling <c>Update</c> therefore leaves the store untouched until that <c>Update</c> runs, and two
	/// instances retrieved separately are independent of each other. The write-backs described by rules 1, 2,
	/// 5 and 7 are the only values that travel from the store back onto a caller's instance.
	/// </para>
	///
	/// <para><b>WHY.</b></para>
	///
	/// <para>
	/// <b>Two inherited interfaces.</b> <see cref="IBaseGetAllDao{T}"/> supplies <c>GetAll</c> and
	/// <see cref="IBasePagedDao{T}"/> supplies <c>GetPaged</c> and <c>GetCount</c> together; there is no
	/// standalone count interface. Inheriting both is the supported way to get all three, and both derive from
	/// the same <see cref="IBaseDao{T}"/>, so the four CRUD members are inherited once and are not ambiguous.
	/// </para>
	///
	/// <para>
	/// <b>Rule 3 is the one that gets broken.</b> The obvious implementation of <c>Update</c> is
	/// whole-object replacement, which silently wipes <see cref="Department.DeletedDate"/> whenever the caller
	/// passes an instance fetched before the delete. Soft-delete then fails with nothing to point at.
	/// </para>
	///
	/// <para>
	/// <b>Rule 4 is deliberate.</b> Refusing to modify a deleted record would be a <i>business rule</i>, and
	/// business rules belong in a Core layer where they can be stated, tested and changed on their own terms. A
	/// Data Access Layer that quietly enforces policy of its own cannot be reasoned about from the outside and
	/// cannot be swapped, because a replacement would have to rediscover the policy to behave identically.
	/// </para>
	///
	/// <para>
	/// <b>Rule 8 is proved by a round trip, not by an object reference.</b> Delete the department, then pass a
	/// fresh <see cref="Department"/> carrying only the <see cref="Entities.BaseIntEntity.Id"/> to <c>Get</c>
	/// and inspect what comes back. A <see cref="User"/> instance already in hand keeps its
	/// <see cref="User.Department"/> reference regardless of what the store does, so that reference says
	/// nothing about the Data Access Layer.
	/// </para>
	///
	/// <para>
	/// <b>Rule 10 narrows the base library.</b> <c>BaseDataAccess.GetAll&lt;T&gt;</c> permits <c>null</c>; this
	/// contract does not. That is intentional — a caller of this DAO never needs a null check.
	/// </para>
	///
	/// <para>
	/// <b>Rules 11 and 12 exist because conforming implementations would otherwise disagree.</b> Without stable
	/// ordering, the rule 9 agreement test is flaky by construction. Without a stated answer on negatives, a
	/// LINQ-backed DAL silently no-ops where a SQL-backed one throws — two DALs behaving differently against
	/// the same contract is the exact failure this repository exists to disprove.
	/// </para>
	///
	/// <para>
	/// <b>Rule 13 contradicts the inherited documentation.</b> <see cref="IBaseGetAllDao{T}"/> says the
	/// parameter "just needs to be an instance of itself"; under the dispatcher it is <c>null</c>. An
	/// implementer who trusts the inherited text writes a <see cref="NullReferenceException"/> the compiler
	/// cannot catch.
	/// </para>
	///
	/// <para>
	/// <b>Rule 18 names the clock because a Data Access Layer that does not is untestable.</b> Without it a test
	/// can only assert a wide sanity window, which a hardcoded date would pass. Coordinated Universal Time is
	/// the default a Data Access Layer should demonstrate: local time repeats an hour and skips an hour every
	/// year, so rows stamped across a daylight-saving boundary sort wrongly and cannot be ordered by time at all.
	/// </para>
	///
	/// <para>
	/// <b>Rule 19 is what makes this repository's central claim true.</b> A database hands back rows, not
	/// object references. An in-memory store that hands back the object it is holding gives a caller a way to
	/// change stored data that no database-backed implementation can reproduce, and the claim that the same
	/// tests pass against either Data Access Layer would be quietly false. Stating the rule forces an
	/// in-memory implementation to copy on read and on write, so a dictionary-backed store and a
	/// database-backed one are genuinely interchangeable. It also gives the delete and restore assertions
	/// their teeth: without it, a method that stamps only its argument and writes nothing passes, because the
	/// assertion reads back the very object the method mutated.
	/// </para>
	/// </remarks>
	public interface IDepartmentDao : IBaseGetAllDao<Department>, IBasePagedDao<Department>
	{
		/// <summary>
		/// Clears a soft-delete, returning the department to ordinary circulation. An illustrative custom
		/// method, not a feature of <c>ProphetsWay.BaseDataAccess</c>.
		/// </summary>
		/// <param name="item">
		/// The department to restore. Only <see cref="Entities.BaseIntEntity.Id"/> need be set; the remaining
		/// properties are ignored.
		/// </param>
		/// <returns>
		/// <c>1</c> when the department was soft-deleted and has now been restored. <c>0</c> when nothing was
		/// changed, which covers both a department that was already live and an
		/// <see cref="Entities.BaseIntEntity.Id"/> that matches no stored department.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="item"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		/// <para>
		/// Clears <see cref="Department.DeletedDate"/> back to <c>null</c>, written back onto the instance the
		/// caller passed in. After a successful restore the department reappears in <c>GetAll</c>,
		/// <c>GetPaged</c> and <c>GetCount</c>. Restoring a department that is not deleted, or an identifier
		/// that matches nothing, is a no-op returning <c>0</c> rather than an error, so the method is
		/// idempotent.
		/// </para>
		/// <para>
		/// Touches no timestamp other than <see cref="Department.DeletedDate"/>. In particular it does
		/// <i>not</i> stamp <see cref="Department.UpdatedDate"/> — a restore is a change of lifecycle state,
		/// not a modification of the department's data.
		/// </para>
		/// <para>
		/// <b>Illustrative, not prescribed.</b> <c>ProphetsWay.BaseDataAccess</c> has no notion of restoring;
		/// this is an ordinary custom method on a DAO interface, exactly like
		/// <see cref="ICompanyDao.GetCustomCompanyFunction"/>, reached through
		/// <see cref="IExampleDataAccess"/> like any other member — that is the point it makes. Do not read it
		/// as a recommended pattern: a domain that genuinely needs user-facing reversible removal usually wants
		/// an explicit <i>archive</i> flag modeled as part of the domain, and any lifecycle richer than
		/// "deleted or not" belongs to the consumer's business layer.
		/// </para>
		/// </remarks>
		int Restore(Department item);
	}
}
