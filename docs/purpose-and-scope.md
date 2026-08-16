# Purpose & Scope — ProphetsWay.Example

_Written 2026-08-15, against the working tree at v3.1.0, immediately after the `Modernizer` retarget pass.
**Amended 2026-08-16** for the two `Scope` retraits — the trait counts, the suite size, and the status of the
README items are the amended values and were re-measured rather than carried forward.
The factual base — project inventory, API surface, test counts, packaging audit — is
[docs/repo-profile.md](repo-profile.md) and is not re-derived here; where this document and that one disagree,
the disagreement is called out explicitly under [Stale Claims — Corrected in 3.1.0](#stale-claims--corrected-in-310).
Feature requests and deferred decisions live in [docs/feature-requests.md](feature-requests.md) and are cited
here by number._

**Verified current state**, now also reflected in `AGENTS.md` and `repo-profile.md`:

| Thing | Value |
| --- | --- |
| `ProphetsWay.Example.DataAccess`, `.DataAccess.NoDB` | `netstandard2.0;net10.0` |
| `ProphetsWay.Example.Tests` | `net48;net10.0` |
| `ProphetsWay.BaseDataAccess` reference | `3.1.0` (NuGet `PackageReference`) |
| Suite | 164 tests × 2 legs = **328 executions** |
| Version | 3.1.1 in [app-variables.yml](../app-variables.yml) |

---

## The Argument This Repository Exists To Make

Everything below is subordinate to one claim, and the claim is not about features:

> `ProphetsWay.Example.DataAccess` defines the entities and the DAO interfaces.
> `ProphetsWay.Example.Tests` is written against those interfaces and nothing else.
> `ProphetsWay.Example.DataAccess.NoDB` is an in-memory implementation of them.
> In the `ProphetsWay.EFTools` repository, `ProphetsWay.Example.DataAccess.EF` is a *completely different*
> implementation of the same contracts, over Entity Framework and a real database.
> **The same test suite passes against both.**

That is the entire argument for the `ProphetsWay.BaseDataAccess` paradigm. Every design decision in this
repository is answerable to it, and any change that weakens it is a scope violation no matter how useful the
change is in isolation.

### The load-bearing invariant

[ProphetsWay.Example.Tests/TestDataAccessFactory.cs](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs) is
the only file in the test project that names a concrete implementation. One `return` repoints all 164 tests:

```csharp
public static IExampleDataAccess Create()
{
	//>>> The one line to change to point this suite at another implementation. <<<
	return new ExampleDataAccess();
}
```

**Treat this as an invariant, not a convenience.** A second construction site anywhere in the suite does not
merely add work when swapping implementations — it silently destroys the property the repository exists to
demonstrate, because the swap stops being provably total. `CreateAs<T>` exists so that the *one* site can
serve test classes closed over a DAO interface rather than the aggregate; it is an extension of the invariant,
not an exception to it.

The two documented exceptions are correct and must stay exceptions: the `ConventionShowcase/` DALs construct
themselves, because each is deliberately mis-wired and is therefore the *subject* of its test rather than the
implementation under test.

---

## Proposed One-Sentence Purpose

A worked domain, one in-memory Data Access Layer, and a 164-test suite written against interfaces only —
existing to prove, by being read and then re-pointed at a different implementation in one line, that the
`ProphetsWay.BaseDataAccess` paradigm actually decouples business logic from data access.

## Current Purpose (as implied by README/csproj)

**The csproj implies nothing at all, correctly.** Every packaging element in both DAL projects is an empty
self-closing stub, `PostTargetToNuGet` and `TargetProject` are commented out in
[app-variables.yml](../app-variables.yml), and the test project sets `<IsPackable>false</IsPackable>`. There
is no `<Description>` to drift from. This repository states its purpose in prose, not in metadata.

The README states it accurately and leads with the right thing — a problem statement about `DbContext` in
business logic, then the swap demonstration, then the `Scope` trait table, then the project inventory. It is
in better shape than most released libraries' READMEs.

## The Drift

Unusually for this workspace, **there is no drift between stated and real purpose.** The README, the
`<remarks>` on [IExampleDataAccess](../ProphetsWay.Example.DataAccess/IExampleDataAccess.cs), and the code all
say the same thing. The four findings below are drift of a different kind — between the documentation and the
current state of the tree, between the specification and what the suite actually demands, and between the
headline claim and the state of a sibling repository.

1. **The README's central claim is currently false in practice, through no fault of this repository.**
   "`ProphetsWay.EFTools` carries an Entity Framework implementation of the very same `IExampleDataAccess`
   contract, and the tests do not change to accommodate it" is true of `main` at the 3.0.0 branch point and is
   *not* true today: EFTools consumes this repository as a **git submodule pinned** to that older commit, so
   `ProphetsWay.Example.DataAccess.EF` implements a contract that no longer exists here — no `Dispose`, no
   `Department`, no `CompanyResource`, no snapshot rule. This is a **coordination requirement, not a
   duplication problem**, and it is the highest-consequence open fact about the repository. Captured as
   [feature request 5](feature-requests.md#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts).

2. **The demonstrated half of `Get<T>(null)` is presented as though it were the whole rule.** `IBaseDataAccess`
   specifies a deliberate split — `ArgumentException` where the identifier is a non-nullable value type,
   acceptance where it is a reference type or a nullable value type. Every entity here keys on `int`, `long`,
   or `Guid`, so a reader can only see the throwing half and could reasonably conclude the throw is
   unconditional. In a repository whose product is comprehension, teaching half of a documented split *is*
   drift. Captured as [feature request 1](feature-requests.md#1--demonstrate-the-accepting-half-of-gettnull).

3. ~~**The README still describes a tree that no longer exists**~~ — **fixed since this was written**,
   re-verified against `README.md` on 2026-08-16. `CHANGELOG.md` still has no `v3.1.0` entry.
   `AGENTS.md` and [docs/repo-profile.md](repo-profile.md) carried the same drift and were corrected
   during 3.1.0 — see [Stale Claims — Corrected in 3.1.0](#stale-claims--corrected-in-310) and the
   [Still Open](#still-open--owned-by-other-agents) table beneath it.

4. **The `Contract` scope over-claimed, in two places. Both are now corrected.** Two assertions
   were marked `Scope=Contract` — binding every conforming DAL — while actually encoding choices of the
   in-memory store: one requiring a row shape only a denormalized store can produce, one pinning a string
   literal that `IUserDao`'s `<remarks>` explicitly declines to specify. Since
   `dotnet test --filter "Scope=Contract"` is offered as *the* conformance gate, an over-claiming `Contract`
   scope is the repository failing at its stated job, and it was the sharpest drift on this list. Both were
   retraited on 2026-08-16, each by **splitting** the test rather than demoting it — the genuinely
   contractual half stayed in `Contract` and a new `Characterization` sibling took the over-claim, which is
   why the suite total rose from 160 to 162 while `Contract` held at 138. Captured with the full reasoning
   — and with the decision that `.NoDB` stays — as
   [feature request 11](feature-requests.md#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays).

   **Both were found by someone attempting a second implementation with different physical constraints**, which
   is the argument for [FR 5](feature-requests.md#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts)
   being worth more than its face value: advancing the EFTools submodule pointer is also this repository's audit
   of its own `Contract` scope, and it should be expected to find more.

---

## The Scope Bar for This Repository

A normal library asks *is this feature useful*. That is the wrong question here, and applying it is how a
teaching artifact rots.

> **The bar: does this earn its place in something meant to be read end to end?**

This repository's product is **clarity**. Its budget is not build time or binary size — it is the reader's
working memory. A demonstration that adds a corner case at the cost of the domain no longer fitting in a
reader's head is a **net loss**, even when the corner case is real, even when the code is correct, and even
when it would raise coverage of the paradigm. Seven entities is already near the ceiling; an eighth added
purely to illustrate one behaviour has to justify itself against every reader who now has one more thing to
hold while trying to follow the main argument.

Three corollaries follow, and they govern the triage of every entry in
[docs/feature-requests.md](feature-requests.md):

1. **Prefer the test project over the domain.** `ConventionShowcase/` already hosts entity and DAL types that
   exist only to be read and to fail in a specific way — `NoIdentifierEntity`, `GetOnlyIdentifierEntity`,
   `PrivateSetterIdentifierEntity`. A behaviour of `ProphetsWay.BaseDataAccess` demonstrated there costs the
   reader of the *domain* nothing, and lands in the `Dispatcher` scope where it correctly binds nobody's DAL.
   Adding an entity to `ProphetsWay.Example.DataAccess` costs contracts, a DAO interface, a NoDB
   implementation, database schema, the EF mirror in another repository, and permanent reader attention.

2. **A behaviour of the base library is not a behaviour of this domain.** `Get<T>(null)`, identifier
   resolution, and exception passthrough belong to `ProphetsWay.BaseDataAccess`. This repository demonstrates
   them as a service to a reader; it is not obliged to model them in the domain.

3. **Completeness of a 2×2 is not a reason.** "The soft × keyless corner is empty" is an observation, not an
   argument. The corner earns a demonstration only if someone would plausibly build it.

---

## Cohesion Map

Extraction is not the question this repository faces — see [Publication](#publication-and-the-nuget-extraction-proposal)
below — so the columns that matter are the dependency directions that keep the argument honest.

| Cluster | Types | Depends on | Depended on by | Extraction candidate? |
| --- | --- | --- | --- | --- |
| Entities | `Company`, `Job`, `User`, `Transaction`, `Resource`, `Department`, `CompanyResource`, `BaseEntity`, `Roles` | `ProphetsWay.BaseDataAccess` markers only | Every other cluster | **No** — it is the domain being taught |
| DAO contracts | `ICompanyDao`, `IJobDao`, `IUserDao`, `ITransactionDao`, `IResourceDao`, `IDepartmentDao`, `ICompanyResourceDao` | Entities, `BaseDataAccess` capability interfaces | `IExampleDataAccess`, NoDB, tests, EF (other repo) | **No** |
| Aggregate contract | `IExampleDataAccess` | The seven DAO contracts, `IBaseDataAccess` | NoDB, tests, EF (other repo) | **No** — it *is* the swap seam |
| In-memory implementation | `ExampleDataAccess`, `DataStore`, `StoreTable<,>`, `StoreList<>`, `TransactionLog`, 7 internal DAOs | The two clusters above | Tests, via the factory only | **No** — one of two implementations of a demonstration |
| Test suite | `BaseUnitTests<T>`, `TestDataAccessFactory`, 12 test classes | Contracts only, plus one factory line | Nothing | **No** |
| Convention showcase | `ShowcaseDataAccess` + 8 deliberately mis-wired DALs and their local entities | `ProphetsWay.BaseDataAccess` directly | Nothing | **No** — subject under test, not implementation under test |
| Database schema | `ProphetsWay.Example.Database` (`Microsoft.Build.Sql/2.2.0`) | Nothing in code | Nothing in code | **No** — the shape a real DAL would target |

**The one structural fact worth stating:** the contracts project has exactly one outward edge —
`ProphetsWay.BaseDataAccess` 3.1.0 — and no reference of any kind to an implementation. There is no
`DbContext`, no `SqlConnection`, no `HttpContext`, and no mention of `NoDB` anywhere in it. The seam holds,
and it holding is the precondition for the entire argument. Any future change that puts an implementation type
in the contracts project's public surface fails the scope gate automatically.

---

## In Scope

- The seven-entity domain, its DAO interfaces, and `IExampleDataAccess`, held at a size a reader can carry.
- Exactly **one** in-memory implementation in this repository. The second implementation lives in EFTools by
  design — the argument requires two implementations in *different* repositories with *different* storage, not
  two in this one. **Settled 2026-08-15**, against a proposal to add or relocate a second one here:
  [FR 10](feature-requests.md#10--a-second-data-access-layer-implementation-in-this-repository--sqlite-mssql-or-relocating-the-entity-framework-one).
- **`ProphetsWay.Example.DataAccess.NoDB` specifically**, and not a SQLite-backed replacement for it. A
  dictionary versus a relational engine is the radical difference the claim rests on; two relational engines is
  a configuration change. Recorded with its reasoning in
  [FR 11](feature-requests.md#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays).
- The behavioural specifications in XML `<remarks>` — the snapshot, ordering, identifier and row count rules
  on `IExampleDataAccess`, the 19 numbered rules on `IDepartmentDao`, the 10 on `ICompanyResourceDao`. These
  are the executable specification, and they are the source of truth for behaviour; prose elsewhere indexes
  them and must not restate them. The identifier and row count rules were added on 2026-08-16; text naming
  only the first two is superseded.
- The `Scope` trait partition — `Contract` / `Characterization` / `Dispatcher` — and the honesty it enforces. A
  suite claiming total portability would be hiding the places a conforming implementation is allowed to differ.
  **The split is 139 / 5 / 20 of 164**, measured by static trait count against every `[Fact]`, `[Theory]`
  and `[InlineData]` in the test project on 2026-08-16. It was 138 / 2 / 20 of 160 until two mis-scoped
  assertions were retraited; because each retrait **split** its test rather than demoting it, `Contract`
  held at 138 and the total rose to 162. Two further tests were then added — closing a gate hole where a
  cascading `Update` had been passing every `Contract` test — and `ShouldCallCustomUserFunctionality` moved
  to `Characterization`, giving the current figures. An earlier figure of 137 / 3 / 20 recorded in this
  document was a mid-edit reading of a moving tree and never described a committed state — it is superseded,
  not disputed.
  Reasoning in
  [FR 11](feature-requests.md#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays).
- **A `Scope=Contract` assertion must trace to a stated rule** — an interface, a `<remarks>`, or a numbered
  DAO rule. If nothing states it, the assertion is `Characterization`. The canonical statement of this rule
  lives in `AGENTS.md` beside the `Scope` partition, which is the text every agent loads; it is not restated
  here. Reasoning and limits:
  [FR 12](feature-requests.md#12--a-traceability-rule-for-contract-scoped-assertions).
- `ConventionShowcase/` as the home for base-library behaviours that a reader benefits from seeing but that
  the domain should not carry.
- The SQL database project, as the schema a database-backed implementation of these contracts would target.

## Out of Scope (and where it should live instead)

| Not this repository's job | Where it belongs |
| --- | --- |
| A second in-repo DAL implementation (MSSQL, Dapper, …) | Its own repository consuming this one, exactly as `ProphetsWay.EFTools` does. Two implementations side by side here would make the swap look like a local convenience rather than a cross-repository property. **Asked and answered in four variants — including replacing `.NoDB` with SQLite and relocating `ProphetsWay.Example.DataAccess.EF` into this repository — all Rejected: [FR 10](feature-requests.md#10--a-second-data-access-layer-implementation-in-this-repository--sqlite-mssql-or-relocating-the-entity-framework-one)** |
| A published, reusable conformance test kit | `ProphetsWay.BaseDataAccess` — recorded as entry 1 in [its feature-request index](../../ProphetsWay.BaseDataAccess/docs/feature-requests.md), deferred until a second real implementation exists. This suite is *shaped* like one, which is precisely why the temptation to publish it should be resisted here |
| Rules about `IBaseDataAccess` disposal, transactions, or the reflection convention | The `<remarks>` in `ProphetsWay.BaseDataAccess`. This repository *demonstrates* them; a restatement here is a fourth copy that will drift |
| Benchmarks, performance work, or making `DataStore` production-grade | Nowhere. `.NoDB` is a teaching store; optimizing it adds code a reader must skip |
| Async, DI-container wiring, an API layer, a UI | An application solution — `ProphetsWay.BPA`. Adding a host here doubles the surface a reader must traverse before reaching the argument |
| Packaging metadata, a NuGet icon, a package description | Nowhere. Not published, by decision, permanently |

---

## Documentation Artifacts — What Is `n/a` Here

Recorded so it is not rediscovered and re-proposed each pass.

| Artifact | Status for this repository |
| --- | --- |
| `docs/repo-profile.md` | Present, generated by `Repo Analyst` |
| `docs/purpose-and-scope.md` | This file |
| `docs/feature-requests.md` | Created this session |
| `docs/nuget-extraction-proposal.md` | **`n/a` — not missing.** See below |
| `docs/architecture.md` | **`n/a`** — see below |
| per-project `docs/requirements.md` | **`n/a`** — see below |

**Owner decision, 2026-08-15: `docs/architecture.md` and per-project `docs/requirements.md` are deliberately
`n/a` for the utility and reference libraries.** Their architecture already lives in three places the
conventions name as authoritative — `AGENTS.md`, the README, and the XML `<remarks>` on the contracts — and a
fourth copy would drift away from all three. Those documents become relevant for multi-project *application*
solutions, where architecture is a design decision rather than a description of four files: `ProphetsWay.BPA`
certainly, `ProphetsWay.EFTools` possibly.

### Publication, and the NuGet extraction proposal

**`docs/nuget-extraction-proposal.md` is `n/a` for this repository, not missing, and should not be produced.**
This is a teaching artifact and will not be published. The empty packaging stubs are a recorded correct
decision, not drift.

For completeness, having looked for it: **nothing here warrants publication.** The only cluster that would
even be argued for is the test suite as a conformance kit — and that argument has already been made, in the
right place, against the right library: it is entry 1 in
[ProphetsWay.BaseDataAccess/docs/feature-requests.md](../../ProphetsWay.BaseDataAccess/docs/feature-requests.md),
deferred until EFTools and possibly BPA have taught us what "conforming" means against real storage. Extracting
it from *here* would be worse than deferring it, because these tests are written against
`IExampleDataAccess` — a specific seven-entity domain — not against `IBaseDataAccess`. A conformance kit has to
be parameterised over an implementer's own entities, which is a different artifact that happens to resemble
this one. Publishing this suite would also cost the repository the property that makes it valuable: the moment
the tests are a package, changing them stops being a documentation edit and starts being a release.

---

## Recommended Refinements

Every entry is cross-referenced to [docs/feature-requests.md](feature-requests.md), which carries the reasoning
and the status. Nothing here is applied by this document.

| # | Change | Rationale | Effort | Breaking? |
| --- | --- | --- | --- | --- |
| 1 | Advance the EFTools submodule pointer and update `ProphetsWay.Example.DataAccess.EF` onto the 3.x contracts — [FR 5](feature-requests.md#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) | The README's headline claim is false until this happens, and the headline claim is the whole product | High — another repository, seven DAOs, two new entities | No, to this repo |
| 2 | Demonstrate the accepting half of `Get<T>(null)` in `ConventionShowcase/` — [FR 1](feature-requests.md#1--demonstrate-the-accepting-half-of-gettnull) | Half of a documented behavioural split is currently taught as the whole of it | Low, if done in the showcase rather than the domain | No |
| 3 | Add the Visual Studio onboarding note to the README — [FR 7](feature-requests.md#7--a-visual-studio-onboarding-note-in-the-readme) | VS 2022/2026 cannot load the SDK-style `.sqlproj`; a newcomer hits this in the first two minutes of a repository designed to be opened | Trivial | No |
| 4 | ~~Genericize `ProphetsWay.Example.localhost.publish.xml`~~ — **Done in 3.1.0**, [FR 6](feature-requests.md#6--the-committed-developer-specific-publish-profile) | Removing or gitignoring the file was considered and **declined**: a teaching repository benefits from shipping a working publish profile. Resolved by genericization in place — line 7 changed `Data Source=Terebellum` → `Data Source=localhost`, a single token, nothing else altered. The file remains tracked, is not gitignored, and the `.sqlproj` `<None Include>` reference is unchanged | Trivial | No |
| 5 | ~~Point the "Not demonstrated" table in `AGENTS.md` at [docs/feature-requests.md](feature-requests.md) rather than restating the four gaps~~ — **Done in 3.1.0** | `AGENTS.md` is rewritten by `Repo Analyst` on every pass, so decisions recorded only there are not durable. Its "Coverage" section now links to entries 1–4 instead of restating them | Trivial | No |
| 6 | ~~Refresh the stale claims in [docs/repo-profile.md](repo-profile.md) at the next analyst pass~~ — **Done in 3.1.0** | The analyst pass that produced the current `repo-profile.md` corrected all of them; see below | Trivial | No |
| 7 | ~~Update `README.md` — test legs, TFMs, the `ProphetsWay.BaseDataAccess` version, and the EFTools claim~~ — **Done**, re-verified 2026-08-16 | All four statements now match the tree; the EFTools claim is stated with its pinned-submodule qualification rather than unqualified | Low | No |
| 8 | Add a `v3.1.0` entry to `CHANGELOG.md` | Its most recent heading is `v3.0.0`; the retarget is a shipped, consumer-visible change. **`Changelog Author`'s edit to make** | Low | No |
| 9 | ~~Retrait `UserDaoTests.ShouldGetCustomFunctionality` off `Contract`~~ — **Done 2026-08-16**, [FR 11](feature-requests.md#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays) | It demanded of every implementer a literal that `IUserDao`'s `<remarks>` explicitly declines to specify, inside the filter offered as the conformance gate. `Test Designer` replaced the class-level trait with six method-level ones and **split** the test — `ShouldCallCustomUserFunctionality` keeps the contractual half, `ShouldGetCustomFunctionality` is now `Characterization`. **Superseded 2026-08-16:** `ShouldCallCustomUserFunctionality` has since moved to `Characterization` as well — nothing in `IUserDao` promises a no-throw call — and the contractual half is now `ShouldNotAdoptTheInstanceHandedToCustomUserFunctionality` | Low | No — it removes an obligation |
| 10 | ~~Adopt the traceability rule for `Contract`-scoped assertions~~ — **Done 2026-08-16**, [FR 12](feature-requests.md#12--a-traceability-rule-for-contract-scoped-assertions) | An assertion wrongly in `Contract` is a demand made in the name of a specification that does not make it. The rule is now stated in `AGENTS.md` beside the `Scope` partition — the one place every agent loads on every request. It remains a **review convention with no enforcement mechanism**; FR 12 stays `Proposed` until it is enforceable, and only `Purpose Refiner` may change that status | Trivial | No |
| 11 | ~~Correct the `Scope` trait counts wherever they are quoted~~ — **Done 2026-08-16**, and **done again later the same day** | The counts were 138 / 4 / 20 of 162 at the first correction and are **139 / 5 / 20 of 164, 328 executions** now, after two tests were added and one was retraited. Corrected in [README.md](../README.md), `AGENTS.md`, [repo-profile.md](repo-profile.md) and this file in each pass. `CHANGELOG.md` line 80 carries the original figure and was **deliberately left alone** — it sits under the `v3.0.0` heading, where 138 / 2 / 20 was accurate; editing a shipped release's notes to match a later tree is falsifying history, not fixing a typo | Trivial | No |

**Explicitly not recommended:** enabling `<Nullable>enable</Nullable>`. The projects multi-target
`netstandard2.0`, which caps shared code at C# 7.3, so nullable reference types cannot work here regardless of
what a csproj claims — and warning noise in a repository whose product is clarity is a direct cost. The inert
`<NullableContextOptions>` property that prompted this question in earlier documents is already gone.

---

## Stale Claims — Corrected in 3.1.0

An earlier edition of this document listed six places where `AGENTS.md` or
[docs/repo-profile.md](repo-profile.md) contradicted the tree, and handed them to the next `Repo Analyst` pass.
**That pass ran, and all six are corrected.** They are kept here as the audit trail of what moved during 3.1.0,
not as outstanding work. Each was re-verified against the files on 2026-08-15.

| Source | Claim as it stood | Corrected to | Status |
| --- | --- | --- | --- |
| `AGENTS.md`, `repo-profile.md` | TFMs are `netstandard2.0;net48;net8.0;net9.0` for the DAL projects and `net48;net8.0;net9.0` for the tests | `netstandard2.0;net10.0` and `net48;net10.0`, with the library/test split documented as deliberate rather than as drift | **Corrected in 3.1.0** |
| `AGENTS.md`, `repo-profile.md` | `ProphetsWay.BaseDataAccess` reference is `3.0.0` | `3.1.0` in both | **Corrected in 3.1.0** |
| `repo-profile.md` | "160 tests across each of `net48`, `net8.0`, `net9.0` — 480 executed test cases" | "160 tests × 2 legs = **320 executions**, green" | **Corrected in 3.1.0** |
| `AGENTS.md` deviation 4; `repo-profile.md` §"Target Frameworks" and gap 4 | `<NullableContextOptions>enable</NullableContextOptions>` sits inertly in `ProphetsWay.Example.DataAccess.csproj` | Recorded as **closed** in both, with a do-not-reinstate note. The property is absent from every `.csproj`; the string survives only in the documents describing its removal | **Corrected in 3.1.0** |
| `AGENTS.md` §"3.0.0 Coverage" | The four "Not demonstrated" gaps are restated in `AGENTS.md` | The section now **links** to entries 1–4 in [docs/feature-requests.md](feature-requests.md), where the status and reasoning are durable | **Corrected in 3.1.0** |
| `repo-profile.md` §"Build & Test Verification" | Evidence is CI build `3.0.0.486` on PR #19 against the `3.0.0-feature-update` branch | Explicitly superseded; the current evidence is the local `Modernizer` run of 2026-08-15, and the absence of a 3.1.0 CI build is stated rather than papered over | **Corrected in 3.1.0** |

Two claims were checked and found **accurate**, recorded so they are not re-verified needlessly:
`TestDataAccessFactory.Create` is still the single construction site repository-wide, and the contracts project
still has exactly one `PackageReference` and no leakage of implementation types.

### Still Open — Owned By Other Agents

Neither of these is this document's to fix, and neither has been fixed yet.

| Source | What is still wrong | Owner |
| --- | --- | --- |
| ~~`README.md`~~ | **Fixed since this was written.** The README now states two test legs (`net48`, `net10.0`), `netstandard2.0;net10.0` for the DAL projects, a `ProphetsWay.BaseDataAccess` **3.1.0** reference, and the EFTools claim qualified as pending. Its trait counts and suite size are **139 / 5 / 20 of 164**, 328 executions, re-measured 2026-08-16. Re-verified against the file, not inherited | `README Author` |
| `CHANGELOG.md` | No `v3.1.0` entry — its most recent heading is `v3.0.0`. It must also **not** be back-edited for the trait counts: line 80 sits under `v3.0.0`, where 138 / 2 / 20 was accurate | `Changelog Author` |
