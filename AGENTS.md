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

```xml
<!-- default for a published library -->
<TargetFrameworks>netstandard2.0;net10.0</TargetFrameworks>

<!-- only when a framework-conditional dependency or API requires it -->
<TargetFrameworks>netstandard2.0;net48;net10.0</TargetFrameworks>

<!-- test projects — netstandard2.0 is not a valid test target -->
<TargetFrameworks>net48;net10.0</TargetFrameworks>
```

.NET ships every November: **even-numbered = LTS (3 years), odd-numbered = STS (18 months)**.
.NET 10 is the current LTS (Nov 2025 → ~Nov 2028). **.NET 8 and .NET 9 both go end of life on
10 November 2026** — `net8.0`/`net9.0` are now debt, as are `netcoreapp*`, `net5.0`–`net7.0`,
and anything below `net48`.

1. **LTS only.** Never target an STS release in a published library — an 18-month window means
   re-cutting the list every year and stranding someone each time. Never target a preview.
2. **`netstandard2.0` is permanent.** It is an API contract, not a runtime, so it cannot expire.
   It is the reach floor: consumable by .NET Framework 4.6.1+ (painless from 4.7.2 up) and by every
   .NET Core/5+ runtime. It is also the last .NET Standard version Framework supports —
   `netstandard2.1` deliberately excluded it.
3. **`net48` is conditional, not default.** `netstandard2.0` already reaches .NET Framework 4.8, so
   an explicit `net48` target earns its place only when the repo has a framework-conditional
   *dependency* or needs an API `netstandard2.0` does not expose. `ProphetsWay.EFTools` qualifies —
   its EF6 branch is keyed on `net4*`. Most repos do not. Justify it per repo.
   (.NET Framework 4.8 is the final Framework version; it ships as a Windows component and inherits
   the OS lifecycle, so it has no standalone EOL date.)
4. **Carry exactly one modern TFM** unless something concrete requires two. Every extra target
   multiplies build time.
5. **Test projects name runtimes directly**, since `netstandard2.0` cannot be a test target. A
   `net48` test target is how .NET Framework behavior is *verified*, which is distinct from a
   library merely *supporting* it: `Activator.CreateInstance<T>()` wraps a throwing constructor on
   .NET Framework and does not on .NET Core, so `ProphetsWay.Example.Tests` must keep `net48` or its
   exception-passthrough regression guard stops guarding anything.
6. **Canonical dotted monikers** — `net10.0`, never `net100`. The undotted form parses, but it is
   non-standard and inconsistent across the repos.
7. **`LangVersion`:** `netstandard2.0` defaults to C# 7.3, and that constraint applies to all shared
   code in a multi-targeted project. This is why nullable reference types do not work in these
   libraries regardless of what a csproj claims.
8. **Dropping `net8.0`/`net9.0` is not a breaking change** while `netstandard2.0` remains — those
   consumers still install and still resolve an asset.
9. **Adding a TFM is a MINOR bump, never a patch.** A new target silently repoints existing
   consumers to a *different assembly* — a .NET 10 consumer that resolved the `netstandard2.0` asset
   starts binding the `net10.0` one, a different compilation with different BCL bindings and no
   netstandard shims. A patch must be safe to take without reading the notes.

## Packaging Metadata

Required in every **published** library's `.csproj`. If a repo is not published to NuGet, these
are optional — but they become mandatory the moment publishing is on the table.

```xml
<PackageId>ProphetsWay.Thing</PackageId>
<Product>Thing</Product>
<Authors>G. Gordon Nasseri</Authors>
<Company>Prophet's Way</Company>
<Description>...</Description>
<RepositoryType>git</RepositoryType>
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
  ├─ repo-profile.md
  ├─ purpose-and-scope.md
  ├─ nuget-extraction-proposal.md
  └─ feature-requests.md    ← durable request and decision index
```

These artifacts are generated by agents and committed. `feature-requests.md` becomes applicable once
the first request is captured; an empty repo need not carry a placeholder.

