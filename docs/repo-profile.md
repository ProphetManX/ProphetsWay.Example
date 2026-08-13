# Repo Profile — ProphetsWay.Example

_Generated 2026-08-12, against branch `3.0.0-feature-update` at commit `122dc69` (7 commits ahead of
`main`). Evidence-based; every claim cites a source file._

> **Scope note.** This profile analyzes the repository as it stands on the `3.0.0-feature-update`
> branch, immediately prior to merge.

---

## ✅ Build & Test Verification — PASSED IN CI

**The solution builds and the full suite passes.** Verified by Azure DevOps build **`3.0.0.486`**
(build id `486`) on **PR #19 — "3.0.0 feature update", `3.0.0-feature-update` → `main`**:

| Status check | Result |
|---|---|
| `ProphetManX.ProphetsWay.Example` | **success** — "Build #3.0.0.486 succeeded" |
| `ProphetManX.ProphetsWay.Example (PR Build and Run Unit Tests)` | **success** — "PR Build and Run Unit Tests succeeded" |

**This was a CI run, not a local one.** No `dotnet build` / `dotnet test` was executed in the
session that produced this profile — no terminal tool was available — so the evidence is the PR
status checks rather than a developer machine. The test project targets `net48;net8.0;net9.0`, so
the passing run covers all three test frameworks. PR #19 is OPEN and requires 0 approvals; it
currently has 0 approvals and no changes requested.

### What was verified statically

| Check | Method | Result |
|---|---|---|
| Compile errors / analyzer diagnostics | Language server across all three C# projects | **No errors reported** |
| Test count | Static count of `[Fact]` + `[InlineData]` attributes | **160 test cases** — 145 `[Fact]` + 6 `[Theory]` yielding 15 `[InlineData]` cases |
| `Scope` trait partition | Static count of `[Trait("Scope", …)]` | **138 Contract / 2 Characterization / 20 Dispatcher** |
| Untagged tests | Cross-check of class-level vs. method-level traits | **None** — every test carries a `Scope` |

