# AGENTS.md — ProphetsWay.Example

<!-- ═══════════════════════════════════════════════════════════════════════
     BEGIN SHARED BLOCK
     Generated from prophets-pipelines/conventions/AGENTS.shared.md
     DO NOT EDIT BY HAND — run /sync-agents-md to regenerate.
     ═══════════════════════════════════════════════════════════════════════ -->

## About This Codebase

`ProphetsWay.*` is a family of small, focused .NET libraries by G. Gordon Nasseri, published to
NuGet under the `ProphetsWay.` prefix and hosted at `github.com/ProphetManX`. Each library lives
in its own repository with its own version line, changelog, and pipeline.

### The Two Families

| Family | Repos | Purpose |
|---|---|---|
| **Utility** | Utilities, Logger, Hasher | Standalone helpers with no dependency on each other |
| **Data Access** | BaseDataAccess, EFTools | A layered DAL-decoupling paradigm; EFTools implements BaseDataAccess |

`ProphetsWay.Example` is a reference implementation, not a published package.
`prophets-pipelines` holds shared Azure DevOps YAML templates and this conventions file.

## Naming

**Display vs. codified.** The organization name is written two ways, and the distinction matters:

- **Display name** — `Prophet's Way`, with the apostrophe. Used in `<Company>`, prose, README text, and anything a human reads.
- **Codified** — `ProphetsWay`, no apostrophe or space. Used in namespaces, package IDs, assembly names, repo names, and the Azure DevOps org.

| Thing | Rule | Example |
|---|---|---|
| Repository | `ProphetsWay.<Library>` | `ProphetsWay.Logger` |
| Package ID | matches repository | `ProphetsWay.Logger` |
| Assembly name | matches repository | `ProphetsWay.Logger` |
| Library project folder | matches repository | `ProphetsWay.Logger/` |
| Test project | `<Library>.Tests` — **plural** | `ProphetsWay.Logger.Tests` |
| Example project | `<Library>.Example` | `ProphetsWay.Logger.Example` |
| `<Company>` | display name | `Prophet's Way` |
| `<Authors>` | `G. Gordon Nasseri` | |
| `<Product>` | library name without prefix | `Logger` |

### Namespaces

The rule is **family-dependent**. Do not "correct" one family to match the other.

- **Utility family** shares one root namespace regardless of assembly name:
  `ProphetsWay.Utilities`, with sub-namespaces for areas (`ProphetsWay.Utilities.LoggerDestinations`).
  A consumer adds one `using ProphetsWay.Utilities;` and reaches every utility library.
  This is why `ProphetsWay.Logger.dll` declares `namespace ProphetsWay.Utilities` — intentional, not a bug.
- **Data Access family** uses per-library namespaces: `ProphetsWay.BaseDataAccess`, `ProphetsWay.EFTools`
  (plus key-type sub-namespaces `.Guid`, `.Int`, `.Long`). These are an architectural paradigm, not
  utilities, and are kept separately addressable.
- **Test projects always use their own namespace**, `<AssemblyName>.Tests` — never the shared root.

## Target Frameworks

Standard set for new and modernized libraries:

```xml
<TargetFrameworks>netstandard2.0;net48;net8.0;net9.0</TargetFrameworks>
```

- `netstandard2.0` — maximum reach for older consumers
- `net48` — final .NET Framework release, for legacy consumers
- `net8.0` / `net9.0` — current LTS and current

Do not add a TFM without a consumer who needs it. Every extra target multiplies build time and
holds the whole library back to the oldest target's language features. TFMs below `net48`,
plus `netcoreapp*`, `net5.0`, `net6.0`, and `net7.0`, are end-of-life — treat them as debt.

Write monikers in canonical dotted form (`net8.0`, not `net80`). The undotted form parses, but
it is non-standard and inconsistent across the repos.

## Packaging Metadata

Required in every **published** library's `.csproj`. If a repo is not published to NuGet, these
are optional — but they become mandatory the moment publishing is on the table.