**`docs/architecture.md` and per-project `docs/requirements.md` are `n/a` for a utility or reference
library** — its architecture lives in `AGENTS.md`, the README, and XML `<remarks>`. They apply to
multi-project **application** solutions only. Do not report either as missing from a library repo.

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
  | `Pipeline Engineer` | `.yml` / `.yaml` only — never versions, secrets, project files, or Markdown |
  | `Changelog Author` | `CHANGELOG.md` only |
  | `Threat Modeler`, `Security Reviewer` | `docs/security/` only — read-only on source |

  If an agent edits a test to make it pass, the workflow has failed. Report it rather than
  accepting the green build.
- **Never bump a version** in `app-variables.yml`. That is a human decision.
- **Never invent an Azure DevOps `definitionId`.** Badge URLs must be copied from a file that
  already exists in the repo. If one is missing, ask.
- **Feature requests are shared-capture, single-owner triage.** The owner or any agent may append a
  `Proposed` entry to `docs/feature-requests.md`, but must read the index first and extend an existing
  entry instead of duplicating it. Only `Purpose Refiner` may change status. Never delete or renumber
  entries; rejected requests remain with their reasoning, and new numbers increase monotonically.
- **A namespace change is a binary-breaking change.** Never make one casually; it requires a major
  version bump and a CHANGELOG entry.
- **Affirming an inherited claim is not verifying it.** Before restating any existing claim in a
  README, `AGENTS.md`, or doc as still accurate, open the artifact it describes. A claim that has
  survived several passes has been *copied* several times, not *checked* several times. Say which
  file you opened.
- **When a hygiene fix has a teaching cost, genericize rather than delete** — replace the
  machine-specific value, keep the artifact — and record the declined option in the
  `docs/feature-requests.md` entry so it is not re-proposed later as unfinished work.
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

**3.1.0 is a retarget plus documentation release.** No `.cs` file changed in it; the suite is
byte-identical to 3.0.0's, which is precisely what makes its passing the evidence for the retarget.

### Documents

| File | Owner | Contains |
|---|---|---|
| [docs/repo-profile.md](docs/repo-profile.md) | `Repo Analyst` | The evidence base — inventory, API surface, TFMs, packaging audit, README accuracy |
| [docs/purpose-and-scope.md](docs/purpose-and-scope.md) | `Purpose Refiner` | What this repo is for, and the scope bar everything is judged against |
| [docs/feature-requests.md](docs/feature-requests.md) | `Purpose Refiner` triages; anyone may append | Entries 1–8 — the durable record of what was considered and deliberately not built |

Numbering in `feature-requests.md` is **per-repository, starting at 1**, and does not correspond to
the index of the same name in `ProphetsWay.BaseDataAccess`.

### The Point It Proves

`ProphetsWay.Example.DataAccess` defines the domain and DAO interfaces. `ProphetsWay.Example.Tests`
runs against them. `ProphetsWay.Example.DataAccess.NoDB` is an in-memory implementation. In the
EFTools repo, `ProphetsWay.Example.DataAccess.EF` is a second implementation of the same contracts.
**The same tests pass against both** — that is the entire argument for the paradigm, and it is the
thing any documentation of this repo must lead with.

The claim is **currently pending, not false forever**: EFTools' submodule pointer is still pre-3.0.0,
so its EF implementation has not yet met the 3.x contracts. See deviation 1 below before repeating
the claim unqualified in prose meant for a reader.

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

TFMs as of **3.1.0** are `netstandard2.0;net10.0` for both DAL projects and `net48;net10.0` for the
tests — the current house standard, in canonical dotted form. `ProphetsWay.BaseDataAccess` reached the
same standard in its own 3.1.0, so this repo is **a** reference for the TFM convention rather than the
sole one. Copy either; do not copy the older repos.

**The library/test split is deliberate.** The DAL projects ship no `net48` asset, so the `net48` test
leg binds their `netstandard2.0` output — the exact assembly a .NET Framework consumer receives. That
leg exists to verify .NET Framework *behavior*: `Activator.CreateInstance<T>()` wraps a throwing
constructor there and does not on .NET Core, which is what the `ConventionShowcase`
exception-passthrough guard pins. Do not report the split as drift.

The suite runs 160 tests on each leg — **320 executions**. `ProphetsWay.BaseDataAccess` is consumed as
a NuGet `PackageReference` at **3.1.0**, never as a project reference.

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

### Coverage — What Is Demonstrated, and Where the Gaps Are Recorded