The trait partition **exactly matches** the counts claimed in [CHANGELOG.md](CHANGELOG.md) and the
table in [README.md](README.md#L118). The static count confirms the tests *exist and are tagged*;
the CI run above is what confirms they pass.

The language server reporting no errors covered only the TFM the OmniSharp/Roslyn workspace loaded
(normally the first in the list). Multi-TFM compilation — `netstandard2.0` and `net48` included —
is covered by the CI build, not by that diagnostic.

### Reproducing locally

```
dotnet build ProphetsWay.Example.sln -c Release
dotnet test  ProphetsWay.Example.sln -c Release
dotnet test  ProphetsWay.Example.sln --filter "Scope=Contract"
```

Expect 160 tests across each of `net48`, `net8.0`, `net9.0` — **480 executed test cases total**.
Note the solution includes the `.sqlproj`, so `dotnet build` also exercises the SDK-style database
project migration claimed in the changelog.

---

## One-Line Purpose

The reference implementation and executable specification for the `ProphetsWay.BaseDataAccess`
paradigm — a worked domain, one in-memory DAL, and a 160-test suite written against interfaces so
that pointing it at a different DAL is a one-line change.

## What It Actually Does

Four projects split along the contracts/implementation seam the paradigm exists to enforce:

- [ProphetsWay.Example.DataAccess](ProphetsWay.Example.DataAccess) declares seven entities, seven
  `I*Dao` interfaces, and the aggregate [IExampleDataAccess](ProphetsWay.Example.DataAccess/IExampleDataAccess.cs).
  It references `ProphetsWay.BaseDataAccess` 3.0.0 from NuGet and nothing else.
- [ProphetsWay.Example.DataAccess.NoDB](ProphetsWay.Example.DataAccess.NoDB) implements all of it
  over a process-wide in-memory store.
- [ProphetsWay.Example.Tests](ProphetsWay.Example.Tests) exercises the contracts. Exactly one method
  in the project — [TestDataAccessFactory.Create](ProphetsWay.Example.Tests/TestDataAccessFactory.cs#L38)
  — names a concrete implementation.
- [ProphetsWay.Example.Database](ProphetsWay.Example.Database) is the SQL Server schema the contracts
  could be backed by.

The most substantive thing on this branch is not code volume but **specification**. The
`<remarks>` on [IExampleDataAccess](ProphetsWay.Example.DataAccess/IExampleDataAccess.cs#L10) now
carry two DAL-wide rules — a **snapshot rule** (reads return deep copies, writes read rather than
adopt their argument) and an **ordering rule** (unspecified but stable order; paged windows
partition a full pass). Both are written with an explanation of *why an in-memory store satisfies
them incidentally while SQL Server does not*, which is precisely the class of divergence that would
falsify the repo's central claim.

## Projects in the Solution

Source: [ProphetsWay.Example.sln](ProphetsWay.Example.sln)

| Project | Type | Role |
|---|---|---|
| `ProphetsWay.Example.DataAccess` | Library | Contracts — `Entities/`, `IDaos/`, `Enums/`, `IExampleDataAccess` |
| `ProphetsWay.Example.DataAccess.NoDB` | Library | In-memory DAL implementation |
| `ProphetsWay.Example.Database` | `.sqlproj` | SQL Server schema + post-deploy seed scripts |
| `ProphetsWay.Example.Tests` | xUnit | 160 tests written against the interfaces |

No project references flow the wrong way: `.NoDB` → `.DataAccess`, `.Tests` → both. The contracts
project references no implementation.

## Public API Surface

### Entities — [ProphetsWay.Example.DataAccess/Entities](ProphetsWay.Example.DataAccess/Entities)

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
orphan on any side.** `Department` and `CompanyResource` are new on this branch.

### DAO capability composition — [IDaos/](ProphetsWay.Example.DataAccess/IDaos)

| Interface | Extends | Notes |
|---|---|---|
| `ICompanyDao` | `IBasePagedDao<Company>` | Plus custom `GetCustomCompanyFunction(int)` |
| `IJobDao` | `IBaseGetAllDao<Job>` | |
| `IUserDao` | `IBaseDao<User>` | Plus custom `CustomUserFunctionality(User)` |
| `ITransactionDao` | `IBasePagedDao<Transaction>` | |
| `IResourceDao` | `IBaseGetAllDao<Resource>` | |
| `IDepartmentDao` | `IBaseGetAllDao<Department>, IBasePagedDao<Department>` | Soft delete; custom `Restore`; **19 numbered rules** in XML docs |
| `ICompanyResourceDao` | *(nothing from BaseDataAccess)* | Declares only `Insert`/`Delete`/`GetAll`; **10 numbered rules** |

`ICompanyResourceDao` deliberately inherits no `IBaseDao<T>` — its documented purpose is to show
the DAO interfaces are a menu rather than a mandate
([ICompanyResourceDao.cs](ProphetsWay.Example.DataAccess/IDaos/ICompanyResourceDao.cs#L58)).

### Implementation internals — [ProphetsWay.Example.DataAccess.NoDB](ProphetsWay.Example.DataAccess.NoDB)

| Type | Kind | Role |
|---|---|---|
| `ExampleDataAccess` | public class | Extends `BaseDataAccess`, implements `IExampleDataAccess`; delegates to seven DAOs |
| `DataStore` | internal static | Process-wide store standing in for the database; `Interlocked`-based identity counters |
| `StoreTable<TKey,TEntity>` | internal | Keyed table; copies on read and on write |
| `StoreList<TEntity>` | internal | Keyless table for `CompanyResource` |
| `TransactionLog` | internal sealed | Per-instance undo log backing the transaction members |
| `BaseDao` + 7 DAOs | internal | The per-entity implementations |

## Does the Example Exercise the 3.0.0 Features?

### Demonstrated ✅

| 3.0.0 change | Where it is implemented | Where it is tested |
|---|---|---|
| `IBaseDataAccess : IDisposable`; `Dispose` abstract | [ExampleDataAccess.Dispose](ProphetsWay.Example.DataAccess.NoDB/ExampleDataAccess.cs#L74) | [DataAccessDisposalTests](ProphetsWay.Example.Tests/DataAccessDisposalTests.cs) |
| `Dispose` is idempotent, never throws | `if (_disposed) return;` guard | [ShouldNotThrowWhenDisposedTwice](ProphetsWay.Example.Tests/DataAccessDisposalTests.cs#L73) |
| Every member but `Dispose` throws `ObjectDisposedException` | `ThrowIfDisposed()` on **all 40 delegating members** | [ShouldThrowWhenAMemberIsCalledAfterDispose](ProphetsWay.Example.Tests/DataAccessDisposalTests.cs#L89) |
| DAL disposes what it created, not the store | `Dispose` abandons the transaction and leaves `DataStore` alone | [ShouldNotDiscardStoredDataWhenAnotherDataAccessInstanceIsDisposed](ProphetsWay.Example.Tests/DataAccessDisposalTests.cs#L53) |
| All three transaction members abstract → implemented | [TransactionStart/Commit/RollBack](ProphetsWay.Example.DataAccess.NoDB/ExampleDataAccess.cs#L272) over `TransactionLog` | [DataAccessTransactionTests](ProphetsWay.Example.Tests/DataAccessTransactionTests.cs) — 17 methods, 20 cases |
| No nesting; `InvalidOperationException` on misuse | [TransactionLog.Start / RequireOpen](ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L46) | [DataAccessTransactionTests](ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L79) |
| Scope is the instance, not the connection | `_transaction` is an instance field, deliberately not on `DataStore` | [ShouldNotEnrolAnotherInstancesWorkInThisInstancesTransaction](ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L468), [ShouldTrackTransactionStateOnEachInstanceSeparately](ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L491) |
| Open transaction rolled back on disposal | [TransactionLog.Abandon](ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L88) | [ShouldRollBackAnOpenTransactionWhenDisposed](ProphetsWay.Example.Tests/DataAccessTransactionTests.cs#L517) |
| Calls outside a transaction auto-commit | [TransactionLog.Record](ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L110) discards when closed | Covered across the transaction suite |
| **Exceptions propagate unwrapped** (no `TargetInvocationException`) | n/a — base library behavior | [ExceptionPassthroughShowcaseTests](ProphetsWay.Example.Tests/ConventionShowcase/ExceptionPassthroughShowcaseTests.cs) — 9 tests, with an explicit `ShouldNotBeOfType<TargetInvocationException>()` regression guard |
| **`Get<T>(null)` throws `ArgumentException`** on a non-nullable value-type identifier | n/a — base library behavior | [ShouldThrowWhenGenericGetIsGivenANullId](ProphetsWay.Example.Tests/DepartmentDataAccessTests.cs#L156) |
| `ArgumentException` vs. `DataAccessConventionException` split | | [ShouldThrowWhenGenericGetIsGivenAnIdThatIsNotAnInt](ProphetsWay.Example.Tests/DepartmentDataAccessTests.cs#L168) asserts `ArgumentException` *and specifically not* the convention exception |
| The reflection convention itself | n/a | [ConventionShowcaseTests](ProphetsWay.Example.Tests/ConventionShowcase/ConventionShowcaseTests.cs) — 11 deliberately mis-wired DALs |
| `item` parameter is a type selector only | DAOs never read it | [ShouldGetGenericAllDepartmentsAndAgreeWithGenericCount](ProphetsWay.Example.Tests/DepartmentDataAccessTests.cs#L112) notes the dispatcher passes `null` |

The disposal work is thorough in a way that is easy to under-credit: `ThrowIfDisposed()` appears on
**every** delegating member of `ExampleDataAccess`, not just a representative few, even though only
one of them is directly asserted.

### NOT demonstrated ❌

These are the 3.0.0 behaviors the example is silent on. All four are gaps in *coverage of the
paradigm*, not defects in the code that exists.

| # | 3.0.0 behavior | Why the example cannot show it | Severity |
|---|---|---|---|
| 1 | **`Get<T>(null)` is *accepted* where the identifier is a reference type (`string`) or nullable value type (`int?`)** | Every entity here keys on `int`, `long`, or `Guid` — all non-nullable value types. Only the *throwing* half of the split is demonstrated; a reader could reasonably conclude `Get<T>(null)` always throws. | **Highest-value gap.** The AGENTS.md contract calls this split "deliberate"; the example teaches half of it. |
| 2 | **Value-type entities are supported by `Get<T>` but cannot express "not found" as `null`** | No `struct` entity exists — every entity is a `class`. | Medium. An unusual shape, but the one with the sharpest footgun. |
| 3 | **Bare `IBaseSoftEntity`** — soft delete on an entity with no identifier | `Department` implements `IBaseSoftIdEntity<int>`; `CompanyResource` implements bare `IBaseEntity`. The fourth corner of the 2×2 (soft × keyless) is empty. | Low. Arguably a shape nobody needs. |
| 4 | **Ambient `TransactionScope` is untouched by the DAL's transaction members** | No reference to `System.Transactions` anywhere in the repo. | Low. Hard to demonstrate meaningfully against an in-memory store; worth a documented note rather than a test. |

Gap #1 is the one worth acting on. A single additional entity keyed on `string` — or simply an
added test proving `Get<Resource>` behaves differently from a hypothetical string-keyed entity —
would close it. Everything else on this list is defensible as out of scope.

## Consistency Check Across the Four Projects

### Contracts project leakage — **clean** ✅

Every `using` in [ProphetsWay.Example.DataAccess](ProphetsWay.Example.DataAccess) resolves to
`System`, `ProphetsWay.BaseDataAccess`, or a sibling namespace within the same project. There is
**no** `DbContext`, `SqlConnection`, `HttpContext`, or reference to `NoDB` anywhere in it, and its
only `PackageReference` is `ProphetsWay.BaseDataAccess` 3.0.0
([csproj](ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L31)). The seam holds.

### Half-finished work — **none found** ✅

- No `TODO`, `HACK`, `FIXME`, or `XXX` in any source file.
- No `NotImplementedException` anywhere in shipping code. The three `NotSupportedException` throws
  are in [ShowcaseDataAccess](ProphetsWay.Example.Tests/ConventionShowcase/ShowcaseDataAccess.cs#L42)
  and are intentional — those DALs exist to be mis-wired.
- No commented-out code blocks.
- No stubbed or empty members. `ExampleDataAccess` delegates all 40 members with a disposal guard on
  each; no member returns a placeholder.

### Test-suite discipline — **strong** ✅

`TestDataAccessFactory` is genuinely the only construction site
([TestDataAccessFactory.cs#L38](ProphetsWay.Example.Tests/TestDataAccessFactory.cs#L38)), and
`CreateAs<T>` performs a *checked* cast that names the missing interface rather than throwing a bare
`InvalidCastException` from a base constructor. The `ConventionShowcase` DALs deliberately bypass
the factory, and that decision is documented in the factory's own remarks.

## Dependencies

| Project | Reference | Version |
|---|---|---|
| `.DataAccess` | `ProphetsWay.BaseDataAccess` | **3.0.0** (NuGet) |
| `.DataAccess.NoDB` | → `.DataAccess` | project reference |
| `.Tests` | `Microsoft.NET.Test.Sdk` | 17.13.0 |
| `.Tests` | `xunit` | 2.9.3 |
| `.Tests` | `xunit.runner.visualstudio` | 3.0.2 |
| `.Tests` | `Shouldly` | 4.3.0 |
| `.Tests` | `coverlet.collector` | 6.0.4 |
| `.Database` | `Microsoft.Build.Sql` SDK | 2.2.0 |

Fully conformant with house convention: xUnit + Shouldly + coverlet, no FluentAssertions, no Moq,
and the versions match the workspace's most modern repo.

**The `ProphetsWay.BaseDataAccess` reference is a NuGet `PackageReference`, not a project
reference.** Version `3.0.0` must actually be published to the feed for a clean-machine restore to
succeed. The BaseDataAccess repo is tagged `3.0.0` locally; whether the package is *on the feed* is
an open question — see below.

## Target Frameworks

| Project | TFMs |
|---|---|
| `.DataAccess` | `netstandard2.0;net48;net8.0;net9.0` |
| `.DataAccess.NoDB` | `netstandard2.0;net48;net8.0;net9.0` |
| `.Tests` | `net48;net8.0;net9.0` |

**This is exactly the house standard**, in canonical dotted form, and a substantial improvement over
the branch point (`net461;net471;net48;net50;net60;net70;net80;net90`). No end-of-life TFM remains,
no redundant TFM, and current LTS plus current are both present. `netstandard2.0` is correctly
omitted from the test project — a test project needs a runnable target.

No `LangVersion` pin remains in any project. **Nothing to recommend here; this is the reference the
other repos should copy.**

One inert property: `.DataAccess` declares
`<NullableContextOptions>enable</NullableContextOptions>`
([csproj#L28](ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L28)).
`NullableContextOptions` was the .NET Core 3.0 *preview* name for what shipped as `Nullable`. MSBuild
does not recognize it, so it is silently ignored — nullable reference types are **not** enabled.
`.NoDB` does not declare it at all, so the two projects are inconsistent about a setting neither
actually has. Enabling `<Nullable>enable</Nullable>` for real would surface warnings across
`netstandard2.0`/`net48` and is not a merge blocker, but the dead property is misleading to a reader
of a repo whose whole job is to be read.

## Packaging Audit

**PACKAGING: informational — not currently published.**

Publication intent is explicitly negative:
[app-variables.yml](app-variables.yml) has both `PostTargetToNuGet` and `TargetProject` **commented
out**, and every packaging element in both DAL `.csproj` files is an empty self-closing stub
(`<PackageId />`, `<RepositoryUrl />`, `<PackageTags />`, …). The test project sets
`<IsPackable>false</IsPackable>`.

This is **correct and intentional** — the repo is a teaching artifact, not a package. The empty
stubs are inherited boilerplate rather than a defect. No action required, and no snippet is proposed.

The one thing that *is* worth confirming is the inverse: the pipeline should be building and testing
only. `HasSqlProj: 'yes'` in [app-variables.yml](app-variables.yml#L8) means the shared template will
also build the `.sqlproj` — which is now SDK-style and therefore CLI-buildable for the first time.
That combination had never run in CI before this branch; build `3.0.0.486` on PR #19 succeeded, so
it is now confirmed working.

## Real Usage Examples Found

The best snippet in the repo is the swap point itself:

```csharp
public static IExampleDataAccess Create()
{
	//>>> The one line to change to point this suite at another implementation. <<<
	return new ExampleDataAccess();
}
```

And the disposal implementation, which is the clearest short statement of the 3.0.0 contract
anywhere in the workspace — idempotent, non-throwing, rolls back rather than commits, and pointedly
does *not* clear the store
([ExampleDataAccess.cs#L74](ProphetsWay.Example.DataAccess.NoDB/ExampleDataAccess.cs#L74)).

The `README.md` on this branch has already harvested these. It is 500+ lines, current, and accurate
against the code — unusually so.

## README Accuracy Check

| Existing claim | Verdict | Evidence |
|---|---|---|
| "138 Contract / 2 Characterization / 20 Dispatcher" | **Accurate** | Static trait count reproduces all three exactly |
| "`TestDataAccessFactory.Create` is the only place `new ExampleDataAccess()` appears" | **Accurate** | Single match repo-wide |
| "`IDepartmentDao` carries 19 numbered rules; `ICompanyResourceDao` carries 10" | **Accurate** | Both files carry the numbered rule sets |
| "`TransactionStart` throws `InvalidOperationException`…" | **Accurate** | [TransactionLog.cs#L50](ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L50) |
| "Every other member throws `ObjectDisposedException` once disposed" | **Accurate** | `ThrowIfDisposed()` on every delegating member |
| "`ProphetsWay.BaseDataAccess` 3.0.0 removed the `TargetInvocationException` wrapper" | **Accurate** | Asserted directly in `ExceptionPassthroughShowcaseTests` |
| **"`ProphetsWay.EFTools` carries an Entity Framework implementation of the very same `IExampleDataAccess` contract, and the tests do not change to accommodate it."** | **STALE — currently false** | EFTools consumes this repo as a **submodule pinned to `origin/main`** ([.gitmodules](../ProphetsWay.EFTools/.gitmodules)), i.e. at this branch's branch point. It has none of the 3.0.0 contracts — no `Department`, no `CompanyResource`, no snapshot rule, no `Dispose`. The claim is true of `main` and will be false the instant this branch merges, until EFTools advances its submodule pointer. |
| Badge points at `branchName=main` | **Expected but noted** | Correct once merged; shows nothing about this branch |

The README is otherwise in better shape than most released libraries. The EFTools claim is the
single thing to fix, and it is a *merge-ordering* problem rather than a wording problem.

## Gaps & Observations

Ordered by consequence.

1. **Merging this branch breaks the repo's headline claim until EFTools is updated.** The whole
   argument is "the same tests pass against both implementations." EFTools is pinned to the
   pre-3.0.0 commit, so after this merge the EF implementation will not satisfy `IExampleDataAccess`
   at all — it lacks `Dispose`, both new entities, and their DAO interfaces. **This is a
   coordination requirement, not a defect in this branch**, but it is the highest-consequence fact
   about merging it.

2. **The `.gitmodules` in EFTools has a malformed second entry.** Below the valid
   `[submodule "ProphetsWay.Example"]` block sits `[submodule "Submod"]` carrying only
   `branch = main` — no `path`, no `url`
   ([.gitmodules](../ProphetsWay.EFTools/.gitmodules#L5)). This is stray, and while git tolerates
   it today, it will confuse `git submodule` operations. Belongs to the EFTools repo; flagged here
   because it was discovered while verifying the submodule relationship.

3. **Gap #1 in the 3.0.0 coverage table** — the nullable/reference-type identifier half of
   `Get<T>(null)` is undemonstrated. See that table.

4. **`<NullableContextOptions>` is an inert property.** Silently does nothing; the project it sits
   in does not have nullable reference types enabled despite appearing to.

5. **A developer-specific publish profile is committed.**
   [ProphetsWay.Example.localhost.publish.xml](ProphetsWay.Example.Database/ProphetsWay.Example.localhost.publish.xml)
   contains `Data Source=Terebellum` — an internal machine name — and is referenced as `<None>` from
   the `.sqlproj`. **No credentials are present** (Integrated Security, `Persist Security Info=False`),
   so this is not a secret leak. It is a hostname disclosure and a per-developer file in a shared
   repo. Consider gitignoring it or renaming it to something generic.

6. **A stale `.dbmdl` sidecar remains on disk** at
   `ProphetsWay.Example.Database/ProphetsWay.Example.Database.dbmdl` after the SDK-style migration.
   It **is** covered by [.gitignore](.gitignore#L226), so it is untracked local debris rather than a
   committed artifact. Safe to delete; harmless if left.

7. **Stale `obj/` output from long-dead TFMs.** `ProphetsWay.Example.DataAccess/obj/Debug/` still
   holds folders for `net40`, `net45`, `net451`, `net452`, `net46`, `net461`, `net471`, `net50`,
   `net60`, `net70`, `netcoreapp2.1`, `netcoreapp3.1`, `netstandard2.1`, plus both dotted and
   undotted `net8.0`/`net80` forms. Gitignored, so cosmetic — but a `dotnet clean` or a manual
   `obj/` wipe before the verification build is advisable, so that a stale artifact cannot mask a
   real failure.

8. **Visual Studio can no longer open the solution intact.** The `.sqlproj` migration to
   `Microsoft.Build.Sql` is correct per house convention and is what makes `dotnet build` work at
   all, but VS 2022/2026 cannot load an SDK-style `.sqlproj`. The CHANGELOG documents this honestly.
   For a repo whose purpose is to be opened and read by newcomers, the onboarding note ("unload the
   database project, or use VS Code / the CLI") deserves to be in the README, not only the CHANGELOG.

9. **`HasSqlProj: 'yes'` + SDK-style `.sqlproj` was an untested CI combination — now tested.** The
   database project had never been built by the shared pipeline in this form; build `3.0.0.486` on
   PR #19 built it successfully.

## Open Questions for the Owner

1. ~~**Is `ProphetsWay.BaseDataAccess` 3.0.0 actually published to the NuGet feed?**~~ **Answered.**
   The contracts project consumes it as a `PackageReference`, not a project reference, so a clean
   agent could only restore it from the feed — and build `3.0.0.486` on PR #19 restored, built, and
   tested successfully. The package is available to CI.

2. **What is the intended merge order for this repo and EFTools?** Merging here first leaves the
   README's central claim false and EFTools' submodule pointing at a contract that no longer exists.
   Advancing EFTools' submodule pointer and updating `ProphetsWay.Example.DataAccess.EF` to the
   3.0.0 contracts in the same change set is the only ordering that keeps the claim true throughout.

3. **Should an entity with a `string` or `int?` identifier be added** to demonstrate the accepting
   half of the `Get<T>(null)` split? Adding one is a real cost — a new entity touches contracts,
   DAO, implementation, database schema, and the EF mirror. Declining is defensible; leaving the
   asymmetry undocumented is not.

4. **Is the `[submodule "Submod"]` entry in EFTools' `.gitmodules` deliberate,** or leftover from an
   abandoned experiment? It appears to be dead configuration.

5. **Should `ProphetsWay.Example.localhost.publish.xml` stay in the repo?** It is developer-specific
   and names an internal host.