```xml
<PackageId>ProphetsWay.Thing</PackageId>
<Product>Thing</Product>
<Authors>G. Gordon Nasseri</Authors>
<Company>Prophet's Way</Company>
<Description>...</Description>
<RepositoryType>GitHub</RepositoryType>
<RepositoryUrl>https://github.com/ProphetManX/ProphetsWay.Thing</RepositoryUrl>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
<PackageRequireLicenseAcceptance>true</PackageRequireLicenseAcceptance>
<PackageIcon>profile.png</PackageIcon>
<PackageReadmeFile>README.md</PackageReadmeFile>
<PackageTags>...</PackageTags>
```

Paired with the item group that actually packs those files — declaring `PackageIcon` or
`PackageReadmeFile` without the matching `<None Pack="true">` packs nothing and fails the build:

```xml
<ItemGroup>
  <None Include="..\CHANGELOG.md" Link="CHANGELOG.md" Pack="true" PackagePath="" />
  <None Include="..\README.md" Link="README.md" Pack="true" PackagePath="" />
  <Content Include="..\profile.png" Link="profile.png" Pack="true" PackagePath="" />
</ItemGroup>
```

**An empty self-closing element is not a value.** `<PackageId />` silently falls back to
`AssemblyName` and leaves the nuget.org listing without a license, readme, or source link.
Treat empty stubs as missing.

Versioning is owned by the pipeline. Leave `<Version />`, `<AssemblyVersion />`,
`<FileVersion />`, and `<InformationalVersion />` empty in the csproj — `app-variables.yml`
supplies them at build time.

## Testing

- **xUnit** — the test framework. Do not introduce NUnit or MSTest.
- **Shouldly** — assertion style. Prefer `result.ShouldBe(...)` over `Assert.Equal`.
  FluentAssertions 8.x requires a paid commercial license; do not add it to any project.
- **coverlet.collector** — coverage.
- **Moq** — only where a test genuinely needs a mock; most of these libraries do not.
- Test class names mirror the type under test: `HasherTests`, `FileDestinationTests`.
- Tests requiring a local database set `LocalTestsOnly: 'yes'` in `app-variables.yml` so CI skips them.

## Pipelines

Every repo consumes the shared templates in `prophets-pipelines` via two root files:

| File | Purpose |
|---|---|
| `app-variables.yml` | Per-repo values — `Major`/`Minor`/`Patch`, `TargetProject`, `Product`, `RepoName`, `PostTargetToNuGet`, `LocalTestsOnly` |
| `local-pipeline.yml` | Thin wrapper pulling `prophets-pipelines` stage templates |

`Major`/`Minor`/`Patch` are bumped **by hand** in `app-variables.yml` as work proceeds. The
pipeline appends build metadata to produce alpha/beta/release packages.

## Repo Layout

```
ProphetsWay.Thing/
├─ AGENTS.md                 ← this file
├─ README.md                 ← packed into the nupkg
├─ CHANGELOG.md              ← packed into the nupkg
├─ LICENSE                   ← MIT
├─ profile.png               ← NuGet icon
├─ app-variables.yml
├─ local-pipeline.yml
├─ ProphetsWay.Thing.sln
├─ ProphetsWay.Thing/        ← library
├─ ProphetsWay.Thing.Tests/  ← xUnit
└─ docs/                     ← agent-generated analysis
```

`docs/` holds `repo-profile.md`, `purpose-and-scope.md`, and `nuget-extraction-proposal.md`.
These are generated by agents and committed.

## Solution Layout

For multi-project application solutions, the rule is **base name = contracts, suffix = swappable
implementation**. Business logic has one implementation and needs no split; a DAL has many and does.

| Project | Contains |
|---|---|
| `<Solution>.Core` | Domain models, business logic interfaces, and their implementation |
| `<Solution>.DataAccess` | DAL contracts only — interfaces and entities |
| `<Solution>.DataAccess.<Provider>` | One DAL implementation: `.MSSQL`, `.PostgreSQL`, `.MySQL`, `.NoDB`, `.EF` |
| `<Solution>.Database` | The `.sqlproj` database project |
| `<Solution>.Api` | Service endpoints |
| `<Solution>.Web` | Web UI |
| `<Solution>.Win` | Desktop UI |
| `<Project>.Tests` | xUnit tests for that project — `<Solution>.Core.Tests` |

**The suffix list is open.** A new provider or UI technology gets a new suffix following the same
shape (`.DataAccess.Cosmos`, `.Mobile`, `.Cli`). Do not invent a new *pattern* — extend this one.

