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

As of **3.0.0** it is also an **executable specification**: `IExampleDataAccess` carries DAL-wide
contract rules in its `<remarks>`, and the test suite is partitioned by a `Scope` trait so a newly
written DAL can run the subset it is actually bound by. **There are four such rules as of
2026-08-16** — SNAPSHOT, ORDERING, IDENTIFIER and ROW COUNT. Earlier text here said two; that was
accurate at 3.1.0 and is not now.

**3.1.0 is a retarget plus documentation release.** No `.cs` file changed **in the release**, and the
suite it shipped is byte-identical to 3.0.0's — which is precisely what makes its passing the evidence
for the retarget. **That is a fact about v3.1.0 as shipped, not about the working tree.** Three `.cs`
files have changed since it shipped — `SnapshotDeepCopyTests.cs` and `UserDaoTests.cs`, retraited and
split on 2026-08-16 and then extended with a test each, and `IExampleDataAccess.cs`, which gained the
IDENTIFIER and ROW COUNT rules the same day — so the tree now carries **164 tests** where 3.1.0
shipped 160. Do not repeat the byte-identical claim against the current tree, and do not restate the
released 3.1.0 as anything other than what it was.

**HEAD is `f93f0a4`, and it added a mechanism rather than a test.** See *The Capability Declaration*
below. The suite total is **unchanged at 164** — re-counted 2026-08-23 by grepping every `[Fact]`,
`[Theory]` and `[InlineData]` in `ProphetsWay.Example.Tests/`, which returns **149 `[Fact]` + 6
`[Theory]` carrying 15 `[InlineData]` = 164 cases**, not carried from the previous figure. The version
line is still **3.1.1** — `app-variables.yml` reads `Major: '3'` / `Minor: '1'` / `Patch: '1'`, opened
2026-08-23. Never change it.

### Documents

| File | Owner | Contains |
|---|---|---|
| [docs/repo-profile.md](docs/repo-profile.md) | `Repo Analyst` | The evidence base — inventory, API surface, TFMs, packaging audit, README accuracy |
| [docs/purpose-and-scope.md](docs/purpose-and-scope.md) | `Purpose Refiner` | What this repo is for, and the scope bar everything is judged against |
| [docs/feature-requests.md](docs/feature-requests.md) | `Purpose Refiner` triages; anyone may append | Entries **1–17** — the durable record of what was considered and deliberately not built. **The figure of "1–12" this row carried until 2026-08-23 is superseded**; five entries have been filed since, and 5, 11, 13 and 17 are now `Done` |

Numbering in `feature-requests.md` is **per-repository, starting at 1**, and does not correspond to
the index of the same name in `ProphetsWay.BaseDataAccess`.

### The Point It Proves

`ProphetsWay.Example.DataAccess` defines the domain and DAO interfaces. `ProphetsWay.Example.Tests`
runs against them. `ProphetsWay.Example.DataAccess.NoDB` is an in-memory implementation. In the
EFTools repo, `ProphetsWay.Example.DataAccess.EF` is a second implementation of the same contracts.
**The same tests pass against both** — that is the entire argument for the paradigm, and it is the
thing any documentation of this repo must lead with.

**As of 2026-08-23 the claim is no longer pending — it is demonstrated.** Every hedge this section
carried is dead. `ProphetsWay.EFTools` pins this repository at **`f93f0a4`**, its own tip, and runs
this suite against `ProphetsWay.Example.DataAccess.EF` to **270 total / 270 passed / 0 failed**, with
the conformance gate `--filter "Scope=Contract|Guard=Seam"` at **245 / 245 / 0** in about two seconds.
`Purpose Refiner` closed [FR 5](docs/feature-requests.md) as **Done — 2026-08-23** on that basis.
**Do not restate "151 tests, 123 passed, 28 failed", and do not restate that `CompanyResourceDao`
does not exist** — `ProphetsWay.EFTools/ProphetsWay.Example.DataAccess.EF/Daos/CompanyResourceDao.cs`
was located by file search on 2026-08-23. What remains is release sequencing, not correctness: the
pointer sits on an untagged commit, and D11 step 3 wants it on a tag. See deviation 1.

