# Repo Profile — ProphetsWay.Example

_Generated 2026-08-15, against the working tree at **v3.1.0**, immediately after the `Modernizer`
retarget pass and the `Purpose Refiner` pass. **Amended 2026-08-16** for the two `Scope` retraits and
a re-check of the README. **Amended again 2026-08-16** on branch `3.1.1-eftool-findings`: the suite is
now **164 tests / 328 executions**, the version line reads **3.1.1**, `IExampleDataAccess` carries
**four** DAL-wide rules, and the EFTools submodule pointer has been advanced._

> **Amended 2026-08-23, at HEAD `f93f0a4`.** Three claims carried by the previous edition were
> **measurably false** and are corrected below, each citing the file that was opened: the
> `ProphetsWay.BaseDataAccess` reference is **3.2.0**, not 3.1.0; the EFTools submodule pointer is
> **`f93f0a4`**, not `61d9e7d`; and the EFTools run is **270 / 270 / 0**, not "151 tests, 123 passed,
> 28 failed" against a `CompanyResourceDao` that "does not exist." A fourth item is new rather than
> wrong: `f93f0a4` added a **capability-declaration mechanism** to the test project, documented in its
> own section below. **Every count in this edition was re-derived by grep, not carried** — which is the
> whole reason for this pass.

> **Supersedes the 2026-08-12 edition**, which profiled the `3.0.0-feature-update` branch at commit
> `122dc69` and cited CI build `3.0.0.486` on PR #19. That branch is merged and that evidence
> describes 3.0.0, not the current tree. Nothing from it is carried forward unverified.

**Verified current state — every row re-opened 2026-08-23:**