A contracts project must never reference an implementation project, and must never expose a type
from a specific technology (`DbContext`, `SqlConnection`, `HttpContext`) in its public surface.
That rule is what makes the DAL swappable, and it is the whole point of the paradigm.

### Database Projects

New `.sqlproj` projects use the **`Microsoft.Build.Sql`** SDK — SDK-style, cross-platform, and
buildable with `dotnet build`:

```xml
<Project Sdk="Microsoft.Build.Sql/<version>">
```

The legacy SSDT format (`ToolsVersion="4.0"`, the 2003 MSBuild namespace, `TargetFrameworkVersion`,
plus `.dbmdl`/`.jfm` sidecar files) requires Visual Studio on Windows and cannot be built by the
.NET CLI. Existing legacy projects are **debt to migrate** — the `.sql` files carry over unchanged;
the project header and sidecars are what change.

## Code Style

- Tabs for indentation in `.csproj` and `.cs`.
- Braces on their own line (Allman).
- Interfaces prefixed `I`. Abstract bases prefixed `Base` or `Root`.
- Public API surface gets XML doc comments; internals do not need them.
- No `.editorconfig` exists yet — style is convention, not enforced. Match surrounding code.

## Rules for Agents

- **Never edit `.cs`, `.csproj`, `.sln`, or `.yml`** unless the human explicitly asks in that turn.
  Propose changes as fenced snippets labeled `PROPOSED — not applied`.
- **Exception — the TDD agents.** `Interface Architect`, `Test Designer`, `Implementer`, and
  `Refactorer` exist to write code; invoking one *is* the explicit ask. Each is restricted to one
  kind of file, and those restrictions are load-bearing:

  | Agent | May write |
  |---|---|
  | `Interface Architect` | Interfaces and their supporting types — never tests, never implementations |
  | `API Designer` | HTTP contracts and `docs/api/` — never implementations |
  | `Test Designer` | `*Tests.cs` only |
  | `Implementer` | Implementation `.cs` only — **never** a test file |
  | `Refactorer` | Implementation `.cs` only, behavior-preserving — **never** a test file |
  | `Modernizer` | `.csproj` / `.sqlproj` build and packaging config — never versions, never namespaces |
  | `Changelog Author` | `CHANGELOG.md` only |
  | `Threat Modeler`, `Security Reviewer` | `docs/security/` only — read-only on source |

  If an agent edits a test to make it pass, the workflow has failed. Report it rather than
  accepting the green build.
- **Never bump a version** in `app-variables.yml`. That is a human decision.
- **Never invent an Azure DevOps `definitionId`.** Badge URLs must be copied from a file that
  already exists in the repo. If one is missing, ask.
- **A namespace change is a binary-breaking change.** Never make one casually; it requires a major
  version bump and a CHANGELOG entry.
- Respect the family split above. `ProphetsWay.EFTools` living outside `ProphetsWay.Utilities`
  is correct, not drift.
- Deviations from these conventions are listed per-repo below. They are known, not overlooked —
  do not re-report them as discoveries.

<!-- ═══════════════════════════════════════════════════════════════════════
     END SHARED BLOCK
     ═══════════════════════════════════════════════════════════════════════ -->

---

## This Repo

**Family:** Data Access (reference implementation) · **Published:** no

Not a library. This is the **teaching artifact** for the Data Access family — a worked example of
consuming `ProphetsWay.BaseDataAccess` contracts, demonstrating that the same business logic runs
against completely different DAL implementations with no changes.

Because its job is to be *read*, clarity beats cleverness here. Prefer obvious code over concise
code, and keep the domain model small enough to hold in your head.

As of **3.0.0** it is also an **executable specification**: `IExampleDataAccess` carries two
DAL-wide contract rules in its `<remarks>`, and the test suite is partitioned by a `Scope` trait so
a newly written DAL can run the subset it is actually bound by.

### The Point It Proves

`ProphetsWay.Example.DataAccess` defines the domain and DAO interfaces. `ProphetsWay.Example.Tests`
runs against them. `ProphetsWay.Example.DataAccess.NoDB` is an in-memory implementation. In the
EFTools repo, `ProphetsWay.Example.DataAccess.EF` is a second implementation of the same contracts.
**The same tests pass against both** — that is the entire argument for the paradigm, and it is the
thing any documentation of this repo must lead with.