`ProphetsWay.Example.Tests/TestDataAccessFactory.cs` is the **only** file in the suite that names a
concrete implementation — re-verified 2026-08-16 by a repo-wide search for `new ExampleDataAccess`,
which matches once, in `Create`. Changing that single `return` repoints all 164 tests. Do not
introduce a second construction site — doing so silently destroys the property this repo exists to
demonstrate.

### The Capability Declaration — new in `f93f0a4`, and easy to get backwards

A consuming repository now states **two** things in the one `Use` call: which implementation the suite
runs against, and **what its store is structurally able to do**. Read from
`ProphetsWay.Example.Tests/StoreCapabilities.cs` and `TestDataAccessFactory.cs` on 2026-08-23.

| Piece | What it is |
|---|---|
| `StoreCapabilities` | A `[Flags] public enum` — `None = 0`, `DenormalizedNavigationWrites = 1`, `TransactionIsolation = 2` |
| `TestDataAccessFactory.InMemoryStore` | A `public const StoreCapabilities` equal to `DenormalizedNavigationWrites` — `.NoDB`'s actual shape, named so a reader meeting the default is told what it *is* rather than which flags it sets |
| `TestDataAccessFactory.Use(Func<IExampleDataAccess>, StoreCapabilities)` | The second parameter, **defaulting to `InMemoryStore`** |
| `TestDataAccessFactory.Capabilities` | Read under the same lock, at execution time, by **exactly two** tests and nothing else |

**This repository declares nothing, and its own suite is unchanged.** The default is its own shape, so
no call site here moved and no test was added — which is why the total is still 164.

**The nuance that inverts if restated from memory:** the flags are **positive capabilities on two
independent axes**, not a store-kind label. A normalized relational DAL declares `TransactionIsolation`
**only**, and the *absence* of `DenormalizedNavigationWrites` is itself a positive declaration — such a
store holds one Companies row read back through a join and physically cannot hold a second,
denormalized copy of a navigation node. **The branch that absence selects is the *stronger* assertion**:
it proves an `Update` did not cascade into the Company, Job and Department rows the caller never named.
Adding the flag to "fix" a relational failure would assert an in-memory outcome against a relational
store.

The two readers were **renamed** to say what they assert rather than what their setup does:

- `SnapshotDeepCopyTests.ShouldReadANavigationPropertyEditBackOnlyWhereTheStoreDenormalizesTheWrite`
- `DataAccessTransactionTests.ShouldExposeUncommittedWritesToAnotherInstanceOnlyWhereTheStoreDoesNotIsolateThem`

In the second, **the branch is around the read, not around the `ShouldBe`** — against an isolating
store the uncommitted read does not fail, it *waits* on the lock until the provider's command timeout,
so skipping only the assertion would leave the whole cost in place. Removing that read is what took
EFTools' full run from roughly thirty-two seconds to about two.

**No `Scope=Contract` assertion reads the enum, and none may be made to.** A contract obligation a
caller can decline is not one. Reasoning and limits: the `<remarks>` on `StoreCapabilities` itself and
[FR 17](docs/feature-requests.md), `Done — 2026-08-23`.

### Layout

| Project | Role |
|---|---|
| `ProphetsWay.Example.DataAccess/` | Domain — `Entities/`, `IDaos/`, `Enums/`, `IExampleDataAccess` |
| `ProphetsWay.Example.DataAccess.NoDB/` | In-memory DAL implementation (`DataStore`, `StoreTable`, `StoreList`, `TransactionLog`, `Daos/`) |
| `ProphetsWay.Example.Database/` | SQL database project — SDK-style `Microsoft.Build.Sql/2.2.0` |
| `ProphetsWay.Example.Tests/` | xUnit + Shouldly, 164 tests written against the interfaces, not an implementation |

