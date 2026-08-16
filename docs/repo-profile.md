# Repo Profile — ProphetsWay.Example

_Generated 2026-08-15, against the working tree at **v3.1.0**, immediately after the `Modernizer`
retarget pass and the `Purpose Refiner` pass. **Amended 2026-08-16** for the two `Scope` retraits and
a re-check of the README. Evidence-based; every claim cites a source file._

> **Supersedes the 2026-08-12 edition**, which profiled the `3.0.0-feature-update` branch at commit
> `122dc69` and cited CI build `3.0.0.486` on PR #19. That branch is merged and that evidence
> describes 3.0.0, not the current tree. Nothing from it is carried forward unverified.

**Verified current state:**

| Thing | Value | Source |
|---|---|---|
| `ProphetsWay.Example.DataAccess` | `netstandard2.0;net10.0` | [csproj](../ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L4) |
| `ProphetsWay.Example.DataAccess.NoDB` | `netstandard2.0;net10.0` | [csproj](../ProphetsWay.Example.DataAccess.NoDB/ProphetsWay.Example.DataAccess.NoDB.csproj#L4) |
| `ProphetsWay.Example.Tests` | `net48;net10.0` | [csproj](../ProphetsWay.Example.Tests/ProphetsWay.Example.Tests.csproj#L4) |
| `ProphetsWay.BaseDataAccess` reference | **3.1.0** (NuGet `PackageReference`) | [csproj](../ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L29) |
| Suite | 162 tests × 2 legs = **324 executions** | Static count, 2026-08-16 — see below |
| Version | 3.1.0, set by hand | [app-variables.yml](../app-variables.yml#L3) |

---

## ✅ Build & Test Verification

**The solution builds and the full suite passes on both legs.** The evidence is the local
`Modernizer` verification run of 2026-08-15, which built and tested `net48` and `net10.0`
independently and confirmed 160 tests on each — **320 executions total**.

**That run predates the tree.** Two test classes were retraited and split on 2026-08-16 —
[SnapshotDeepCopyTests.cs](../ProphetsWay.Example.Tests/SnapshotDeepCopyTests.cs) and
[UserDaoTests.cs](../ProphetsWay.Example.Tests/UserDaoTests.cs) — each keeping a `Contract` assertion
and moving an over-claiming one into a new `Characterization` sibling. The suite is therefore **162
tests, 324 executions** today. No green run has been recorded against that tree; the count below is a
static one and is labelled as such.

**This analyst pass did not itself run a build.** No claim below rests on a build I performed; where
the evidence is the `Modernizer` run it says so. There is **no CI build for 3.1.0 yet** — the last
recorded pipeline evidence is build `3.0.0.486` on PR #19, which validated the 3.0.0 tree and is
superseded.

### Statically re-verified in this pass

| Check | Method | Result |
|---|---|---|
| TFM lists | Read all three `.csproj` | Match the table above |
| BaseDataAccess version | Read the single `PackageReference` | `3.1.0` |
| `<NullableContextOptions>` | Repo-wide search | **Absent from every `.csproj`** — the property is gone |
| `Scope` trait partition | Static count of class- and method-level `[Trait("Scope", …)]` against `[Fact]` / `[Theory]` / `[InlineData]` | **138 Contract / 4 Characterization / 20 Dispatcher** — sums to 162, the suite total |
| Untagged tests | Cross-check of class-level vs. method-level traits, test method by test method | **None** — every test carries exactly one `Scope`. Four classes now declare it per method: `CompanyDaoTests`, `DataAccessTransactionTests`, `SnapshotDeepCopyTests`, `UserDaoTests` |
| Single construction site | Repo-wide search for `new ExampleDataAccess()` outside `ConventionShowcase/` | **One**, in [TestDataAccessFactory.Create](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs#L38) |

### The `net48` test leg is now doing something it was not doing before

The libraries no longer ship a `net48` asset, so the `net48` test leg resolves the DAL's
**`netstandard2.0`** output — i.e. **the exact assembly a .NET Framework consumer receives**. Before
the retarget it bound a `net48`-specific compilation that no consumer of `netstandard2.0` would ever
load. This is a *strengthening* of the arrangement, not a mismatch, and it is the reason the test
project keeps `net48` at all: `Activator.CreateInstance<T>()` wraps a throwing constructor on .NET
Framework and does not on .NET Core, which is what the `ConventionShowcase` exception-passthrough
guard exists to pin.

### Reproducing locally

```
dotnet build ProphetsWay.Example.sln -c Release
dotnet test  ProphetsWay.Example.sln -c Release
dotnet test  ProphetsWay.Example.sln --filter "Scope=Contract"
```

Expect 162 tests on each of `net48` and `net10.0` — **324 executed test cases total**. The solution
includes the `.sqlproj`, so `dotnet build` also exercises the SDK-style database project.

### Warnings

Two pre-existing `xUnit1013` warnings on
[DepartmentDaoTests.cs](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs), raised by the two public
static helpers that carry no test attribute —
[EditEveryFieldAfterTheCall](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs#L294) and
[AssertEveryStampIsUtc](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs#L1001). They predate the
retarget, which introduced no new warnings. Cosmetic; making them `internal` would silence both.

---

## One-Line Purpose

The reference implementation and executable specification for the `ProphetsWay.BaseDataAccess`
paradigm — a worked domain, one in-memory DAL, and a 162-test suite written against interfaces so
that pointing it at a different DAL is a one-line change.

## What It Actually Does

Four projects split along the contracts/implementation seam the paradigm exists to enforce:

- [ProphetsWay.Example.DataAccess](../ProphetsWay.Example.DataAccess) declares seven entities, seven
  `I*Dao` interfaces, and the aggregate
  [IExampleDataAccess](../ProphetsWay.Example.DataAccess/IExampleDataAccess.cs). It references
  `ProphetsWay.BaseDataAccess` 3.1.0 from NuGet and nothing else.
- [ProphetsWay.Example.DataAccess.NoDB](../ProphetsWay.Example.DataAccess.NoDB) implements all of it
  over a process-wide in-memory store.
- [ProphetsWay.Example.Tests](../ProphetsWay.Example.Tests) exercises the contracts. Exactly one
  method in the project — [TestDataAccessFactory.Create](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs#L38)
  — names a concrete implementation.
- [ProphetsWay.Example.Database](../ProphetsWay.Example.Database) is the SQL Server schema the
  contracts could be backed by.

The most substantive thing here is not code volume but **specification**. The `<remarks>` on
[IExampleDataAccess](../ProphetsWay.Example.DataAccess/IExampleDataAccess.cs#L10) carry two DAL-wide
rules — a **snapshot rule** (reads return deep copies, writes read rather than adopt their argument)
and an **ordering rule** (unspecified but stable order; paged windows partition a full pass). Both
are written with an explanation of *why an in-memory store satisfies them incidentally while SQL
Server does not*, which is precisely the class of divergence that would falsify the repo's central
claim.

**v3.1.0 changed no `.cs` file.** It is a retarget plus documentation release; the suite **it shipped**
is byte-identical to 3.0.0's, and that is deliberately what makes its passing the evidence for the
retarget. **The working tree has since moved on** — two test files were retraited and split on
2026-08-16, taking the suite from 160 tests to 162 — so the byte-identical claim describes v3.1.0 as
released and must not be restated about the current tree.

## Projects in the Solution

Source: [ProphetsWay.Example.sln](../ProphetsWay.Example.sln)

| Project | Type | Role |
|---|---|---|
| `ProphetsWay.Example.DataAccess` | Library | Contracts — `Entities/`, `IDaos/`, `Enums/`, `IExampleDataAccess` |
| `ProphetsWay.Example.DataAccess.NoDB` | Library | In-memory DAL implementation |
| `ProphetsWay.Example.Database` | `.sqlproj` | SQL Server schema + post-deploy seed scripts |
| `ProphetsWay.Example.Tests` | xUnit | 162 tests written against the interfaces |

No project references flow the wrong way: `.NoDB` → `.DataAccess`, `.Tests` → both. The contracts
project references no implementation.

## Public API Surface

### Entities — [ProphetsWay.Example.DataAccess/Entities](../ProphetsWay.Example.DataAccess/Entities)

| Entity | Marker implemented | Identifier | DAO interface | NoDB DAO |
|---|---|---|---|---|
| `Company` | `BaseIntEntity` | `int` | `ICompanyDao` | `CompanyDao` |
| `Job` | `BaseIntEntity` | `int` | `IJobDao` | `JobDao` |
| `User` | `BaseIntEntity` | `int` | `IUserDao` | `UserDao` |
| `Transaction` | `IBaseIdEntity<long>` | `long` | `ITransactionDao` | `TransactionDao` |
| `Resource` | `IBaseIdEntity<Guid>` | `Guid` | `IResourceDao` | `ResourceDao` |
| `Department` | `BaseIntEntity, IBaseSoftIdEntity<int>` | `int` | `IDepartmentDao` | `DepartmentDao` |
| `CompanyResource` | `IBaseEntity` | **none** | `ICompanyResourceDao` | `CompanyResourceDao` |

**The mapping is complete and symmetrical — 7 entities, 7 DAO interfaces, 7 NoDB DAOs, with no
orphan on any side.**

### DAO capability composition — [IDaos/](../ProphetsWay.Example.DataAccess/IDaos)

| Interface | Extends | Notes |
|---|---|---|
| `ICompanyDao` | `IBasePagedDao<Company>` | Plus custom `GetCustomCompanyFunction(int)` |
| `IJobDao` | `IBaseGetAllDao<Job>` | |
| `IUserDao` | `IBaseDao<User>` | Plus custom `CustomUserFunctionality(User)` |
| `ITransactionDao` | `IBasePagedDao<Transaction>` | |
| `IResourceDao` | `IBaseGetAllDao<Resource>` | |
| `IDepartmentDao` | `IBaseGetAllDao<Department>, IBasePagedDao<Department>` | Soft delete; custom `Restore`; **19 numbered rules** in XML docs |
| `ICompanyResourceDao` | *(nothing from BaseDataAccess)* | Declares only `Insert`/`Delete`/`GetAll`; **10 numbered rules** |

`ICompanyResourceDao` deliberately inherits no `IBaseDao<T>` — its documented purpose is to show the
DAO interfaces are a menu rather than a mandate
([ICompanyResourceDao.cs](../ProphetsWay.Example.DataAccess/IDaos/ICompanyResourceDao.cs#L58)).

### Implementation internals — [ProphetsWay.Example.DataAccess.NoDB](../ProphetsWay.Example.DataAccess.NoDB)

| Type | Kind | Role |
|---|---|---|
| `ExampleDataAccess` | public class | Extends `BaseDataAccess`, implements `IExampleDataAccess`; delegates to seven DAOs |
| `DataStore` | internal static | Process-wide store standing in for the database; `Interlocked`-based identity counters |
| `StoreTable<TKey,TEntity>` | internal | Keyed table; copies on read and on write |
| `StoreList<TEntity>` | internal | Keyless table for `CompanyResource` |
| `TransactionLog` | internal sealed | Per-instance undo log backing the transaction members |
| `BaseDao` + 7 DAOs | internal | The per-entity implementations |

## Coverage of the 3.x Contracts

### Demonstrated ✅

| Contract | Where it is implemented | Where it is tested |
|---|---|---|
| `IBaseDataAccess : IDisposable`; `Dispose` abstract | [ExampleDataAccess.Dispose](../ProphetsWay.Example.DataAccess.NoDB/ExampleDataAccess.cs#L74) | [DataAccessDisposalTests](../ProphetsWay.Example.Tests/DataAccessDisposalTests.cs) |
| `Dispose` is idempotent, never throws | `if (_disposed) return;` guard | [ShouldNotThrowWhenDisposedTwice](../ProphetsWay.Example.Tests/DataAccessDisposalTests.cs#L73) |
| Every member but `Dispose` throws `ObjectDisposedException` | `ThrowIfDisposed()` on **all 40 delegating members** | [ShouldThrowWhenAMemberIsCalledAfterDispose](../ProphetsWay.Example.Tests/DataAccessDisposalTests.cs#L89) |
| DAL disposes what it created, not the store | `Dispose` abandons the transaction and leaves `DataStore` alone | [ShouldNotDiscardStoredDataWhenAnotherDataAccessInstanceIsDisposed](../ProphetsWay.Example.Tests/DataAccessDisposalTests.cs#L53) |
| All three transaction members abstract → implemented | [TransactionStart/Commit/RollBack](../ProphetsWay.Example.DataAccess.NoDB/ExampleDataAccess.cs#L272) over `TransactionLog` | [DataAccessTransactionTests](../ProphetsWay.Example.Tests/DataAccessTransactionTests.cs) — 17 methods, 20 cases |
| No nesting; `InvalidOperationException` on misuse | [TransactionLog.Start / RequireOpen](../ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L46) | [DataAccessTransactionTests](../ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L79) |
| Scope is the instance, not the connection | `_transaction` is an instance field, deliberately not on `DataStore` | [ShouldNotEnrolAnotherInstancesWorkInThisInstancesTransaction](../ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L468), [ShouldTrackTransactionStateOnEachInstanceSeparately](../ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L491) |
| Open transaction rolled back on disposal | [TransactionLog.Abandon](../ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L88) | [ShouldRollBackAnOpenTransactionWhenDisposed](../ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L517) |
| Calls outside a transaction auto-commit | [TransactionLog.Record](../ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L110) discards when closed | Covered across the transaction suite |
| **Exceptions propagate unwrapped** (no `TargetInvocationException`) | n/a — base library behavior | [ExceptionPassthroughShowcaseTests](../ProphetsWay.Example.Tests/ConventionShowcase/ExceptionPassthroughShowcaseTests.cs) — 9 tests, with an explicit `ShouldNotBeOfType<TargetInvocationException>()` regression guard |
| **`Get<T>(null)` throws `ArgumentException`** on a non-nullable value-type identifier | n/a — base library behavior | [ShouldThrowWhenGenericGetIsGivenANullId](../ProphetsWay.Example.Tests/DepartmentDataAccessTests.cs#L156) |
| `ArgumentException` vs. `DataAccessConventionException` split | | [ShouldThrowWhenGenericGetIsGivenAnIdThatIsNotAnInt](../ProphetsWay.Example.Tests/DepartmentDataAccessTests.cs#L168) asserts `ArgumentException` *and specifically not* the convention exception |
| The reflection convention itself | n/a | [ConventionShowcaseTests](../ProphetsWay.Example.Tests/ConventionShowcase/ConventionShowcaseTests.cs) — 11 deliberately mis-wired DALs |
| `item` parameter is a type selector only | DAOs never read it | [ShouldGetGenericAllDepartmentsAndAgreeWithGenericCount](../ProphetsWay.Example.Tests/DepartmentDataAccessTests.cs#L112) notes the dispatcher passes `null` |

The disposal work is thorough in a way that is easy to under-credit: `ThrowIfDisposed()` appears on
**every** delegating member of `ExampleDataAccess`, not just a representative few, even though only
one of them is directly asserted.

### Not demonstrated — triaged, do not re-report as discoveries ❌

Earlier editions of this document restated these four gaps in full. **They now live in
[docs/feature-requests.md](feature-requests.md), where they carry a status and the reasoning behind
it.** The table below is an index only; the reasoning is deliberately not duplicated here, because
duplicated reasoning drifts.

| Gap | Entry | Status |
|---|---|---|
| `Get<T>(null)` being *accepted* where the identifier is a reference type or nullable value type | [FR 1](feature-requests.md#1--demonstrate-the-accepting-half-of-gettnull) | **Proposed** — the highest-value gap; to be closed in `ConventionShowcase/`, not the domain |
| Value-type (`struct`) entities and their inability to express "not found" as `null` | [FR 2](feature-requests.md#2--a-value-type-struct-entity) | Deferred — becomes cheap only if FR 1 lands |
| Bare `IBaseSoftEntity` — soft delete without an identifier | [FR 3](feature-requests.md#3--bare-ibasesoftentity--soft-delete-without-an-identifier) | **Rejected** — the shape is barely coherent; grid completeness is not an argument |
| Ambient `TransactionScope` being left untouched | [FR 4](feature-requests.md#4--demonstrating-that-ambient-transactionscope-is-left-untouched) | **Rejected here** — an in-memory store cannot fail the assertion, so the test could never be red |

## Consistency Check Across the Four Projects

### Contracts project leakage — **clean** ✅

Every `using` in [ProphetsWay.Example.DataAccess](../ProphetsWay.Example.DataAccess) resolves to
`System`, `ProphetsWay.BaseDataAccess`, or a sibling namespace within the same project. There is
**no** `DbContext`, `SqlConnection`, `HttpContext`, or reference to `NoDB` anywhere in it, and its
only `PackageReference` is `ProphetsWay.BaseDataAccess` 3.1.0
([csproj](../ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L29)). The seam
holds.

### Half-finished work — **none found** ✅

- No `TODO`, `HACK`, `FIXME`, or `XXX` in any source file.
- No `NotImplementedException` anywhere in shipping code. The three `NotSupportedException` throws
  are in [ShowcaseDataAccess](../ProphetsWay.Example.Tests/ConventionShowcase/ShowcaseDataAccess.cs#L42)
  and are intentional — those DALs exist to be mis-wired.
- No commented-out code blocks.
- No stubbed or empty members. `ExampleDataAccess` delegates all 40 members with a disposal guard on
  each; no member returns a placeholder.

### Test-suite discipline — **strong** ✅

`TestDataAccessFactory` is genuinely the only construction site
([TestDataAccessFactory.cs#L38](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs#L38)), and
`CreateAs<T>` performs a *checked* cast that names the missing interface rather than throwing a bare
`InvalidCastException` from a base constructor. The `ConventionShowcase` DALs deliberately bypass
the factory, and that decision is documented in the factory's own remarks.

## Dependencies

| Project | Reference | Version |
|---|---|---|
| `.DataAccess` | `ProphetsWay.BaseDataAccess` | **3.1.0** (NuGet) |
| `.DataAccess.NoDB` | → `.DataAccess` | project reference |
| `.Tests` | `Microsoft.NET.Test.Sdk` | 17.13.0 |
| `.Tests` | `xunit` | 2.9.3 |
| `.Tests` | `xunit.runner.visualstudio` | 3.0.2 |
| `.Tests` | `Shouldly` | 4.3.0 |
| `.Tests` | `coverlet.collector` | 6.0.4 |
| `.Database` | `Microsoft.Build.Sql` SDK | 2.2.0 |

Fully conformant with house convention: xUnit + Shouldly + coverlet, no FluentAssertions, no Moq.

**The `ProphetsWay.BaseDataAccess` reference is a NuGet `PackageReference`, not a project
reference**, so `3.1.0` must be on the feed for a clean-machine restore. It resolved in the
`Modernizer` run against a stable `3.1.0` (assembly file version `3.1.0.495`), so the package is
published, not a local artifact. This closes the open question the previous edition carried about
`3.0.0`'s availability.

## Target Frameworks

| Project | TFMs |
|---|---|
| `.DataAccess` | `netstandard2.0;net10.0` |
| `.DataAccess.NoDB` | `netstandard2.0;net10.0` |
| `.Tests` | `net48;net10.0` |

**This is exactly the current house standard**, in canonical dotted form: `netstandard2.0` as the
permanent reach floor plus exactly one modern LTS, no end-of-life target, no redundant target, and
no `net48` on the libraries because `netstandard2.0` already reaches .NET Framework 4.8 and neither
DAL project has a framework-conditional dependency. The test project names runnable targets only and
keeps `net48` to *verify* .NET Framework behavior — see the note above on what that leg now binds.

`ProphetsWay.BaseDataAccess` is at the same standard as of its 3.1.0. This repo is therefore **a**
reference for the TFM convention rather than **the** reference; either can be copied.

No `LangVersion` pin remains in any project. Note that `netstandard2.0` caps shared code at C# 7.3,
which is why nullable reference types cannot work here regardless of what a csproj declares — the
reason `Purpose Refiner` explicitly does *not* recommend `<Nullable>enable</Nullable>`.

**Nothing to recommend. The inert `<NullableContextOptions>enable</NullableContextOptions>` property
that previous editions of this document flagged has been removed** from
`ProphetsWay.Example.DataAccess.csproj`; a repo-wide search finds the string in no project file. That
observation is closed and should not be re-reported.

## Packaging Audit

**PACKAGING: informational — not currently published.**

Publication intent is explicitly negative: [app-variables.yml](../app-variables.yml) has both
`PostTargetToNuGet` and `TargetProject` **commented out**, and every packaging element in both DAL
`.csproj` files is an empty self-closing stub (`<PackageId />`, `<RepositoryUrl />`,
`<PackageTags />`, …). The test project sets `<IsPackable>false</IsPackable>` and now also
`<IsTestProject>true</IsTestProject>`.

This is **correct and intentional** — the repo is a teaching artifact, not a package. The empty stubs
are inherited boilerplate rather than a defect. No action required, and no snippet is proposed.
`Purpose Refiner` reached the same conclusion independently and recorded
`docs/nuget-extraction-proposal.md` as `n/a` rather than missing
([purpose-and-scope.md](purpose-and-scope.md#publication-and-the-nuget-extraction-proposal)).

`HasSqlProj: 'yes'` in [app-variables.yml](../app-variables.yml#L8) means the shared template also
builds the `.sqlproj`, which the SDK-style migration made CLI-buildable. That combination was first
proven by CI build `3.0.0.486`; **it has not been re-run in CI since the retarget**, though nothing
in the retarget touched the database project.

## Real Usage Examples Found

The best snippet in the repo is the swap point itself:

```csharp
public static IExampleDataAccess Create()
{
	//>>> The one line to change to point this suite at another implementation. <<<
	return new ExampleDataAccess();
}
```

And the disposal implementation, which is the clearest short statement of the 3.x contract anywhere
in the workspace — idempotent, non-throwing, rolls back rather than commits, and pointedly does
*not* clear the store
([ExampleDataAccess.cs#L74](../ProphetsWay.Example.DataAccess.NoDB/ExampleDataAccess.cs#L74)).

## README Accuracy Check

`README.md` is owned by `README Author`. **The four rows this table carried as STALE have since been
fixed in the README and are re-verified as accurate below** — the file now states `netstandard2.0` /
`net10.0`, two test legs, a `3.1.0` reference, and the EFTools claim qualified by the pinned submodule
pointer. The count row was corrected on 2026-08-16 by the same pass that corrected this document.

| Existing claim | Verdict | Evidence |
|---|---|---|
| **"138 Contract / 2 Characterization / 20 Dispatcher"** ([README.md#L137](../README.md#L137)) | **WAS STALE — corrected 2026-08-16** | Accurate when written. The two retraits make it **138 / 4 / 20 of 162**; the README table and its "all but two" list were updated in the same pass |
| "`TestDataAccessFactory.Create` is the only place `new ExampleDataAccess()` appears" | **Accurate** | Single match repo-wide |
| "`IDepartmentDao` carries 19 numbered rules; `ICompanyResourceDao` carries 10" | **Accurate** | Both files carry the numbered rule sets |
| "`TransactionStart` throws `InvalidOperationException`…" | **Accurate** | [TransactionLog.cs#L50](../ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L50) |
| "Every other member throws `ObjectDisposedException` once disposed" | **Accurate** | `ThrowIfDisposed()` on every delegating member |
| "`ProphetsWay.BaseDataAccess` 3.0.0 removed the `TargetInvocationException` wrapper" | **Accurate** | Asserted directly in `ExceptionPassthroughShowcaseTests`; a statement about 3.0.0's history, still true at 3.1.0 |
| Test-suite size and legs | **WAS STALE — corrected** | The README now says two legs, `net48` and `net10.0`, and was updated to **162 tests / 324 executions** on 2026-08-16 |
| Data access layer and test project target frameworks | **Accurate** | README now states `netstandard2.0` / `net10.0` for the two DAL projects and `net48` / `net10.0` for the tests — matches all three `.csproj` |
| `ProphetsWay.BaseDataAccess` reference version | **Accurate** | README now says **3.1.0**; matches the single `PackageReference` |
| **"`ProphetsWay.EFTools` carries an Entity Framework implementation of the very same `IExampleDataAccess` contract, and the tests do not change to accommodate it."** | **Now qualified — accurate as written** | The README states the pointer is pinned at `967fd26`, the 3.0.0 branch point, and that the EF implementation is on the pre-3.0.0 contract. The underlying coordination gap is unchanged: [FR 5](feature-requests.md#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) |
| Badge points at `branchName=main` | **Correct** | 3.0.0 is merged; the badge now reflects this tree |

`CHANGELOG.md` has **no v3.1.0 entry** — its most recent heading is `v3.0.0`. That file belongs to
`Changelog Author` and is a later phase of this same documentation pass; it is recorded here as a
known outstanding item, not as drift.

## Gaps & Observations

Ordered by consequence. Anything with an `FR n` reference is triaged in
[docs/feature-requests.md](feature-requests.md) and should not be re-litigated here.

1. **The repo's headline claim is currently false, and today made it more so.** `ProphetsWay.EFTools`
   pins this repo as a submodule at `967fd26`, **pre-3.0.0**; after the 3.1.0 retarget it is further
   behind still, and its `ProphetsWay.Example.DataAccess.EF` implements a contract that no longer
   exists. **This is a coordination requirement, not duplication** — a submodule cannot drift, only
   lag. Never edit files under `ProphetsWay.EFTools/ProphetsWay.Example/`; edit here and advance the
   pointer. [FR 5](feature-requests.md#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts).

2. **Half of the `Get<T>(null)` split is taught as the whole of it.**
   [FR 1](feature-requests.md#1--demonstrate-the-accepting-half-of-gettnull) — and the decision that
   it be closed in `ConventionShowcase/` rather than by an eighth domain entity is the substantive
   part of that entry.

3. **Visual Studio cannot load the SDK-style `.sqlproj`,** and a newcomer meets that failure in the
   first minute of a repo designed to be opened. The CHANGELOG documents it; the README is where it
   belongs. [FR 7](feature-requests.md#7--a-visual-studio-onboarding-note-in-the-readme).

4. **Two `xUnit1013` warnings** on `DepartmentDaoTests.cs`, from the two public static helpers.
   Pre-existing, unrelated to the retarget, and silenced by making both `internal`.

5. **Residual `obj/` output from dropped TFMs.** `ProphetsWay.Example.DataAccess/obj/Debug/` still
   holds `net8.0/` and `net9.0/` alongside the current `net10.0/` and `netstandard2.0/`. Gitignored
   and cosmetic; a `dotnet clean` clears it.

Closed since the previous edition, recorded so they are not rediscovered as findings:

- The committed publish profile —
  [ProphetsWay.Example.localhost.publish.xml](../ProphetsWay.Example.Database/ProphetsWay.Example.localhost.publish.xml)
  now reads `Data Source=localhost`. It remains tracked, committed, and referenced as a
  `<None Include>` item from the
  [.sqlproj](../ProphetsWay.Example.Database/ProphetsWay.Example.Database.sqlproj#L29) — all three
  deliberate, since a teaching repo benefits from shipping a working publish profile. With the
  hostname genericized there is no machine-specific value and, as before, **no credentials**
  (Integrated Security, `Persist Security Info=False`). Nothing left to act on.
  **Removing the file does not break the build; that was verified empirically** — retained as a
  build fact, not as a reason to remove it.
  [FR 6](feature-requests.md#6--the-committed-developer-specific-publish-profile) records the
  decision.
- `<NullableContextOptions>` — **removed** from the csproj by the `Modernizer` pass.
- The stale `.dbmdl` sidecar — **gone from disk**; a repo-wide search finds no `.dbmdl`.
- The `netstandard2.0` leg going unverified locally — the `net48` test leg now binds the
  `netstandard2.0` asset directly, so it is exercised on every run.
- Whether `ProphetsWay.BaseDataAccess` is actually on the feed — `3.1.0` restored in the
  `Modernizer` run.

## Open Questions for the Owner

1. **What is the plan and timing for advancing the EFTools submodule pointer?** It is the only open
   item that changes whether this repo's central claim is true, and nothing in *this* repo can fix
   it.

2. **Is a CI run wanted on 3.1.0 before the remaining documentation phases finish?** The retarget is
   verified locally on both legs, but the shared pipeline has not built the tree since `3.0.0.486`,
   and `HasSqlProj: 'yes'` means CI covers the `.sqlproj` that a local `dotnet test` largely does
   not.

3. **Is the stray `[submodule "Submod"]` block in EFTools' `.gitmodules`** — `branch = main` with no
   `path` and no `url` — deliberate or abandoned? It belongs to EFTools and is noted here only
   because it was found while verifying the submodule relationship.