| Thing | Value | Source |
|---|---|---|
| `ProphetsWay.Example.DataAccess` | `netstandard2.0;net10.0` | [csproj](../ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L4) |
| `ProphetsWay.Example.DataAccess.NoDB` | `netstandard2.0;net10.0` | [csproj](../ProphetsWay.Example.DataAccess.NoDB/ProphetsWay.Example.DataAccess.NoDB.csproj#L4) |
| `ProphetsWay.Example.Tests` | `net48;net10.0` | [csproj](../ProphetsWay.Example.Tests/ProphetsWay.Example.Tests.csproj#L4) |
| `ProphetsWay.BaseDataAccess` reference | **3.2.0** (NuGet `PackageReference`) — **was 3.1.0; changed 2026-08-23** | [csproj](../ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L29) |
| Suite | 164 tests × 2 legs = **328 executions** | Static count re-derived 2026-08-23 — see below |
| `Scope` partition | **139 Contract / 5 Characterization / 20 Dispatcher** | Static count re-derived 2026-08-23 — sums to 164 |
| Version | 3.1.1, set by hand | [app-variables.yml](../app-variables.yml#L3) |
| EFTools submodule pointer | **`f93f0a4`** — this repo's own tip, on the untagged 3.1.1 line | `ProphetsWay.EFTools/.git/modules/ProphetsWay.Example/HEAD` |
| EFTools run against these contracts | **270 total / 270 passed / 0 failed** | Owner-reported 2026-08-23; see the EFTools section below |

---

## ✅ Build & Test Verification

**The solution builds and the full suite passes on both legs.** The evidence is the local
`Modernizer` verification run of 2026-08-15, which built and tested `net48` and `net10.0`
independently and confirmed 160 tests on each — **320 executions total**.

**That run predates the tree.** Two test classes were retraited and split on 2026-08-16 —
[SnapshotDeepCopyTests.cs](../ProphetsWay.Example.Tests/SnapshotDeepCopyTests.cs) and
[UserDaoTests.cs](../ProphetsWay.Example.Tests/UserDaoTests.cs) — each keeping a `Contract` assertion
and moving an over-claiming one into a new `Characterization` sibling. **The same two files were then
extended with a test each**, closing a gate hole where a cascading `Update` had been passing every
`Contract` test; `UserDaoTests.ShouldCallCustomUserFunctionality` moved to `Characterization` in the
same pass. The suite has been **164 tests, 328 executions** ever since.

**`f93f0a4` did not change that.** It renamed the two `Characterization` tests those files carry and
gave them a branch, but added no test and removed none — the total and the partition were both
re-derived on 2026-08-23 and are unchanged. The owner reports the local run time falling from
**7.32 s to 5.78 s** on this repository's own suite; that figure is owner-reported and was not
measured here.

**This analyst pass did not itself run a build.** No claim below rests on a build I performed; where
the evidence is the `Modernizer` run it says so, and where it is an owner report it says that. There
is **no CI build for 3.1.0 or 3.1.1 yet** — the last recorded pipeline evidence is build `3.0.0.486`
on PR #19, which validated the 3.0.0 tree and is superseded.

### Statically re-verified in this pass — 2026-08-23

Nothing in this table is carried from the previous edition. Each row names the method used.

| Check | Method | Result |
|---|---|---|
| TFM lists | Read all three `.csproj` | Match the table above — unchanged |
| BaseDataAccess version | Read the single `PackageReference` | **`3.2.0`** — the previous edition's `3.1.0` is false |
| Suite total | Grep of `[Fact]`, `[Theory]`, `[InlineData]` across `ProphetsWay.Example.Tests/` | **149 `[Fact]` + 6 `[Theory]` carrying 15 `[InlineData]` = 164 cases** — unchanged, and now re-derived rather than restated |
| `Scope` trait partition | Static count of all 52 `[Trait(…)]` attributes, class- and method-level, against the per-file `[Fact]`/`[Theory]` counts | **139 Contract / 5 Characterization / 20 Dispatcher** — sums to 164, the suite total |
| Untagged tests | Cross-check of class-level vs. method-level traits, file by file | **None** — every test carries exactly one `Scope`. Four classes declare it per method: `CompanyDaoTests`, `DataAccessTransactionTests`, `SnapshotDeepCopyTests`, `UserDaoTests` |
| `<NullableContextOptions>` | Read both DAL `.csproj` in full | **Still absent** — the property remains gone |
| Single construction site | Repo-wide search for `new ExampleDataAccess` outside `ConventionShowcase/` | **One**, in [TestDataAccessFactory](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs) |
| EFTools submodule pointer | Read `ProphetsWay.EFTools/.git/modules/ProphetsWay.Example/HEAD` | **`f93f0a41a76834647962ddf9e830e01e24e05f24`** — the previous edition's `61d9e7d` is false |
| `CompanyResourceDao` on the EF side | File search under `ProphetsWay.EFTools/ProphetsWay.Example.DataAccess.EF/` | **Exists** — `Daos/CompanyResourceDao.cs`. The previous edition's "does not exist" is false |

**Why the counts did not move even though HEAD did.** `f93f0a4` added a mechanism, not a test — see
*The Capability Declaration* below. That is exactly the situation in which a figure gets copied
instead of checked, so it was checked.

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

Expect 164 tests on each of `net48` and `net10.0` — **328 executed test cases total**. The solution
includes the `.sqlproj`, so `dotnet build` also exercises the SDK-style database project.

### Warnings

**The two `xUnit1013` warnings previous editions carried are closed — verified 2026-08-23.** Both
helpers on [DepartmentDaoTests.cs](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs) are now
`private static`, not `public static`: `EditEveryFieldAfterTheCall` at
[#L294](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs#L294) and `AssertEveryStampIsUtc` at
[#L1001](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs#L1001), read by grepping every `static`
member declaration in the file. The sentence "both helpers are still present and still public,
verified 2026-08-16" is **superseded and must not be restated**. `ProphetsWay.EFTools` built this
project reporting **0 warnings** on 2026-08-22, which is consistent with that.

---

## One-Line Purpose

The reference implementation and executable specification for the `ProphetsWay.BaseDataAccess`
paradigm — a worked domain, one in-memory DAL, and a 164-test suite written against interfaces so
that pointing it at a different DAL is a one-line change.

## What It Actually Does

Four projects split along the contracts/implementation seam the paradigm exists to enforce:

- [ProphetsWay.Example.DataAccess](../ProphetsWay.Example.DataAccess) declares seven entities, seven
  `I*Dao` interfaces, and the aggregate
  [IExampleDataAccess](../ProphetsWay.Example.DataAccess/IExampleDataAccess.cs). It references
  `ProphetsWay.BaseDataAccess` **3.2.0** from NuGet and nothing else.
- [ProphetsWay.Example.DataAccess.NoDB](../ProphetsWay.Example.DataAccess.NoDB) implements all of it
  over a process-wide in-memory store.
- [ProphetsWay.Example.Tests](../ProphetsWay.Example.Tests) exercises the contracts. Exactly one
  method in the project — [TestDataAccessFactory.Create](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs#L38)
  — names a concrete implementation.
- [ProphetsWay.Example.Database](../ProphetsWay.Example.Database) is the SQL Server schema the
  contracts could be backed by.

The most substantive thing here is not code volume but **specification**. The `<remarks>` on
[IExampleDataAccess](../ProphetsWay.Example.DataAccess/IExampleDataAccess.cs#L10) carry **four**
DAL-wide rules — a **snapshot rule** (reads return deep copies, writes read rather than adopt their
argument), an **ordering rule** (unspecified but stable order; paged windows partition a full pass),
an **identifier rule** (`Insert` assigns the generated identifier back onto the caller's instance),
and a **row count rule** (`Update` and `Delete` return the number of rows matched and written). The
last two were added on 2026-08-16; earlier editions of this document said there were two, which was
accurate at 3.1.0. Each is written with an explanation of *why an in-memory store satisfies it
incidentally while SQL Server does not*, which is precisely the class of divergence that would
falsify the repo's central claim.

**v3.1.0 changed no `.cs` file.** It is a retarget plus documentation release; the suite **it shipped**
is byte-identical to 3.0.0's, and that is deliberately what makes its passing the evidence for the
retarget. **The working tree has since moved on** — two test files were retraited, split and then
extended, and `IExampleDataAccess.cs` gained two rules, taking the suite from 160 tests to 164 — so
the byte-identical claim describes v3.1.0 as released and must not be restated about the current tree.

## Projects in the Solution

Source: [ProphetsWay.Example.sln](../ProphetsWay.Example.sln)

| Project | Type | Role |
|---|---|---|
| `ProphetsWay.Example.DataAccess` | Library | Contracts — `Entities/`, `IDaos/`, `Enums/`, `IExampleDataAccess` |
| `ProphetsWay.Example.DataAccess.NoDB` | Library | In-memory DAL implementation |
| `ProphetsWay.Example.Database` | `.sqlproj` | SQL Server schema + post-deploy seed scripts |
| `ProphetsWay.Example.Tests` | xUnit | 164 tests written against the interfaces |

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

Statuses re-read from [feature-requests.md](feature-requests.md) on 2026-08-23; all four are
unchanged. **The index itself has grown to seventeen entries**, and four are now `Done` — 5, 11, 13
and 17. Only `Purpose Refiner` may change a status; nothing in this pass did.

## The Capability Declaration — new in `f93f0a4`

Read from [StoreCapabilities.cs](../ProphetsWay.Example.Tests/StoreCapabilities.cs) and
[TestDataAccessFactory.cs](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs) on 2026-08-23. It is
the one substantive addition since the previous edition of this document, and it is easy to restate
backwards.

| Piece | What it is |
|---|---|
| `StoreCapabilities` | `[Flags] public enum` — `None = 0`, `DenormalizedNavigationWrites = 1`, `TransactionIsolation = 2` |
| `TestDataAccessFactory.InMemoryStore` | `public const StoreCapabilities` equal to `DenormalizedNavigationWrites` — `.NoDB`'s actual shape |
| `TestDataAccessFactory.Use(Func<IExampleDataAccess>, StoreCapabilities)` | The second parameter, **defaulting to `InMemoryStore`** |
| `TestDataAccessFactory.Capabilities` | A lock-guarded getter, read at execution time by **exactly two** tests |

**This repository declares nothing and gained no test.** The default is its own shape, so every call
site here is unchanged — which is why the suite total is still 164 and the partition still
139 / 5 / 20.

**The two readers were renamed to say what they assert rather than what their setup does:**

| Test | Scope | Where |
|---|---|---|
| `ShouldReadANavigationPropertyEditBackOnlyWhereTheStoreDenormalizesTheWrite` | `Characterization` | [SnapshotDeepCopyTests.cs](../ProphetsWay.Example.Tests/SnapshotDeepCopyTests.cs) |
| `ShouldExposeUncommittedWritesToAnotherInstanceOnlyWhereTheStoreDoesNotIsolateThem` | `Characterization` | [DataAccessTransactionTests.cs](../ProphetsWay.Example.Tests/DataAccessTransactionTests.cs) |

**The nuance, stated so it is not inverted later.** The flags are **positive capabilities on two
independent axes**, not a store-kind label. A normalized relational DAL declares `TransactionIsolation`
**only**, and the *absence* of `DenormalizedNavigationWrites` is itself a positive declaration — such a
store holds one Companies row read back through a join and physically cannot hold a second,
denormalized copy of a navigation node. **The branch that absence selects is the stronger of the two**:
it proves an `Update` did not cascade into the Company, Job and Department rows the caller never named,
asserted from inside the transaction that submitted it, where nothing else in the suite looks.

**In the transaction test the branch is around the read, not around the `ShouldBe`.** Against an
isolating store the uncommitted read does not fail, it *waits* on the row lock until the provider's
command timeout — so skipping only the assertion would leave the whole cost in place. What survives the
branch either way is the half both shapes owe: the row is gone once the transaction that wrote it is
abandoned.

**No `Scope=Contract` assertion reads the enum, and none may be made to.** A contract obligation a
caller can decline is not one. The reasoning and the limits are in the type's own `<remarks>` and in
[FR 17](feature-requests.md), `Done — 2026-08-23`. The `ConventionShowcase/` DALs are outside the
mechanism entirely — they construct their own instances and carry no dependency on the factory, so
`Use` does not and must not change what they exercise.

## Consistency Check Across the Four Projects

### Contracts project leakage — **clean** ✅

Every `using` in [ProphetsWay.Example.DataAccess](../ProphetsWay.Example.DataAccess) resolves to
`System`, `ProphetsWay.BaseDataAccess`, or a sibling namespace within the same project. There is
**no** `DbContext`, `SqlConnection`, `HttpContext`, or reference to `NoDB` anywhere in it, and its
only `PackageReference` is `ProphetsWay.BaseDataAccess` **3.2.0**
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
([TestDataAccessFactory.cs](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs)), and
`CreateAs<T>` performs a *checked* cast that names the missing interface rather than throwing a bare
`InvalidCastException` from a base constructor. The `ConventionShowcase` DALs deliberately bypass
the factory, and that decision is documented in the factory's own remarks. Both the implementation
seam and the capability declaration now travel through that one file — see the section above.

## Dependencies

| Project | Reference | Version |
|---|---|---|
| `.DataAccess` | `ProphetsWay.BaseDataAccess` | **3.2.0** (NuGet) |
| `.DataAccess.NoDB` | → `.DataAccess` | project reference |
| `.Tests` | `Microsoft.NET.Test.Sdk` | 17.13.0 |
| `.Tests` | `xunit` | 2.9.3 |
| `.Tests` | `xunit.runner.visualstudio` | 3.0.2 |
| `.Tests` | `Shouldly` | 4.3.0 |
| `.Tests` | `coverlet.collector` | 6.0.4 |
| `.Database` | `Microsoft.Build.Sql` SDK | 2.2.0 |

Every test-project version above was re-read from
[ProphetsWay.Example.Tests.csproj](../ProphetsWay.Example.Tests/ProphetsWay.Example.Tests.csproj) on
2026-08-23 and is unchanged. Fully conformant with house convention: xUnit + Shouldly + coverlet, no
FluentAssertions, no Moq.

**The `ProphetsWay.BaseDataAccess` reference is a NuGet `PackageReference`, not a project
reference**, so **3.2.0** must be on the feed for a clean-machine restore. It is — 3.2.0 shipped and is
live on nuget.org, and `ProphetsWay.EFTools` took the same bump on 2026-08-23 in commit `0da679f`.
The bump matters beyond a number: 3.2.0 is the release that added nullable **annotations**
(`<Nullable>annotations</Nullable>`), which is what let EFTools delete ten `CS8766` suppressions. This
repo cannot benefit — `netstandard2.0` caps shared code at C# 7.3 — and nothing here needed to change
to take it.

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

`ProphetsWay.BaseDataAccess` is at the same standard as of its 3.1.0 and remains so at **3.2.0**, the
version this repo now references. This repo is therefore **a** reference for the TFM convention rather
than **the** reference; either can be copied.

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

`README.md` is owned by `README Author`. **Every row below was re-checked against `README.md` on
2026-08-23 by grepping the file, not by re-affirming the previous edition.** The two rows this section
marked stale earlier that day were repaired by `README Author` in the pass that followed, and are recorded
below as closed rather than deleted; four rows are new, covering what `f93f0a4` changed. **No row is
currently stale.**

| Existing claim | Verdict | Evidence |
|---|---|---|
| Trait table — **"`Contract` 139 / `Characterization` 5 / `Dispatcher` 20"** ([README.md#L229](../README.md#L229)) | **Accurate — re-derived, not affirmed** | Static count of all 52 `[Trait(…)]` attributes against the per-file `[Fact]`/`[Theory]` counts gives 139 / 5 / 20, summing to 164. The table sits at lines 229–233 after the 2026-08-23 rewrite, and its `Characterization` row now reads "three are its own choices, and two adapt to the shape of store you declare" |
| **"164 tests … 328 executions"** ([README.md#L374](../README.md#L374), [#L793](../README.md#L793)) | **Accurate — re-derived** | 149 `[Fact]` + 15 `[InlineData]` = 164, on `net48` and `net10.0`. **Anchors re-read 2026-08-23 after the rewrite; the earlier #L201 / #L205 / #L292 / #L709 citations are superseded** |
| "`TestDataAccessFactory.Create` is the only place `new ExampleDataAccess()` appears" | **Accurate** | Single match repo-wide |
| "`IDepartmentDao` carries 19 numbered rules; `ICompanyResourceDao` carries 10" | **Accurate** | Both files carry the numbered rule sets |
| "`TransactionStart` throws `InvalidOperationException`…" | **Accurate** | [TransactionLog.cs#L50](../ProphetsWay.Example.DataAccess.NoDB/TransactionLog.cs#L50) |
| "Every other member throws `ObjectDisposedException` once disposed" | **Accurate** | `ThrowIfDisposed()` on every delegating member |
| "`ProphetsWay.BaseDataAccess` 3.0.0 removed the `TargetInvocationException` wrapper" | **Accurate** | Asserted directly in `ExceptionPassthroughShowcaseTests`; a statement about 3.0.0's history, still true at 3.2.0 |
| Data access layer and test project target frameworks | **Accurate** | README states `netstandard2.0` / `net10.0` for the two DAL projects and `net48` / `net10.0` for the tests — matches all three `.csproj` |
| **"the released `ProphetsWay.BaseDataAccess` **3.2.0**"** ([README.md#L402](../README.md#L402), and again at [#L44](../README.md#L44)) | **Accurate — the earlier STALE row is closed, not carried** | Both occurrences now read **3.2.0**, matching the single `PackageReference` at [csproj line 29](../ProphetsWay.Example.DataAccess/ProphetsWay.Example.DataAccess.csproj#L29). This row previously read "README says 3.1.0 — STALE"; `README Author` corrected the file after that pass |
| **The EFTools status paragraph** ([README.md#L39–L48](../README.md#L39)) | **Accurate — all three earlier defects fixed** | Line 41 names the pointer **`f93f0a4`** and calls it "four commits past the `3.1.0` tag"; line 44 reads **3.2.0**; and the "under way and **not finished** / throwing stubs" framing is gone, replaced by "Read that paragraph as fact, not as ambition" with **270 / 270 / 0**, the **245 / 245 / 0** gate, and FR 5 closed `Done`. The self-contradiction with line 8 no longer exists |
| `TestDataAccessFactory.Use` signature ([README.md#L138](../README.md#L138)) | **Accurate** | Reads `Use(Func<IExampleDataAccess>, StoreCapabilities)` — the two-parameter form `f93f0a4` introduced |
| Adapters in `ProphetsWay.EFTools.Tests` ([README.md#L173](../README.md#L173)) | **Accurate** | "carries **thirteen** of" this suite's test classes as empty derived classes, present only for xUnit discovery. The earlier "no adapter exists" wording is gone |
| The two `Characterization` test names ([README.md#L277](../README.md#L277), [#L283](../README.md#L283)) | **Accurate** | Both carry their post-`f93f0a4` names — `…OnlyWhereTheStoreDoesNotIsolateThem` and `…OnlyWhereTheStoreDenormalizesTheWrite` — and line 232 describes them as adapting rather than failing |
| Badge points at `branchName=main` | **Correct** | 3.0.0 is merged; the badge reflects this tree |

**Nothing in this pass edited `README.md`.** `README Author` ran after this profile's first 2026-08-23
edition and repaired every row it had marked stale; the rows above were re-grepped against the corrected
file on 2026-08-23. **No open README defect is known.**

**`CHANGELOG.md` — correcting a false claim this profile made earlier the same day.** The previous edition
of this section stated the file "has **no v3.1.0 entry** — its most recent heading is `v3.0.0`." **That is
false, and it was carried rather than checked** — `CHANGELOG.md` was the one evidence class not re-opened.
It has now been opened, and its `^# v` headings read directly: **`# v3.1.1 — not yet released` at line 1,
`# v3.1.0` at line 264, `# v3.0.0` at line 329.** The `v3.1.1` entry is deliberately open — the owner will
not tag the line until everything `ProphetsWay.EFTools` needs from this repository is in place — and it
documents the two things `f93f0a4` added: the two-parameter
`TestDataAccessFactory.Use(Func<IExampleDataAccess>, StoreCapabilities = InMemoryStore)` seam, and the
`StoreCapabilities` `[Flags]` enum with its two independent axes. It also records that this repository
declares nothing and its own counts therefore stand at **164** with the partition at **139 / 5 / 20**, and
that a one-parameter `Use` call still compiles. `Changelog Author` owns the file; nothing here edits it.
**Do not restate "no v3.1.0 entry" or "newest heading is v3.0.0."**

## Gaps & Observations

Ordered by consequence. Anything with an `FR n` reference is triaged in
[docs/feature-requests.md](feature-requests.md) and should not be re-litigated here.

1. **The repo's headline claim is no longer pending — it is demonstrated. Corrected 2026-08-23.**
   `ProphetsWay.EFTools` pins this repo as a submodule, and **the pointer is `f93f0a4`** —
   `f93f0a41a76834647962ddf9e830e01e24e05f24`, read from
   `ProphetsWay.EFTools/.git/modules/ProphetsWay.Example/HEAD`. That is **this repository's own tip**,
   four commits past the `3.1.0` tag on the open, untagged 3.1.1 line. `ProphetsWay.EFTools.Tests`
   adapts 13 of this repo's 15 test classes onto `ProphetsWay.Example.DataAccess.EF` through a
   `[ModuleInitializer]` calling
   `TestDataAccessFactory.Use(() => Constants.GetExampleDataAccess, StoreCapabilities.TransactionIsolation)`
   — read from `ProphetsWay.EFTools/ProphetsWay.EFTools.Tests/TestSeam.cs` line 53. **That run is 270
   total / 270 passed / 0 failed**, with the conformance gate `--filter "Scope=Contract|Guard=Seam"` at
   **245 / 245 / 0** in about two seconds. `Purpose Refiner` closed
   [FR 5](feature-requests.md#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) as
   **`Done — 2026-08-23`**.

   **Three claims the previous edition made here are dead and must never be restated:** the pointer is
   not `61d9e7d`; the run is not "151 tests, 123 passed, 28 failed"; and `CompanyResourceDao` **does
   exist** — `ProphetsWay.EFTools/ProphetsWay.Example.DataAccess.EF/Daos/CompanyResourceDao.cs`, found
   by file search on 2026-08-23. The 2026-08-20 claim that both EFTools `.csproj` files read **3.1.0**
   is also superseded; they read **3.2.0**.

   **What is left is release sequencing, not correctness.** The pointer sits on an *untagged* commit,
   and D11 step 3 wants it on a tag of this repository. Never edit files under
   `ProphetsWay.EFTools/ProphetsWay.Example/`; edit here and advance the pointer.

2. **Half of the `Get<T>(null)` split is taught as the whole of it.**
   [FR 1](feature-requests.md#1--demonstrate-the-accepting-half-of-gettnull) — and the decision that
   it be closed in `ConventionShowcase/` rather than by an eighth domain entity is the substantive
   part of that entry.

3. **Visual Studio cannot load the SDK-style `.sqlproj`,** and a newcomer meets that failure in the
   first minute of a repo designed to be opened. The CHANGELOG documents it; the README is where it
   belongs. [FR 7](feature-requests.md#7--a-visual-studio-onboarding-note-in-the-readme).

4. ~~**Two `xUnit1013` warnings** on `DepartmentDaoTests.cs`, from the two public static helpers.~~
   **CLOSED \u2014 verified 2026-08-23.** Both helpers are now `private static`, not `public static`:
   `EditEveryFieldAfterTheCall` at
   [DepartmentDaoTests.cs#L294](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs#L294) and
   `AssertEveryStampIsUtc` at
   [#L1001](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs#L1001), read by grepping every
   `static` member declaration in the file. `ProphetsWay.EFTools` built this same project reporting
   **0 warnings** on 2026-08-22, which is consistent. Do not re-report this as outstanding.

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
  `Modernizer` run, and **3.2.0** is now referenced and is live on nuget.org.
- **The EFTools demonstration** — closed 2026-08-23. 270 / 270 / 0 against
  `ProphetsWay.Example.DataAccess.EF`; [FR 5](feature-requests.md) is `Done`.
- **The two `xUnit1013` warnings** — both helpers are `private static` now; see *Warnings* above.

## Open Questions for the Owner

1. **When is this repository tagged, and is EFTools' pointer then moved onto the tag?** D11 step 3
   wants the submodule pinned to a tag; it currently sits on `f93f0a4`, an untagged commit on the
   3.1.1 line. This is the last item between the two repositories and publication, and it is release
   sequencing rather than correctness — the run is already green.

2. **Is a CI run wanted on 3.1.1 before the remaining documentation phases finish?** The retarget is
   verified locally on both legs, but the shared pipeline has not built the tree since `3.0.0.486`,
   and `HasSqlProj: 'yes'` means CI covers the `.sqlproj` that a local `dotnet test` largely does
   not.

3. ~~**Is the stray `[submodule "Submod"]` block in EFTools' `.gitmodules`** deliberate or
   abandoned?~~ **Closed** — EFTools' own `AGENTS.md` records the block as removed on 2026-08-16 and
   its 2026-08-22 build as 0 warnings. It belonged to that repository, not this one.

4. ~~**Does `CHANGELOG.md` get a v3.1.0 / v3.1.1 entry before the tag?** Its newest heading is still
   `v3.0.0`.~~ **Not a question — the premise was false, and it was this profile's own error.** The file
   was opened on 2026-08-23: `# v3.1.1 — not yet released` (line 1), `# v3.1.0` (line 264), `# v3.0.0`
   (line 329). Both entries exist, and the `v3.1.1` one documents the two-parameter
   `TestDataAccessFactory.Use` and `StoreCapabilities`. That file is `Changelog Author`'s and nothing in
   this pass touched it.