TFMs as of **3.1.0** are `netstandard2.0;net10.0` for both DAL projects and `net48;net10.0` for the
tests — the current house standard, in canonical dotted form. `ProphetsWay.BaseDataAccess` reached the
same standard in its own 3.1.0, so this repo is **a** reference for the TFM convention rather than the
sole one. Copy either; do not copy the older repos.

**The library/test split is deliberate.** The DAL projects ship no `net48` asset, so the `net48` test
leg binds their `netstandard2.0` output — the exact assembly a .NET Framework consumer receives. That
leg exists to verify .NET Framework *behavior*: `Activator.CreateInstance<T>()` wraps a throwing
constructor there and does not on .NET Core, which is what the `ConventionShowcase`
exception-passthrough guard pins. Do not report the split as drift.

The suite runs 164 tests on each leg — **328 executions**. `ProphetsWay.BaseDataAccess` is consumed as
a NuGet `PackageReference` at **3.2.0**, never as a project reference — read from
`ProphetsWay.Example.DataAccess.csproj` line 29 on 2026-08-23, which carries the only
`PackageReference` to it. **Any statement of `3.1.0` anywhere in this workspace is superseded**; the
bump landed on 2026-08-23 and brought the nullable annotations that let `ProphetsWay.EFTools` delete
ten `CS8766` suppressions.

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
- **The identifier rule**, added to `IExampleDataAccess` on 2026-08-16 and binding on every DAO whose
  entity has an identifier: `Insert` assigns the generated identifier back onto the caller's instance.
  It is the one write-back the snapshot rule's closing sentence anticipates.
  `ICompanyResourceDao` is outside it — there is no identifier to assign, which is the absence of one
  rather than a refusal of this one.
- **The row count rule**, added the same day and binding on every DAO: `Update` and `Delete` return
  the number of rows the call matched and wrote. `IDepartmentDao`'s soft-delete rules and
  `ICompanyResourceDao`'s rule 4 are that rule applied to a narrower notion of a match, not
  exceptions to it. Both new rules are conventions **elected here**, not changes to
  `ProphetsWay.BaseDataAccess`.
- **Transactions are scoped to the DAL instance, not the store.** `TransactionLog` is an instance
  field on `ExampleDataAccess` deliberately. Moving it to `DataStore` would let one instance roll
  back another's writes.
- **`Dispose` releases what the instance created and nothing else.** It never clears `DataStore` —
  disposing a DAL no more empties the database than closing one connection does. Every test in the
  suite disposes an instance, so a store-clearing `Dispose` would delete rows out from under
  concurrently running tests.
- **The `Scope` trait partition is load-bearing**, and every test still carries exactly one — verified
  by static count of every `[Fact]`, `[Theory]`, `[InlineData]` and `[Trait("Scope", …)]` in the test
  project on 2026-08-16. `Contract` (139) is what any conforming implementation must pass;
  `Characterization` (5) pins choices this implementation made that the contract does not require;
  `Dispatcher` (20) exercises the reflection convention in `ProphetsWay.BaseDataAccess` itself and
  belongs to no DAL. **They sum to the suite total, 164, and that sum is the check** — a mismatch
  means a test is untraited or double-traited, which is the defect the partition exists to prevent.
  Adding a test without a trait breaks `dotnet test --filter "Scope=Contract"` as a usable gate.
  **Any figure of 138 / 4 / 20 of 162, or 160, or 324 executions, is superseded** — two tests were
  added on 2026-08-16, closing a gate hole where a cascading `Update` had been passing all 138
  `Contract` tests.
- **Four classes declare `Scope` per method rather than per class** — `CompanyDaoTests`,
  `DataAccessTransactionTests`, `SnapshotDeepCopyTests`, `UserDaoTests`. Each mixes scopes, and
  **xUnit accumulates traits rather than letting a method override its class**, so a class-level
  `Contract` on any of them would leave its `Characterization` test selected by
  `--filter "Scope=Contract"` no matter what the method declared. Never put a class-level `Scope` on
  a class with mixed scopes.
