using ProphetsWay.BaseDataAccess;
using ProphetsWay.Example.DataAccess.IDaos;

namespace ProphetsWay.Example.DataAccess
{
	/// <summary>
	/// The single Data Access Layer contract a consumer injects — the interface of all interfaces, aggregating
	/// every DAO in this example alongside <see cref="IBaseDataAccess"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// So long as a DAL implementation takes this interface as its main input and the entities stay defined in
	/// this project, the current DAL can be decoupled and swapped for a newly written one. Any unit tests
	/// written against one implementation should need little or no modification to target the next — that
	/// interchangeability is the whole argument this repository makes.
	/// </para>
	/// <para>
	/// Two members are here to show the edges of the paradigm: <see cref="IDepartmentDao"/> showcases
	/// soft-delete and a custom method, and <see cref="ICompanyResourceDao"/> showcases an entity with no
	/// identifier and a DAO that inherits <see cref="IBaseDao{T}"/> not at all.
	/// </para>
	///
	/// <para><b>SNAPSHOT RULE — binding on every Data Access Object reached through this interface.</b></para>
	///
	/// <para>
	/// An instance returned by <c>Get</c>, <c>GetAll</c> or <c>GetPaged</c> is a snapshot, and an instance
	/// handed to <c>Insert</c>, <c>Update</c> or <c>Delete</c> is read rather than adopted: mutating a returned
	/// instance — or a returned list — does not change stored data, and mutating an argument after the call
	/// returns does not reach the store. Stored data changes only through the write members, each of which
	/// reads its argument's values as they stand at the moment of the call. Fetching an entity, editing the
	/// fetched instance and then calling <c>Update</c> therefore leaves the store untouched until that
	/// <c>Update</c> runs, and two instances retrieved separately are independent of each other and of the
	/// store. Custom members are bound identically — a custom retrieval returns snapshots, a custom write reads
	/// its argument. Where a Data Access Object's own contract states a write-back onto the caller's instance,
	/// such as a generated identifier or a stamped timestamp, those are the only values that travel from the
	/// store back onto a caller's instance.
	/// </para>
	///
	/// <para>
	/// <b>A snapshot is deep.</b> An entity reached through a navigation property on a returned instance —
	/// <see cref="Entities.User.Company"/>, <see cref="Entities.User.Job"/>,
	/// <see cref="Entities.User.Department"/>, <see cref="Entities.Transaction.User"/>,
	/// <see cref="Entities.Transaction.Company"/> — is itself a snapshot, and one reached through a navigation
	/// property on an argument is likewise read rather than adopted. A copy one level deep would leave stored
	/// data reachable and mutable through a snapshot, which is precisely what the paragraph above denies. It
	/// follows that two separately retrieved entities naming the same stored row receive independent instances
	/// rather than a shared one — a retrieval materializes fresh objects, it does not hand out a shared identity
	/// map — so editing <c>userA.Company</c> changes neither <c>userB.Company</c> nor the store even where both
	/// name the same company. The graph is finite and acyclic: <see cref="Entities.Transaction"/> is the deepest
	/// at two levels, <see cref="Entities.User"/> at one, and every other entity carries scalars only.
	/// </para>
	///
	/// <para>
	/// <b>This now holds for every Data Access Object on this interface.</b> The rule was originally stated on
	/// two of them only — <see cref="IDepartmentDao"/> rule 19 and <see cref="ICompanyResourceDao"/> rule 9 —
	/// and material elsewhere in this repository records that the remaining five left it unsaid. That gap is
	/// closed. Those two numbered rules are per-Data-Access-Object restatements of this one, not exceptions to
	/// it, and <see cref="ICompanyDao"/>, <see cref="IJobDao"/>, <see cref="IUserDao"/>,
	/// <see cref="ITransactionDao"/> and <see cref="IResourceDao"/> are bound by it on the same terms.
	/// </para>
	///
	/// <para>
	/// <b>Why it matters.</b> A database hands back rows, not object references. An in-memory store that hands
	/// back the object it is holding gives a caller a way to change stored data that no database-backed
	/// implementation can reproduce, and this repository's claim that the same tests pass against either Data
	/// Access Layer would be quietly false. Stating the rule forces an in-memory implementation to copy on read
	/// and on write, so a dictionary-backed store and a database-backed one are genuinely interchangeable.
	/// </para>
	///
	/// <para>
	/// It is also what lets a transaction rollback actually reverse an <c>Update</c>. Where a Data Access
	/// Object hands back the instance it is storing, a caller who fetches a row, edits what came back and then
	/// calls <c>Update</c> has already changed the store before <c>Update</c> runs; the undo entry captures the
	/// already-edited state, and the rollback restores the edit instead of reversing it. Snapshots on read are
	/// what keep the pre-update state available to be restored.
	/// </para>
	///
	/// <para><b>ORDERING RULE — binding on every Data Access Object reached through this interface.</b></para>
	///
	/// <para>
	/// The order in which <c>GetAll</c> and <c>GetPaged</c> return entities is unspecified, but it is stable
	/// across calls for as long as the stored data is unchanged. Successive <c>GetPaged</c> windows therefore
	/// partition the full set with no overlap and no omission, and each window holds the same entities, in the
	/// same positions, as the stretch of a full pass it covers — a full pass being <c>GetAll</c>, or
	/// <c>GetPaged</c> at a <c>skip</c> of <c>0</c> taking <c>GetCount</c> records. Where a Data Access Object
	/// offers both, <c>GetAll</c> and <c>GetPaged</c> order identically. Where it offers <c>GetAll</c> alone the
	/// stability half still binds: two calls with nothing written between them return the same entities in the
	/// same order.
	/// </para>
	///
	/// <para>
	/// <b>This is the general form of <see cref="IDepartmentDao"/> rule 11</b>, which states it for that Data
	/// Access Object; nothing here replaces or contradicts it. <see cref="ICompanyResourceDao"/> rule 5, which
	/// promises no particular order from <see cref="ICompanyResourceDao.GetAll"/>, is the unspecified half of
	/// this rule rather than an exception to it. <see cref="ICompanyDao"/>, <see cref="ITransactionDao"/>,
	/// <see cref="IJobDao"/> and <see cref="IResourceDao"/> are bound on the same terms.
	/// </para>
	///
	/// <para>
	/// <b>Why it matters.</b> Without it two conforming implementations disagree, and the one that disagrees
	/// does so intermittently and only at scale. An in-memory store satisfies the rule incidentally, through
	/// the insertion order of the dictionary holding its rows. SQL Server guarantees no order at all without an
	/// explicit <c>ORDER BY</c>, and the plan it picks for an unordered scan can change as a table grows — so a
	/// SQL-backed Data Access Layer that omits <c>ORDER BY</c> passes every test today and starts failing them
	/// at some future row count, with nothing to point at. Satisfy it with an explicit <c>ORDER BY</c> on both
	/// <c>GetAll</c> and <c>GetPaged</c>, using the same ordering in each.
	/// </para>
	/// </remarks>
	public interface IExampleDataAccess : IBaseDataAccess, ICompanyDao, IJobDao, IUserDao, ITransactionDao, IResourceDao, IDepartmentDao, ICompanyResourceDao
	{
	}
}
