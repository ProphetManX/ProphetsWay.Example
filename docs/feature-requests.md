# Feature Requests & Deferred Decisions — ProphetsWay.Example

This is the record of things that were **considered and deliberately not built**, together with the reasoning
behind each decision. Nothing here is a limitation, an apology, or a TODO list. Each entry exists so a future
developer — or a future AI agent — can find the decision, judge whether the tradeoff that produced it still
holds, and reopen it as a real feature request when it does not.

**If you are about to propose one of these, read its entry first.** The entry tells you what was already
weighed, so your proposal can start from the open questions rather than from the beginning.

**Numbering is per-repository and starts at 1.** It does not continue, mirror, or correspond to the index in
[ProphetsWay.BaseDataAccess/docs/feature-requests.md](../../ProphetsWay.BaseDataAccess/docs/feature-requests.md),
which is a separate index that happens to run 1–9. That file's *format* is the convention followed here; its
*content* is unrelated. Where an entry below genuinely depends on one of its entries, it is cited by repository
and number.

The contracts themselves are **not** restated here. The binding rules live in the XML `<remarks>` on
[`IExampleDataAccess`](../ProphetsWay.Example.DataAccess/IExampleDataAccess.cs),
[`IDepartmentDao`](../ProphetsWay.Example.DataAccess/IDaos/IDepartmentDao.cs) and
[`ICompanyResourceDao`](../ProphetsWay.Example.DataAccess/IDaos/ICompanyResourceDao.cs), and the base-library
rules live in the `<remarks>` on `IBaseDataAccess` and `DataAccessConventionException` in
`ProphetsWay.BaseDataAccess`. Those are the source of truth. This file links to them and does not duplicate
them, because duplicated rules drift.

## Index