`ProphetsWay.Example.Tests/TestDataAccessFactory.cs` is the **only** file in the suite that names a
concrete implementation. Changing the single `return` in `Create` repoints all 160 tests. Do not
introduce a second construction site — doing so silently destroys the property this repo exists to
demonstrate.

### Layout

| Project | Role |
|---|---|
| `ProphetsWay.Example.DataAccess/` | Domain — `Entities/`, `IDaos/`, `Enums/`, `IExampleDataAccess` |
| `ProphetsWay.Example.DataAccess.NoDB/` | In-memory DAL implementation (`DataStore`, `StoreTable`, `StoreList`, `TransactionLog`, `Daos/`) |
| `ProphetsWay.Example.Database/` | SQL database project — SDK-style `Microsoft.Build.Sql/2.2.0` |
| `ProphetsWay.Example.Tests/` | xUnit + Shouldly, 160 tests written against the interfaces, not an implementation |

TFMs are `netstandard2.0;net48;net8.0;net9.0` for both DAL projects and `net48;net8.0;net9.0` for
the tests — the house standard, in canonical dotted form. **This repo is the TFM reference; copy it
rather than the older repos.**

### Domain Model

Seven entities, each with a matching `I*Dao` and one DAO implementation per DAL. The mapping is
complete and symmetrical in both directions — adding an entity means touching all three layers plus
the database project.

| Entity | Marker | Identifier | Shows |
|---|---|---|---|
| `Company` | `BaseIntEntity` | `int` | Paged DAO plus a custom method |
| `Job` | `BaseIntEntity` | `int` | `GetAll` only |
| `User` | `BaseIntEntity` | `int` | `IBaseDao<T>` plus a custom method; navigation properties |
| `Transaction` | `IBaseIdEntity<long>` | `long` | A non-`int` key; the deepest object graph |
| `Resource` | `IBaseIdEntity<Guid>` | `Guid` | A client-generated key |
| `Department` | `BaseIntEntity, IBaseSoftIdEntity<int>` | `int` | **Soft delete**, plus custom `Restore`; 19 numbered contract rules |
| `CompanyResource` | `IBaseEntity` | **none** | **A keyless join entity** whose DAO inherits no `IBaseDao<T>` at all; 10 numbered rules |

`Department` and `CompanyResource` are new in 3.0.0 and exist to mark the edges of the paradigm.
Namespaces follow the folder structure: `ProphetsWay.Example.DataAccess.Entities`, `.IDaos`,
`.Enums`, `.DataAccess.NoDB.Daos`, `.Tests`.

### Behavioral Contracts Worth Knowing

Read these before changing a DAO or writing a second implementation. The XML `<remarks>` are the
source of truth; this is an index, not a restatement.

- **The snapshot rule**, on `IExampleDataAccess`, binds every DAO: reads return **deep** snapshots
  and writes read their argument rather than adopting it. It is what makes an in-memory store and a
  database interchangeable, and it is what lets a rollback actually reverse an `Update`. An
  implementation that hands back the instance it is holding fails this.
- **The ordering rule**, also on `IExampleDataAccess`: order is unspecified but **stable**, so paged
  windows partition a full pass with no overlap or omission. A SQL-backed DAL satisfies this only
  with an explicit `ORDER BY` — omit it and the suite passes today and fails intermittently at some
  future row count.
- **Transactions are scoped to the DAL instance, not the store.** `TransactionLog` is an instance
  field on `ExampleDataAccess` deliberately. Moving it to `DataStore` would let one instance roll
  back another's writes.
- **`Dispose` releases what the instance created and nothing else.** It never clears `DataStore` —
  disposing a DAL no more empties the database than closing one connection does. Every test in the
  suite disposes an instance, so a store-clearing `Dispose` would delete rows out from under
  concurrently running tests.
- **The `Scope` trait partition is load-bearing**, and every test carries one:
  `Contract` (138) is what any conforming implementation must pass; `Characterization` (2) pins
  choices this implementation made that the contract does not require; `Dispatcher` (20) exercises
  the reflection convention in `ProphetsWay.BaseDataAccess` itself and belongs to no DAL. Adding a
  test without a trait breaks `dotnet test --filter "Scope=Contract"` as a usable gate.
