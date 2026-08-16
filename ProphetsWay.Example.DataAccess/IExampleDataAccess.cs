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
	///
	/// <para>
	/// <b>IDENTIFIER RULE — binding on every Data Access Object reached through this interface whose entity
	/// carries an identifier.</b>
	/// </para>
	///
	/// <para>
	/// <c>Insert</c> assigns the identifier of the row it stored onto the instance the caller passed in, before
	/// it returns. When the call returns, that instance carries an identifier that is not the default value of
	/// its type, and handing an instance carrying that identifier to <c>Get</c> retrieves the row just stored.
	/// A caller therefore reads the identifier off its own instance <i>after</i> the call. This is the
	/// write-back the SNAPSHOT RULE's closing sentence anticipates — a value travelling from the store back
	/// onto a caller's instance, permitted precisely because a Data Access Object's contract states it — and it
	/// is the only value <c>Insert</c> writes back unless a Data Access Object states another, as
	/// <see cref="IDepartmentDao"/> rule 1 does for its timestamps.
	/// </para>
	///
	/// <para>
	/// <b>Where the identifier comes from is the Data Access Layer's business, not the caller's.</b>
	/// <see cref="Entities.Company"/>, <see cref="Entities.Job"/>, <see cref="Entities.User"/> and
	/// <see cref="Entities.Department"/> carry an <see cref="int"/>, <see cref="Entities.Transaction"/> a
	/// <see cref="long"/>, and <see cref="Entities.Resource"/> a <see cref="System.Guid"/>. The rule reads the
	/// same for all six. A <see cref="System.Guid"/> is computed by the Data Access Layer rather than handed
	/// out by a database engine, which changes who produces the value but nothing about what the caller is
	/// promised: after <c>Insert</c> the instance carries the identifier of the stored row either way.
	/// </para>
	///
	/// <para>
	/// <b>What becomes of an identifier the caller pre-assigned is deliberately unspecified</b>, and is the one
	/// place two conforming implementations of this interface may legitimately differ. An entity keyed by a
	/// database identity column has no way to honor a supplied value, so the generated one replaces it; a
	/// client-generated <see cref="System.Guid"/> may reasonably be used as supplied. Existing implementations
	/// do both, so a caller must depend on neither: pass an entity with its identifier left at its default —
	/// that is the case this rule speaks to. <see cref="IDepartmentDao"/> rule 1 pins the answer for
	/// <see cref="Entities.Department"/>, where a pre-assigned value is overwritten. A Data Access Object
	/// narrowing an unspecified point for its own entity is doing what rule 1 does, not making an exception.
	/// </para>
	///
	/// <para>
	/// <b><see cref="ICompanyResourceDao"/> is outside this rule</b>, which is why it is worded as binding only
	/// where an entity carries an identifier. <see cref="Entities.CompanyResource"/> carries none — its
	/// identity is the pair named by <see cref="ICompanyResourceDao"/> rule 1 — so there is nothing for
	/// <c>Insert</c> to assign, and rule 2 there says exactly that. It is the absence of an identifier, not a
	/// refusal of this rule.
	/// </para>
	///
	/// <para><b>ROW COUNT RULE — binding on every Data Access Object reached through this interface.</b></para>
	///
	/// <para>
	/// <c>Update</c> and <c>Delete</c> return <c>1</c> when the argument identified a row the operation applied
	/// to, and <c>0</c> when it identified none. Never a negative number, and never more than <c>1</c> — a
	/// write reached through this interface addresses a single row. A return of <c>0</c> means nothing was
	/// changed and throws nothing, so a write against a row that is not there is an ordinary result rather than
	/// an error.
	/// </para>
	///
	/// <para>
	/// <b><c>Update</c> reports that a row matched, not that a value changed.</b> Updating a row with values
	/// identical to the ones already stored returns <c>1</c>, not <c>0</c>. This is the clause most easily
	/// lost, and losing it is not cosmetic: an implementation that returns whatever its storage layer reports
	/// as rows-<i>modified</i> returns <c>0</c> here, while the same implementation over a layer reporting
	/// rows-<i>matched</i> returns <c>1</c> — two conforming Data Access Layers disagreeing, which is the exact
	/// failure this repository exists to disprove. It follows that <c>Update</c> is idempotent in effect: a
	/// second identical call rewrites the same values and returns <c>1</c> again.
	/// </para>
	///
	/// <para>
	/// <b>A Data Access Object's own contract governs what counts as a row the operation applied to.</b> Where
	/// <c>Delete</c> is a hard delete the row is gone afterwards, so a second <c>Delete</c> of the same entity
	/// returns <c>0</c>. <see cref="IDepartmentDao"/> redefines <c>Delete</c> as a soft delete, and its rules 5
	/// and 6 state the counts that follow: a live department returns <c>1</c>, one already deleted returns
	/// <c>0</c> even though its row is still stored. That is this rule applied to a narrower notion of a match,
	/// not an exception to it, and rules 2 and 4 there read the same way for <c>Update</c>.
	/// <see cref="ICompanyResourceDao"/> declares no <c>Update</c> at all; its <c>Delete</c> matches on the pair
	/// rather than on an identifier, and its rule 4 states the identical counts.
	/// </para>
	///
	/// <para>
	/// <b>Both rules are conventions elected here, not changes to the base package.</b>
	/// <see cref="IBaseDao{T}"/> documents the identifier write-back and then says plainly that it is "a
	/// convention left to the implementation — this library neither performs it nor verifies that it happened",
	/// and describes <c>Update</c> and <c>Delete</c> as returning "typically 1". Both statements are correct
	/// and unchanged, and they are the right statements for a library that cannot know what its implementations
	/// store. What the two rules above do is commit <i>this</i> Data Access Layer to the convention for its own
	/// implementations — which is what <see cref="IDepartmentDao"/> rules 1, 2 and 5–7 and
	/// <see cref="ICompanyResourceDao"/> rules 2 and 4 already did for two of the seven. A reader should not
	/// conclude that <c>ProphetsWay.BaseDataAccess</c> now promises either.
	/// </para>
	///
	/// <para>
	/// <b>Both now hold for every Data Access Object on this interface.</b> They were originally stated on
	/// those two only, while <see cref="ICompanyDao"/>, <see cref="IJobDao"/>, <see cref="IUserDao"/>,
	/// <see cref="ITransactionDao"/> and <see cref="IResourceDao"/> left them unsaid and the test suite
	/// asserted them anyway. That gap is closed on the same terms the SNAPSHOT RULE closed its own, and those
	/// two Data Access Objects' numbered rules are per-Data-Access-Object restatements rather than exceptions.
	/// </para>
	/// </remarks>
	public interface IExampleDataAccess : IBaseDataAccess, ICompanyDao, IJobDao, IUserDao, ITransactionDao, IResourceDao, IDepartmentDao, ICompanyResourceDao
	{
	}
}
