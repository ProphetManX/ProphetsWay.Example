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

### The Point It Proves

`ProphetsWay.Example.DataAccess` defines the domain and DAO interfaces. `ProphetsWay.Example.Tests`
runs against them. `ProphetsWay.Example.DataAccess.NoDB` is an in-memory implementation. In the
EFTools repo, `ProphetsWay.Example.DataAccess.EF` is a second implementation of the same contracts.
**The same tests pass against both** — that is the entire argument for the paradigm, and it is the
thing any documentation of this repo must lead with.

### Layout

| Project | Role |
|---|---|
| `ProphetsWay.Example.DataAccess/` | Domain — `Entities/`, `IDaos/`, `Enums/`, `IExampleDataAccess` |
| `ProphetsWay.Example.DataAccess.NoDB/` | In-memory DAL implementation (`DataStore`, `Daos/`) |
| `ProphetsWay.Example.Database/` | SQL database project |
| `ProphetsWay.Example.Tests/` | xUnit tests written against the interfaces, not an implementation |

### Domain Model

`Company`, `Job`, `Resource`, `Transaction`, `User`, plus `Roles`. Each has a matching `I*Dao` and
a DAO implementation per DAL. Adding an entity means touching all of these — and the mirrored copy
inside the EFTools repo.

Namespaces follow the folder structure: `ProphetsWay.Example.DataAccess.Entities`,
`.IDaos`, `.Enums`, `.DataAccess.NoDB.Daos`, `.Tests`.

### Known Deviations

| # | Deviation | Notes |
|---|---|---|
| 1 | **This repo is duplicated inside `ProphetsWay.EFTools`** | `ProphetsWay.EFTools/ProphetsWay.Example/` contains a copy of `ProphetsWay.Example.DataAccess`, `.DataAccess.NoDB`, and `.Tests`. The two copies drift independently. **Before editing anything here, check whether the same edit is needed in the EFTools copy.** This is the largest open architectural question in the workspace. |
| 2 | Has `app-variables.yml` / `local-pipeline.yml` despite not being published | Correct if the pipeline is only building and testing. Verify `PostTargetToNuGet` is not `'yes'`. |
| 3 | Not packaged | Correct and intentional. Packaging metadata is not required here. |

### Documentation Angle

When writing this repo's README, the reader is someone evaluating whether the BaseDataAccess
paradigm is worth adopting. Lead with the swap-the-DAL demonstration, not with a project listing.