- **The `ConventionShowcase/` DALs are deliberately mis-wired.** They are the subject under test,
  not the implementation under test, and they construct themselves rather than using the factory.
  Do not "fix" them.

### 3.0.0 Coverage — What Is and Is Not Demonstrated

Demonstrated: `IDisposable` on the DAL, idempotent non-throwing `Dispose`, `ObjectDisposedException`
from every other member, disposal rolling back an open transaction, all three transaction members,
no nesting, per-instance transaction scope, unwrapped exception propagation (with an explicit
`ShouldNotBeOfType<TargetInvocationException>()` regression guard), and `Get<T>(null)` throwing
`ArgumentException` on a non-nullable value-type identifier.

**Not demonstrated** — do not report these as discoveries:

| Gap | Why |
|---|---|
| `Get<T>(null)` being *accepted* where the identifier is a reference type or nullable value type | Every entity here keys on `int`, `long`, or `Guid`. Only the throwing half of the split is shown. **The most useful gap to close.** |
| Value-type (`struct`) entities and their inability to express "not found" as `null` | No struct entity exists |
| Bare `IBaseSoftEntity` — soft delete without an identifier | `Department` is `IBaseSoftIdEntity<int>`; the soft × keyless corner is empty |
| Ambient `TransactionScope` being left untouched | No `System.Transactions` reference; hard to show meaningfully in-memory |

### Known Deviations

| # | Deviation | Notes |
|---|---|---|
| 1 | **`ProphetsWay.EFTools` consumes this repo as a git submodule** | **It is a submodule, not a vendored copy — earlier versions of this file said otherwise and were wrong.** `ProphetsWay.EFTools/.gitmodules` declares `path = ProphetsWay.Example`, `url = …/ProphetsWay.Example.git`, `branch = main`. The two therefore **cannot drift**; the submodule is simply *pinned*, currently to `origin/main` at the 3.0.0 branch point. The real consequence is a **coordination requirement, not a duplication problem**: EFTools has picked up none of the 3.0.0 work, so until its pointer is advanced and `ProphetsWay.Example.DataAccess.EF` is updated, the EF implementation does not satisfy the current `IExampleDataAccess`. Never edit files under `ProphetsWay.EFTools/ProphetsWay.Example/` — edit here and advance the pointer. |
| 2 | Has `app-variables.yml` / `local-pipeline.yml` despite not being published | **Correct and verified.** `PostTargetToNuGet` and `TargetProject` are both commented out; the pipeline builds and tests only. |
| 3 | Not packaged; packaging metadata is empty stubs | Correct and intentional. A teaching artifact is not a package — do not fill these in. |
| 4 | `<NullableContextOptions>enable</NullableContextOptions>` in `ProphetsWay.Example.DataAccess.csproj` | **Inert.** That was the .NET Core 3.0 *preview* name for what shipped as `<Nullable>`; MSBuild ignores it, so nullable reference types are not actually on. `.NoDB` does not declare it at all. Cosmetic, but misleading in a repo meant to be read. |
| 5 | Visual Studio cannot open the SDK-style `.sqlproj` | A VS 2022/2026 limitation, not a defect. The migration is what makes `dotnet build` work on the solution at all. VS Code, SSMS 22, and the .NET CLI are fine; a VS user can unload the database project and work on the three C# projects normally. |
| 6 | `ProphetsWay.Example.localhost.publish.xml` is committed and names an internal host | `Data Source=Terebellum`, Integrated Security — **no credentials**, so not a secret leak, but it is a per-developer file in a shared repo. |

Deviations previously listed here about duplicated projects drifting independently are **resolved
and were factually wrong**; do not reinstate them.

### Documentation Angle

When writing this repo's README, the reader is someone evaluating whether the BaseDataAccess
paradigm is worth adopting. Lead with the swap-the-DAL demonstration, not with a project listing.

Two things the current README should gain: an onboarding note that Visual Studio cannot load the
SDK-style database project (and what to do about it), and a correction to the claim that EFTools
runs the same suite unchanged — true of `main`, not true of 3.0.0 until EFTools advances its
submodule pointer.