Demonstrated: `IDisposable` on the DAL, idempotent non-throwing `Dispose`, `ObjectDisposedException`
from every other member, disposal rolling back an open transaction, all three transaction members,
no nesting, per-instance transaction scope, unwrapped exception propagation (with an explicit
`ShouldNotBeOfType<TargetInvocationException>()` regression guard), and `Get<T>(null)` throwing
`ArgumentException` on a non-nullable value-type identifier.

**Four contract behaviors are deliberately *not* demonstrated here.** They were formerly restated in
this file; they are now entries **1–4 in [docs/feature-requests.md](docs/feature-requests.md)**, where
each carries a triage status and the reasoning behind it — two **Rejected**, one **Deferred**, one
**Proposed**.

**Read that index before reporting any coverage gap as a discovery. All four are known and decided.**
Do not copy their reasoning back into this file; ending that duplication is why they moved.

### Known Deviations

| # | Deviation | Notes |
|---|---|---|
| 1 | **`ProphetsWay.EFTools` consumes this repo as a git submodule** | **It is a submodule, not a vendored copy — earlier versions of this file said otherwise and were wrong.** `ProphetsWay.EFTools/.gitmodules` declares `path = ProphetsWay.Example`, `url = …/ProphetsWay.Example.git`, `branch = main`. The two therefore **cannot drift**; the submodule is simply *pinned*, currently at `967fd26`, **pre-3.0.0**. The real consequence is a **coordination requirement, not a duplication problem**: `ProphetsWay.Example.DataAccess.EF` implements a contract that no longer exists, and 3.1.0 puts it further behind still. Never edit files under `ProphetsWay.EFTools/ProphetsWay.Example/` — edit here and advance the pointer. Tracked as [FR 5](docs/feature-requests.md). |
| 2 | Has `app-variables.yml` / `local-pipeline.yml` despite not being published | **Correct and verified.** `PostTargetToNuGet` and `TargetProject` are both commented out; the pipeline builds and tests only. |
| 3 | Not packaged; packaging metadata is empty stubs | Correct and intentional. A teaching artifact is not a package — do not fill these in. `docs/nuget-extraction-proposal.md` is `n/a` here, not missing. |
| 4 | Visual Studio cannot open the SDK-style `.sqlproj` | A VS 2022/2026 limitation, not a defect. The migration is what makes `dotnet build` work on the solution at all. VS Code, SSMS 22, and the .NET CLI are fine; a VS user can unload the database project and work on the three C# projects normally. |

**Closed in 3.1.0, and not to be reinstated:** the inert `<NullableContextOptions>enable</NullableContextOptions>`
property is **gone** from `ProphetsWay.Example.DataAccess.csproj`, and the TFM lists are now at the
house standard. Do not re-add either as a deviation.

**Also closed in 3.1.0 — `ProphetsWay.Example.localhost.publish.xml` is not a deviation.** It is
committed and referenced as a `<None Include>` item in the `.sqlproj`, both deliberately: a teaching
repo benefits from shipping a working publish profile. Its connection string is now generic —
`Data Source=localhost`, Integrated Security, no credentials and no machine-specific value. Nothing
remains to fix. **Removing the file does not break the build — verified**, but there is no reason to.
Do not re-add this as a deviation after seeing the file exists.

Deviations previously listed here about duplicated projects drifting independently are **resolved
and were factually wrong**; do not reinstate them.

### Documentation Angle

When writing this repo's README, the reader is someone evaluating whether the BaseDataAccess
paradigm is worth adopting. Lead with the swap-the-DAL demonstration, not with a project listing.

Three things the current README needs, none of them applied yet — `README Author` owns that file:

- An onboarding note that Visual Studio cannot load the SDK-style database project, and what to do
  about it ([FR 7](docs/feature-requests.md)).
- A correction to the claim that EFTools runs the same suite unchanged — it is pending on EFTools
  advancing its submodule pointer, not wrong forever.
- Its stale build facts: it still says the projects target `net8.0`/`net9.0` and reference
  `ProphetsWay.BaseDataAccess` 3.0.0, and that the suite runs on three legs. See the README accuracy
  table in [docs/repo-profile.md](docs/repo-profile.md).

`CHANGELOG.md` has no v3.1.0 entry yet; that belongs to `Changelog Author`.