| # | Item | Status |
| --- | --- | --- |
| 1 | [Demonstrate the accepting half of `Get<T>(null)`](#1--demonstrate-the-accepting-half-of-gettnull) | **Proposed** — the highest-value gap |
| 2 | [A value-type (`struct`) entity](#2--a-value-type-struct-entity) | Deferred — cheap only if 1 lands first |
| 3 | [Bare `IBaseSoftEntity` — soft delete without an identifier](#3--bare-ibasesoftentity--soft-delete-without-an-identifier) | **Rejected** — not deferred |
| 4 | [Demonstrating that ambient `TransactionScope` is left untouched](#4--demonstrating-that-ambient-transactionscope-is-left-untouched) | **Rejected here** — belongs to a database-backed implementation |
| 5 | [Advance the EFTools submodule pointer onto the 3.x contracts](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) | **Proposed** — highest consequence in the repository |
| 6 | [The committed developer-specific publish profile](#6--the-committed-developer-specific-publish-profile) | **Done** — genericized in v3.1.0, file kept |
| 7 | [A Visual Studio onboarding note in the README](#7--a-visual-studio-onboarding-note-in-the-readme) | **Proposed** — trivial, high leverage |
| 8 | [Selecting the implementation from configuration instead of a code edit](#8--selecting-the-implementation-from-configuration-instead-of-a-code-edit) | **Rejected** — decided in code comments already |
| 9 | [Seed data for `Resources`, `Departments` and `CompanyResources`](#9--seed-data-for-resources-departments-and-companyresources) | **Deferred** — revisit with [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) |

Numbers are permanent. Entries are never renumbered and never removed —
[purpose-and-scope.md](purpose-and-scope.md) cites entries by number, and a rejected entry is decision history
rather than dead weight.

## The Bar Everything Here Is Judged Against

This repository's product is **clarity**, so the question is not *is this useful* but:

> **Does it earn its place in something meant to be read end to end?**

The budget being spent is the reader's working memory, not build time. A demonstration that adds a corner case
at the cost of the domain no longer fitting in a reader's head is a **net loss**, even when the corner case is
real and the code is correct. Seven entities is already near the ceiling.

Two consequences do most of the triage work below:

- **Prefer `ConventionShowcase/` over the domain.** That folder already carries entity and Data Access Layer
  types that exist only to be read and to fail in one specific way — `NoIdentifierEntity`,
  `GetOnlyIdentifierEntity`, `PrivateSetterIdentifierEntity`. A behaviour of `ProphetsWay.BaseDataAccess`
  demonstrated there costs a reader of the *domain* nothing and lands in the `Dispatcher` scope, where it
  correctly binds nobody's Data Access Layer. Adding an entity to `ProphetsWay.Example.DataAccess` costs
  contracts, a Data Access Object interface, a NoDB implementation, database schema, the Entity Framework
  mirror in another repository, and permanent reader attention.
- **Completeness of a 2×2 is not an argument.** "That corner is empty" is an observation. The corner earns a
  demonstration only if someone would plausibly build it.

The full statement of this bar, and the reasoning behind it, is in
[purpose-and-scope.md](purpose-and-scope.md#the-scope-bar-for-this-repository).

## Release Eligibility — v3.1.0

v3.1.0 is a **retarget plus documentation** release: `netstandard2.0;net10.0` for the two Data Access Layer
projects, `net48;net10.0` for the tests, `ProphetsWay.BaseDataAccess` moved to `3.1.0`, and the two documents
in this folder. No contract, entity, Data Access Object interface, or test changed in it.

| # | Status | Eligible for v3.1.0? | Why |
| --- | --- | --- | --- |
| 1 | Proposed | **No** | Adds tests. v3.1.0 changed no test by construction — the suite is byte-identical to 3.0.0's and its passing is the retarget's evidence |
| 2 | Deferred | **No** | Nothing to land, and it is gated on 1 |
| 3 | Rejected | n/a | Decided against |
| 4 | Rejected here | n/a | Decided against in this repository |
| 5 | Proposed | **No** | The work is in another repository and has its own version line |
| 6 | Done | **Yes — landed** | The hostname was genericized in place. No `.cs` file changed, so the retarget's evidence is intact |
| 7 | Proposed | Technically yes — **but no** | README-only and non-breaking, so a documentation release is the right home for it. Left out because the README is `README Author`'s file and no pass has run |
| 8 | Rejected | n/a | Decided against |
| 9 | Deferred | **No** | Nothing to land, and the deployed database is not exercised by any test in this repository |

**The honest answer is none, apart from entry 6** — which changed one attribute value in a publish profile and
no test at all. That is the correct outcome for a release whose evidence is "the same 160 tests still pass";
changing a test would have removed the evidence.

---

## 1 — Demonstrate the accepting half of `Get<T>(null)`

**Status:** **Proposed.** The most useful gap in the repository, and the judgment that it is the most useful
one is carried forward from `AGENTS.md` and [repo-profile.md](repo-profile.md) rather than re-derived.

### The gap

`IBaseDataAccess.Get<T>(object id)` has a **deliberate split**, documented in its `<remarks>` in
`ProphetsWay.BaseDataAccess`:

- where the entity's identifier property is a **non-nullable value type** — `int`, `long`, `Guid` — a `null`
  identifier throws `ArgumentException`, because there is nowhere to put it;
- where it is a **reference type** (`string`) or a **nullable value type** (`int?`), `null` is a legitimate
  value and is **accepted**.

Every entity in this repository keys on `int`, `long`, or `Guid`. Only the throwing half is demonstrated —
`DepartmentDataAccessTests.ShouldThrowWhenGenericGetIsGivenANullId` — and a reader who takes this repository
as the worked example of the paradigm can reasonably conclude that `Get<T>(null)` always throws. That is not a
missing nicety; **it is teaching half of a rule as though it were the whole rule**, in the one artifact whose
job is to teach it.

The companion assertion is already here and is worth reading first:
`ShouldThrowWhenGenericGetIsGivenAnIdThatIsNotAnInt` asserts `ArgumentException` **and specifically not**
`DataAccessConventionException`, pinning the caller-error / wiring-error distinction. Entry 1 is the same
distinction's third case.

### How it should be closed — the part worth arguing about

**Not by adding an eighth entity to the domain.** A `string`-keyed entity would touch
`ProphetsWay.Example.DataAccess`, a new `I*Dao`, `ProphetsWay.Example.DataAccess.NoDB`,
`ProphetsWay.Example.Database`, and the Entity Framework mirror in `ProphetsWay.EFTools` — and would then sit
permanently in the domain a reader has to hold, earning its keep on one assertion. That fails the bar in
[The Bar](#the-bar-everything-here-is-judged-against).

**Close it in `ConventionShowcase/` instead**, which is where it belongs on the merits rather than merely on
cost. `Get<T>(null)` is a behaviour of `ProphetsWay.BaseDataAccess`, not of `IExampleDataAccess`; the showcase
folder is already the home for exactly that class of demonstration, already carries local entity types built
to exhibit one identifier-shape each, and its tests already carry `[Trait("Scope", "Dispatcher")]` — which is
the correct scope, because no implementer's Data Access Layer is bound by it.

Concretely: two local entities alongside the existing three — one with a `string Id`, one with an `int? Id` —
and assertions that `Get<T>(null)` is accepted for both while the existing `Department` assertion continues to
show the throw. The `Dispatcher` count rises; `Contract` does not, which is the tell that nothing was added to
anyone's obligations.

### Open questions

1. Should `IdentifierShowcaseDal` grow the two entities, or should a sibling `NullIdentifierShowcaseDal` be
   added? `IdentifierShowcaseDal` is documented as "the mistakes an **entity** can make", and these two are
   *not* mistakes — they are the line falling elsewhere. `PrivateSetterIdentifierEntity` is already a
   not-a-mistake case in that file, so precedent exists either way.
2. Does the accepting half warrant a `Contract`-scoped test as well, on the grounds that any conforming Data
   Access Layer must accept it? **Probably not** — there is no reference-type-keyed entity on
   `IExampleDataAccess` to assert it against, and inventing one is the eighth entity again.

---

## 2 — A value-type (`struct`) entity

**Status:** Deferred. Real, sharp, and not worth domain space on its own — **but it becomes nearly free if
[entry 1](#1--demonstrate-the-accepting-half-of-gettnull) lands**, and should be reconsidered at that moment
rather than on a schedule.

`Get<T>` supports value-type entities, and such an entity **cannot express "not found" as `null`** — a struct
`T` has no null, so a miss comes back as a default-valued instance that looks like a real one. That is a
genuine footgun, arguably the sharpest in the base library, and every entity in this repository is a `class`,
so it is invisible here.

**Why deferred rather than proposed.** A struct entity is an unusual shape. Adding one to the taught domain
would put an entity in front of every reader that most of them will never write, and the lesson it carries —
"a struct cannot be null" — is one a C# developer already knows in the abstract. The cost is a permanent
increase in what the domain asks a reader to hold; the benefit is a warning most readers do not need.

**Why it is not rejected.** The consequence is silent. A miss that returns a plausible-looking zeroed entity
is the kind of defect that reaches production, and this repository is the only place in the workspace where a
reader would encounter it before writing their own Data Access Layer.

**Revisit trigger.** If entry 1 lands in `ConventionShowcase/`, the marginal cost of a third local type — a
`struct` entity with a `Dispatcher`-scoped test showing the not-found result is a default instance rather than
`null` — is a few dozen lines in a folder readers already approach expecting oddities. At that point the
calculus changes and this should be reopened. It should **not** be closed by adding a struct entity to
`ProphetsWay.Example.DataAccess`; that fails the bar for the same reason entry 1 does.

---

## 3 — Bare `IBaseSoftEntity` — soft delete without an identifier

**Status:** **Rejected.** Not deferred — this was decided against on the merits, and reopening it needs a real
implementation that wants the shape, not a fuller grid.

The observation is correct: `Department` implements `IBaseSoftIdEntity<int>` and `CompanyResource` implements
the bare `IBaseEntity`, so of the 2×2 of {soft, hard} × {keyed, keyless}, the soft-and-keyless corner is empty.

**The corner is empty because the shape is barely coherent.** Soft delete exists so a row can be marked
deleted, hidden from ordinary reads, and later *restored* — `IDepartmentDao` spends several of its 19 numbered
rules on exactly that lifecycle, including a custom `Restore`. Every one of those operations has to name the
row it acts on. An entity with no identifier cannot be named, so the only soft delete available to it is
"delete some row matching these values, and later restore some row matching these values" — which is not the
feature, it is a coincidence that resembles it. `ICompanyResourceDao` is deliberately built the other way: no
`IBaseDao<T>`, no `Get`, only `Insert`/`Delete`/`GetAll`, with `Delete` matching on the composite of its
values. That is what keyless deletion actually looks like, and it is already demonstrated.

Demonstrating the empty corner would therefore mean inventing a shape to fill a table, and inventing it in the
one repository that must not contain anything a reader has to be told to ignore. **Grid completeness is not an
argument** — see [The Bar](#the-bar-everything-here-is-judged-against).

**What would reopen this:** an actual Data Access Layer implementation — in `ProphetsWay.EFTools`,
`ProphetsWay.BPA`, or a consumer — that needs soft delete on a keyless entity and has a coherent story for
restore. If that exists, the shape is real and the example should teach it. Until then this is a corner of a
diagram, not a requirement.

---

## 4 — Demonstrating that ambient `TransactionScope` is left untouched

**Status:** **Rejected here.** Not rejected as a contract — it is a real, specified rule in `IBaseDataAccess`
— but rejected as something *this* repository can demonstrate meaningfully. The demonstration belongs to a
database-backed implementation.

### The rule

`IBaseDataAccess` specifies that `TransactionStart`, `TransactionCommit` and `TransactionRollBack` are scoped
to the Data Access Layer instance and **do not touch an ambient `System.Transactions.TransactionScope`.** A
caller who has opened an ambient scope keeps exactly the semantics they set up; the Data Access Layer neither
enlists in it nor suppresses it.

### Why this repository cannot demonstrate it

**The rule is about what the implementation does *not* do, and `.NoDB` cannot do it in the first place.** A
test here would open a `TransactionScope`, call some Data Access Layer members, and assert that
`Transaction.Current` is unchanged and that the scope's outcome is unaffected. Against a process-local
dictionary with a hand-rolled undo log, that assertion **cannot fail** — there is no connection, no resource
manager, and nothing capable of enlisting. It would be a test that passes by construction, and a green test
that could never have been red is worse than no test in a repository read as a specification: a reader would
take it as evidence the rule is enforced, when all it shows is that an in-memory store has no way to break it.

The absence of a `System.Transactions` reference is a symptom of that, not the cause. Adding one is easy —
`System.Transactions.Local` covers `netstandard2.0`, and the test project's `net48`/`net10.0` legs both have
it. Adding it would not make the test mean anything.

### Where it belongs instead

- **`ProphetsWay.EFTools`**, or any Data Access Layer over a real connection. There, enlistment is the default
  behaviour of the provider, so "the ambient scope is untouched" is a claim that can actually be violated —
  and therefore worth asserting.
- **The conformance kit**, if it is ever built: entry 1 in
  [ProphetsWay.BaseDataAccess/docs/feature-requests.md](../../ProphetsWay.BaseDataAccess/docs/feature-requests.md).
  This rule is a good example of why that kit is deferred until a second *real* implementation exists — it is
  a rule only real storage can be tested against.

### What should happen here instead

Nothing in code. If the omission is worth surfacing to a reader at all, it is one sentence in the README's
transaction section saying the ambient-scope rule is specified by `IBaseDataAccess` and is not exercised by an
in-memory store — pointing at this entry. That is a `README Author` decision, not a change to the suite.

---

## 5 — Advance the EFTools submodule pointer onto the 3.x contracts

**Status:** **Proposed.** The highest-consequence open item about this repository, and the only one that
affects whether its central claim is currently true.

`ProphetsWay.EFTools` consumes this repository as a **git submodule**, not a vendored copy — its
`.gitmodules` declares `path = ProphetsWay.Example`, `url = …/ProphetsWay.Example.git`, `branch = main`. The
two therefore cannot drift; the submodule is **pinned**, currently at the 3.0.0 branch point.

The consequence is a coordination requirement rather than a duplication problem. `ProphetsWay.Example.DataAccess.EF`
implements the *pre-3.0.0* `IExampleDataAccess`: no `Dispose`, no `Department`, no `CompanyResource`, no
snapshot rule, no ordering rule. So the README's headline sentence —

> "`ProphetsWay.EFTools` carries an Entity Framework implementation of the very same `IExampleDataAccess`
> contract, and the tests do not change to accommodate it."

— is true of the old pinned commit and **false of the current contracts**. The single sentence that makes this
repository worth reading is, right now, a statement about history.

**The work**, all of it in `ProphetsWay.EFTools`: advance the pointer, add the two new entities and their Data
Access Object interfaces to the Entity Framework implementation, implement `Dispose` and the three transaction
members against the real context, and satisfy the snapshot and ordering rules — the ordering rule in
particular requires an explicit `ORDER BY` on both `GetAll` and `GetPaged`, which is precisely the divergence
the rule was written to catch. Then run `dotnet test --filter "Scope=Contract"` there.

**Nothing in this repository changes.** It is recorded here because the claim that fails lives here, and
because anyone reading this repository's README needs to be able to find out that the claim is pending rather
than wrong.

**Related, discovered while verifying the submodule and belonging to that repository:** `.gitmodules` in
`ProphetsWay.EFTools` carries a stray second block, `[submodule "Submod"]`, with only `branch = main` and no
`path` or `url`. Git tolerates it today; it will confuse `git submodule` operations eventually.

---

## 6 — The committed developer-specific publish profile

**Status:** **Done** — resolved in **v3.1.0** by genericizing the profile rather than removing it, because a
teaching repository benefits from shipping a publish profile that works.

`ProphetsWay.Example.Database/ProphetsWay.Example.localhost.publish.xml` is committed and referenced as a
`<None>` item from the `.sqlproj`. It named an internal machine — `Data Source=Terebellum` — and uses
Integrated Security with `Persist Security Info=False`.

**This was not a secret leak.** No credential was present, and it should not be reported as one. It was two
smaller things: a hostname disclosure, and a per-developer file living in a shared repository where every
other file is there to be read and understood. A newcomer opening the database project found a publish target
pointing at a machine that does not exist for them.

Options weighed, in order of preference: delete it and gitignore the pattern; or genericize the connection so
it is a working starting point rather than someone else's configuration. The second is better for a teaching
repository — an onboarding developer *should* find a publish profile; it just should not be a stranger's.

### What was done

The owner initially decided to untrack the file, then read the argument above, agreed with it explicitly, and
reversed that decision. The change applied is a **single token** on line 7:
`Data Source=Terebellum` → `Data Source=localhost`. Integrated Security, `TargetDatabaseName`,
`DeployScriptFileName`, the `DatabaseInstance` SqlCmdVariable and every other setting are byte-identical.

The file remains **tracked**, is **not** gitignored, and the `<None Include>` reference in the `.sqlproj` is
unchanged. The database project was rebuilt afterward and produces its dacpac successfully. `Terebellum` no
longer appears anywhere in the repository's source or project files.

**The resolution is genericization, not removal** — a teaching repository benefits from shipping a working
publish profile. Anyone revisiting this entry should treat deletion as the option that was considered and
declined, not as unfinished work.

---

## 7 — A Visual Studio onboarding note in the README

**Status:** **Proposed.** Trivial effort, disproportionate leverage.

`ProphetsWay.Example.Database` was migrated to the SDK-style `Microsoft.Build.Sql/2.2.0` SDK. That migration
is correct per house convention and is what makes `dotnet build` work on the solution at all — but **Visual
Studio 2022/2026 cannot load an SDK-style `.sqlproj`.** A developer who opens `ProphetsWay.Example.sln` in
Visual Studio sees a failed project load in the first thirty seconds.

This is a Visual Studio limitation, not a defect, and the CHANGELOG documents it honestly. **The CHANGELOG is
the wrong place for it.** The audience for this fact is someone evaluating the paradigm who has just cloned
the repository, and they will read the README; they will read the CHANGELOG only after the thing has already
gone wrong. For a repository whose entire product is a smooth read, an unexplained project-load failure at
minute one is the most expensive thirty seconds in it.

One short note under a "Getting started" heading: the database project is SDK-style, Visual Studio cannot load
it, unload it and the three C# projects work normally, and VS Code / SSMS 22 / the .NET CLI are all fine.

`README.md` belongs to `README Author`; this entry records the decision that it should be added, not the
wording.

---

## 8 — Selecting the implementation from configuration instead of a code edit

**Status:** **Rejected.** This one was already decided, in the code, before this index existed — the entry
records the decision so it is not re-proposed as an improvement.

The obvious "upgrade" to `TestDataAccessFactory` is to read the implementation choice from an environment
variable or a `.runsettings` parameter, so one continuous integration run could cover both the in-memory and
the Entity Framework implementations without a code edit.

The `<remarks>` on [`TestDataAccessFactory`](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs) already
address it and decline it:

> Reading the choice from an environment variable or a `.runsettings` parameter would let one continuous
> integration run cover both implementations without a code edit at all, and that is what a real product
> should do. It is deliberately not done here: this repository is read before it is run, and one obvious line
> beats a lookup whose other half a reader has to go and find.

**That reasoning is accepted and is the correct application of the bar.** The demonstration is the thing being
sold, and the demonstration is *one visible line*. Replacing it with configuration trades the repository's
clearest thirty seconds for an operational convenience that this repository — which has exactly one in-repo
implementation — cannot even use.

**What would reopen this:** it becoming genuinely useful, which requires two implementations reachable from
one test run. That is not the current architecture and should not become it; see the "second in-repo
implementation" row in [purpose-and-scope.md](purpose-and-scope.md#out-of-scope-and-where-it-should-live-instead).
A real product built on this paradigm should absolutely do the configuration-driven thing, and the remarks
already say so.

---

## 9 — Seed data for `Resources`, `Departments` and `CompanyResources`

**Status:** Deferred. Real, cheap, and already half-planned in the source — but it buys nothing until a
database-backed implementation exists to consume it, which is [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts).

### The gap

[`PostDeploymentStart.sql`](../ProphetsWay.Example.Database/PostBuildScripts/PostDeploymentStart.sql) invokes
four seed scripts — `CreateCompanies`, `CreateJobs`, `CreateUsers`, `CreateTransactions`. `PostBuildScripts/`
contains those four plus `CreateDbUser.sql`, `PurgeSeedData.sql` and `PostDeploymentStart.sql` itself, and
**no seed script for `Resources`, `Departments` or `CompanyResources`.** Deploy the database and three of the
seven tables come up empty.

Two details sharpen it:

- [`PurgeSeedData.sql`](../ProphetsWay.Example.Database/PostBuildScripts/PurgeSeedData.sql) **already documents
  this gap and states where closing it would go** — "Departments and Resources are parents in that graph but
  have no seed script, so their rows are not ours to remove. If either is ever seeded, its purge belongs here —
  Departments after Users, Resources after CompanyResources." `CompanyResources` is already purged despite never
  being seeded, because leaving it populated blocks the `Companies` purge outright. The ordering work is done;
  only the `MERGE`s are missing.
- [`CreateUsers.sql`](../ProphetsWay.Example.Database/PostBuildScripts/CreateUsers.sql) does not carry
  `DepartmentId` in its `VALUES`, its `UPDATE SET`, or its `INSERT` column list at all. `Users.DepartmentId` is
  nullable, so the seed is valid — but it means even the rows that *do* exist leave `FK_Users_Departments`
  entirely unexercised. The relationship is present in schema and absent from data.

### Why this is not "the 3.0.0 entities were not finished"

**The asymmetry does not follow the 3.0.0 boundary, and diagnosing it that way would be wrong.** `Resources`
predates `Department` and `CompanyResource`; it has been unseeded for as long as it has existed. What 3.0.0 did
was *extend* a pre-existing gap from one table to three, not create one. Anyone who notices this later and
reaches for "the 3.0.0 additions were left incomplete" will produce a tidy explanation that happens to be false,
and will scope the fix to two tables when it is three.

### The schema itself is complete — and that narrows entry 5

Recorded explicitly because a reader of [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts)
needs it and entry 5 does not say it: **`dbo/Tables/` carries all seven tables and is level with the 3.0.0
contracts.** `Departments.sql` has `CreatedDate` / `UpdatedDate` / `DeletedDate`, which is the soft-delete shape
`IDepartmentDao` requires; `CompanyResources.sql` is a keyless-by-design composite-primary-key join table with
both foreign keys, which is the shape `ICompanyResourceDao` requires.

**Advancing the EFTools submodule pointer therefore carries no schema prerequisite.** The work in entry 5 is
the Entity Framework implementation and the contract rules, not the database project. Entry 5 is not edited to
say so — it is cross-referenced from here.

### Why deferred rather than proposed

Judged against [The Bar](#the-bar-everything-here-is-judged-against): closing this does **not** cost the reader
anything in the domain — no entity, no contract, no Data Access Object interface, no permanent addition to what
a reader has to hold. That is the unusual part of this entry, and it is why the answer is not *rejected*.

But the payoff today is close to zero:

- The suite runs against `ProphetsWay.Example.DataAccess.NoDB` and is green regardless. The database project is
  compile-validated in continuous integration and **never populated or queried by any test in this repository.**
  Seed data here is a convenience for manual exploration, not a fixture anything depends on.
- The consumer that would actually benefit is a SQL-backed implementation — `ProphetsWay.Example.DataAccess.EF`
  in `ProphetsWay.EFTools`, which is [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts).
  Written before that work starts, the seed rows are guesses at what it will want; written alongside it, they
  are shaped by an implementation that is reading them.

So this is deferred on **timing**, not on merit. It is the rare entry where the right answer is "yes, but not
yet" rather than "no."

### The strongest argument against deferring, recorded because it is a good one

**A teaching repository arguably should ship data for precisely the two entities that exist to demonstrate the
paradigm's edges.** `Department` and `CompanyResource` were added in 3.0.0 to mark the boundaries — soft delete
with a lifecycle, and a keyless join — and they are the two a curious reader is most likely to go looking at.
Someone who deploys the database expecting to explore the soft-delete showcase opens `Departments`, finds it
empty, and gets **no explanation at all** for why the one table built to show something interesting is the one
with nothing in it. Empty-with-no-note reads as unfinished, and in a repository whose product is clarity that
impression is itself a defect.

That argument is why the deferral has a trigger rather than an expiry date. It is not dismissed.

### Options weighed

| Option | Verdict |
| --- | --- |
| Three seed scripts plus the two purge lines `PurgeSeedData.sql` already specifies | The eventual answer. Do it with entry 5, not before |
| Seed `Departments` and `CompanyResources` only | **No.** It would bake in the wrong diagnosis — `Resources` is part of the same gap and `CompanyResources` cannot be seeded without it, since `FK_CompanyResources_Resources` has to point somewhere |
| Leave it, but note the emptiness where a reader meets it | The cheap mitigation if the deferral runs long. A comment in `PostDeploymentStart.sql` costs nothing and turns "unfinished" into "deliberate" |

### Revisit trigger

When [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) is picked up. At that point
there is an implementation that reads these tables, the seed rows can be shaped by what it needs, and the two
can be verified together. Reopen this then, and seed all three tables in one pass — the purge order is already
written down.