- **A `Scope=Contract` assertion must trace to a stated rule.** If no interface, XML `<remarks>`, or
  numbered DAO rule states the behavior being asserted, the assertion is `Characterization`, not
  `Contract`. `--filter "Scope=Contract"` is offered as *the* conformance gate, so every `Contract`
  assertion is an obligation placed on every future implementer; making one in the name of a
  specification that does not make it is this repo failing at its stated job. Two were found and
  retraited on 2026-08-16 — one requiring a row shape only a denormalizing store can produce, one
  pinning a `private const` of the in-memory implementation. **There is no enforcement mechanism**:
  no analyzer, no `.editorconfig` rule, no CI check on test metadata. It is a review convention that
  holds only because whoever writes a `Contract` test asks the question, and it is a filter rather
  than a proof — it catches an assertion nothing states, not an assertion that over-reads something
  real. Reasoning and limits:
  [FR 12](docs/feature-requests.md#12--a-traceability-rule-for-contract-scoped-assertions).
- **The `ConventionShowcase/` DALs are deliberately mis-wired.** They are the subject under test,
  not the implementation under test, and they construct themselves rather than using the factory.
  Do not "fix" them. They are also outside the capability mechanism: they construct their own DALs and
  carry no dependency on `TestDataAccessFactory`, so `Use` does not and must not change what they
  exercise.
- **A `Characterization` test may branch on a declared store capability; a `Contract` test may not.**
  See *The Capability Declaration* above. This is the answer to the question
  [FR 12](docs/feature-requests.md#12--a-traceability-rule-for-contract-scoped-assertions) leaves
  open — what becomes of an observation two correct implementations must answer differently.

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
| 1 | **`ProphetsWay.EFTools` consumes this repo as a git submodule** — ~~*and the EF run is not green*~~ **that half is CLOSED, 2026-08-23** | **It is a submodule, not a vendored copy.** `ProphetsWay.EFTools/.gitmodules` declares `path = ProphetsWay.Example`, `url = …/ProphetsWay.Example.git`, `branch = main`. The two therefore **cannot drift**; the submodule is simply *pinned*. **The pointer is `f93f0a4`** — `f93f0a41a76834647962ddf9e830e01e24e05f24`, read from `ProphetsWay.EFTools/.git/modules/ProphetsWay.Example/HEAD` on **2026-08-23**, and it is **this repository's own tip**. `git submodule status` describes it as `3.1.0-4-gf93f0a4`: four commits past that tag, on the open, untagged **3.1.1** line. **Every earlier pointer this row has carried — `967fd26`, `d845863`, `61d9e7d` — is superseded and must not be restated.** **The blocker this row existed to record is gone.** `ProphetsWay.EFTools.Tests` adapts 13 of this repo's 15 test classes and points them at `ProphetsWay.Example.DataAccess.EF` through a `[ModuleInitializer]` calling `TestDataAccessFactory.Use(() => Constants.GetExampleDataAccess, StoreCapabilities.TransactionIsolation)` — read from `ProphetsWay.EFTools/ProphetsWay.EFTools.Tests/TestSeam.cs` line 53. That run is **270 total / 270 passed / 0 failed**, conformance gate **245 / 245 / 0** in about two seconds. **Three claims this row used to make are dead and must never be restated:** *"151 tests, 123 passed, 28 failed"*; *"the failures dominated by `CompanyResourceDao`, which does not exist"* — `ProphetsWay.EFTools/ProphetsWay.Example.DataAccess.EF/Daos/CompanyResourceDao.cs` was located by file search on 2026-08-23; and *"both EFTools `.csproj` files reference `ProphetsWay.BaseDataAccess` 3.1.0"* — they read **3.2.0**, as does this repo's own contracts project. [FR 5](docs/feature-requests.md) is **`Done — 2026-08-23`**. **What is left is release sequencing, not correctness:** the pointer sits on an *untagged* commit, and D11 step 3 wants it on a tag of this repository. Never edit files under `ProphetsWay.EFTools/ProphetsWay.Example/` — edit here and advance the pointer. |
| 2 | Has `app-variables.yml` / `local-pipeline.yml` despite not being published | **Correct and verified**, re-opened 2026-08-23. `PostTargetToNuGet` and `TargetProject` are both commented out; `HasSqlProj: 'yes'`, `Product: 'Example'`, `RepoName: 'ProphetManX/ProphetsWay.Example'`. The pipeline builds and tests only. |
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

**The Visual Studio note landed and is still there** — re-verified by opening `README.md` on
2026-08-16: it says VS 2022/2026 cannot open an SDK-style `.sqlproj` and points at *Building & Testing
Locally*. [FR 7](docs/feature-requests.md) is still filed `Proposed`; only `Purpose Refiner` may close
it.

**The README defects this section used to list are fixed — re-grepped on 2026-08-23, not assumed.**
`README Author` ran after the analyst pass that recorded them and corrected the file; **no open README
defect is known.** Line 41 names the pointer **`f93f0a4`** and calls it four commits past the `3.1.0`
tag; lines 44 and 402 both read `ProphetsWay.BaseDataAccess` **3.2.0**; the "under way and **not
finished** / throwing stubs" framing is gone, replaced by the **270 / 270 / 0** run, the **245 / 245 / 0**
gate and [FR 5](docs/feature-requests.md) closed `Done` — in the `.NoDB` blurb as well as the EFTools
paragraph; line 138 states the two-parameter `TestDataAccessFactory.Use(Func<IExampleDataAccess>,
StoreCapabilities)`; line 173 says `ProphetsWay.EFTools.Tests` carries **thirteen** adapters, empty by
design so xUnit discovers the suite; and lines 277 and 283 carry the post-`f93f0a4` `Characterization`
test names, which now pass by branching. **Do not restate the old defect list as outstanding, and do not
re-report `61d9e7d` or a `3.1.0` reference as present in that file.**

**What is still accurate and must not be "corrected":** the trait table, now at lines 229–233, reads
`Contract` 139 / `Characterization` 5 / `Dispatcher` 20, and lines 374–375 and 793 read 164 tests and
328 executions on `net48` and `net10.0`. All of it was **re-derived** on 2026-08-23 by grepping every
`[Fact]`, `[Theory]`, `[InlineData]` and `[Trait("Scope", …)]` in the test project — 149 facts plus 15
inline cases is 164, and 139 + 5 + 20 is 164. The counts did not move because `f93f0a4` added a
mechanism, not a test. **The anchors moved in the rewrite; the 201–203 / 205 / 292–293 / 709 citations
this section used to carry are superseded.**

**`CHANGELOG.md` does have a v3.1.0 entry, and a v3.1.1 one — correcting a false claim made here earlier
the same day.** This paragraph previously read "`CHANGELOG.md` has no v3.1.0 entry yet." **That was
carried rather than checked, and it is false.** The file was opened on 2026-08-23 and its `^# v` headings
read directly: **`# v3.1.1 — not yet released` at line 1, `# v3.1.0` at line 264, `# v3.0.0` at line 329.**
The `v3.1.1` entry is open by design — the owner will not tag the line until `ProphetsWay.EFTools` has
what it needs — and it documents the two-parameter
`TestDataAccessFactory.Use(Func<IExampleDataAccess>, StoreCapabilities = InMemoryStore)` seam and the
`StoreCapabilities` `[Flags]` enum, records that this repository declares nothing so its counts stand at
164 with the partition at 139 / 5 / 20, and states that a one-parameter `Use` call still compiles.
`Changelog Author` owns that file; nothing here edits it. It must **not** be back-edited for the trait
counts — the 138 / 2 / 20 figure under its `v3.0.0` heading was accurate at that release.
