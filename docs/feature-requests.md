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
| 5 | [Advance the EFTools submodule pointer onto the 3.x contracts](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) | **Proposed** — first step landed 2026-08-16; the claim it tracks is still pending |
| 6 | [The committed developer-specific publish profile](#6--the-committed-developer-specific-publish-profile) | **Done** — genericized in v3.1.0, file kept |
| 7 | [A Visual Studio onboarding note in the README](#7--a-visual-studio-onboarding-note-in-the-readme) | **Proposed** — trivial, high leverage |
| 8 | [Selecting the implementation from configuration instead of a code edit](#8--selecting-the-implementation-from-configuration-instead-of-a-code-edit) | **Rejected** — decided in code comments already |
| 9 | [Seed data for `Resources`, `Departments` and `CompanyResources`](#9--seed-data-for-resources-departments-and-companyresources) | **Deferred** — revisit with [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) |
| 10 | [A second Data Access Layer implementation in this repository — SQLite, MSSQL, or relocating the Entity Framework one](#10--a-second-data-access-layer-implementation-in-this-repository--sqlite-mssql-or-relocating-the-entity-framework-one) | **Rejected** — all four variants, including replacing `.NoDB` |
| 11 | [The two mis-scoped `Contract` assertions, and why `.NoDB` stays](#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays) | **Done** — both halves applied 2026-08-16; a third mis-scope closed with them |
| 12 | [A traceability rule for `Contract`-scoped assertions](#12--a-traceability-rule-for-contract-scoped-assertions) | **Proposed** — the cheap control that would have caught mis-scope 2 |
| 13 | [A seam letting another repository point this suite at its own implementation](#13--a-seam-letting-another-repository-point-this-suite-at-its-own-implementation) | **Scheduled** — direction approved 2026-08-16 (shape B); **the seam's design is deliberately deferred** until Lap 1 of the Entity Framework work shows what it must carry |
| 14 | [Restoring `DateTimeKind` on a `Department` reached as a navigation property](#14--restoring-datetimekind-on-a-department-reached-as-a-navigation-property) | **Proposed** — 2026-08-16; the gap left open by narrowing `IDepartmentDao` rule 18 to that Data Access Object's own reads |

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
| 10 | Rejected | n/a | Decided against, in all four variants |
| 11 | Done | **No — it is post-v3.1.0 work** | Both halves are applied to the working tree, which means **the tree no longer matches the suite v3.1.0 shipped**. It belongs to the next release, not retroactively to this one |
| 12 | Proposed | **No** | A review convention, not a change to any file that v3.1.0 touched |
| 13 | Proposed | **No** | Filed after v3.1.0 shipped, and it changes a test-project file |

**The honest answer is none, apart from entry 6** — which changed one attribute value in a publish profile and
no test at all. That is the correct outcome for a release whose evidence is "the same 160 tests still pass";
changing a test would have removed the evidence.

**Note for the next release, from [entry 11](#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays):**
the trait retrait applied this session means the phrase "no `.cs` file changed" is true of **v3.1.0 as shipped**
and is **no longer true of the working tree**. The next release's notes must say so; repeating v3.1.0's
byte-identical claim against the current tree would be false.

**Sharpened 2026-08-16.** Three `.cs` files have changed since v3.1.0 shipped — `SnapshotDeepCopyTests.cs`
and `UserDaoTests.cs`, retraited and then extended by a test each, and `IExampleDataAccess.cs`, which gained
the IDENTIFIER and ROW COUNT rules. The suite is **164 tests / 328 executions** against the 160 / 324 that
v3.1.0 shipped, and the version line is now **3.1.1**. The table above is a judgment about **v3.1.0** and is
left as it was; do not retrofit these figures into it.

**Two further changes postdate the table, and it is likewise not retrofitted — 2026-08-16.**
[Entry 13](#13--a-seam-letting-another-repository-point-this-suite-at-its-own-implementation) moved from
`Proposed` to **`Scheduled`** by owner decision, and
[entry 14](#14--restoring-datetimekind-on-a-department-reached-as-a-navigation-property) was filed as
`Proposed`. **Neither is eligible for v3.1.0** — 13 changes a test-project file and its design is deferred;
14 proposes nothing for this repository's suite at all. The table's row 13 records that entry's status *as
the table was written* and is left alone.

---

## 1 — Demonstrate the accepting half of `Get<T>(null)`

**Status:** **Proposed** — **re-verified 2026-08-16, unchanged.** The most useful gap in the repository, and
the judgment that it is the most useful one is carried forward from `AGENTS.md` and
[repo-profile.md](repo-profile.md) rather than re-derived.

**The gap is still open, checked rather than assumed.** A repository-wide search of
`ProphetsWay.Example.Tests/` for `Get<…>(null)`, `string Id` and `int? Id` returns exactly one hit —
`DepartmentDataAccessTests` line 164, `_da.Get<Department>(null)`, which is the **throwing** half.
`ConventionShowcase/` still carries three entity types (`NoIdentifierEntity`, `GetOnlyIdentifierEntity`,
`PrivateSetterIdentifierEntity`), none of them reference-keyed or nullable-keyed. The suite grew by four
tests since v3.1.0 and none of them touched this.

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

**Status:** Deferred — **re-verified 2026-08-16, unchanged.** Real, sharp, and not worth domain space on its
own — **but it becomes nearly free if [entry 1](#1--demonstrate-the-accepting-half-of-gettnull) lands**, and
should be reconsidered at that moment rather than on a schedule. Entry 1 has not landed, so the gate has not
opened; a search of the test project for `struct ` returns no entity declaration, and every entity in
`ProphetsWay.Example.DataAccess/Entities/` is still a `class`.

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

**Status:** **Rejected** — **re-verified 2026-08-16, unchanged.** Not deferred — this was decided against on
the merits, and reopening it needs a real implementation that wants the shape, not a fuller grid. The stated
reopening trigger is *an actual Data Access Layer that needs soft delete on a keyless entity*; the only
candidate named, `ProphetsWay.EFTools`, has not reached the 3.x contracts at all, so no such implementation
exists yet. The rejection stands on the same reasoning, not on inertia.

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

**Status:** **Rejected here** — **re-verified 2026-08-16, unchanged.** Not rejected as a contract — it is a
real, specified rule in `IBaseDataAccess` — but rejected as something *this* repository can demonstrate
meaningfully. The demonstration belongs to a database-backed implementation. A search of the test project
for `TransactionScope` still returns no match, and `.NoDB` is unchanged, so the reasoning below applies to
the current tree exactly as written.

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

**Status:** **Proposed** — re-triaged 2026-08-16, **deliberately unchanged**. The highest-consequence open
item about this repository, and the only one that affects whether its central claim is currently true.

**Why the status did not move, given that its first step has landed.** The pointer advance is one of six
steps in [ProphetsWay.EFTools FR 1](../../ProphetsWay.EFTools/docs/feature-requests.md), and the other five
have not happened:

- **Not `Done`.** The claim this entry exists to track — that the same tests pass against an Entity
  Framework implementation — is still false. Closing the entry would record the opposite.
- **Not `Deferred`.** Deferral implies parking something until a trigger. Nothing is parked; work is in
  flight in the other repository, and marking this `Deferred` would demote the item this file calls its
  highest-consequence one.
- **Not `Scheduled`.** There is nothing in *this* repository to schedule. The entry's own closing paragraph
  already says so.

**And the advance has, for now, made things worse rather than better** — `ProphetsWay.EFTools` **does not
compile** as of this date, because its test project, its adapters and its Entity Framework Data Access Layer
were all left behind by the pointer. That is a known waypoint recorded in that repository's FR 1 and its
`AGENTS.md`, not a regression, but anyone reading this entry expecting the claim to be closer to true should
know the intermediate state is a red build.

`ProphetsWay.EFTools` consumes this repository as a **git submodule**, not a vendored copy — its
`.gitmodules` declares `path = ProphetsWay.Example`, `url = …/ProphetsWay.Example.git`, `branch = main`. The
two therefore cannot drift; the submodule is **pinned**.

**Factual correction, 2026-08-16 — the first step of this entry has landed.** The pointer is no longer at the
3.0.0 branch point: it was advanced to **`d845863`**, the tip of this repository's `main` and therefore the
**3.1.0** tree. Verified by reading `ProphetsWay.EFTools/.git/modules/ProphetsWay.Example/HEAD` against
`.git/refs/heads/main` here — the two are the same commit. The rest of the entry is unaffected and the status
line above is deliberately untouched; only `Purpose Refiner` may change it.

The consequence is a coordination requirement rather than a duplication problem. `ProphetsWay.Example.DataAccess.EF`
still implements the *pre-3.0.0* `IExampleDataAccess` — no `Dispose`, no `Department`, no `CompanyResource`, no
snapshot rule, no ordering rule — and both it and `ProphetsWay.EFTools` itself still reference
`ProphetsWay.BaseDataAccess` **2.5.0**, verified in their two `.csproj` files. So the README's headline
sentence —

> "`ProphetsWay.EFTools` carries an Entity Framework implementation of the very same `IExampleDataAccess`
> contract, and the tests do not change to accommodate it."

— is true of the old pinned commit and **false of the current contracts**. The single sentence that makes this
repository worth reading is, right now, a statement about history.

**The work**, the remainder of it in `ProphetsWay.EFTools`: ~~advance the pointer~~ (done), add the two new
entities and their Data
Access Object interfaces to the Entity Framework implementation, implement `Dispose` and the three transaction
members against the real context, and satisfy the snapshot and ordering rules — the ordering rule in
particular requires an explicit `ORDER BY` on both `GetAll` and `GetPaged`, which is precisely the divergence
the rule was written to catch. Then run `dotnet test --filter "Scope=Contract"` there.

~~**Nothing in this repository changes.**~~ It is recorded here because the claim that fails lives here, and
because anyone reading this repository's README needs to be able to find out that the claim is pending rather
than wrong.

**Corrected 2026-08-16 — "nothing in this repository changes" is no longer true.** That sentence was written
when `BaseUnitTests<T>` still exposed `protected abstract T GetIExampleDataAccess { get; }`, which is how
`ProphetsWay.EFTools` supplied its own implementation. **The single-construction-site refactor removed that
hook**, and `TestDataAccessFactory.Create()` is a `static` method taking no argument and naming `.NoDB`
directly \u2014 so the Entity Framework repository, which may not edit files under its pinned submodule, now has
no way to run this suite against its implementation. Closing this entry therefore requires a change **here**
as well as there. The reasoning is not restated: it is
[entry 13](#13--a-seam-letting-another-repository-point-this-suite-at-its-own-implementation).

**Update 2026-08-16 — the fork that blocked entry 13 is resolved in direction, and this entry's status is
still deliberately `Proposed`.** The owner chose the **upstream seam** over a duplicate suite in
`ProphetsWay.EFTools`, so the route by which this entry can eventually be closed is now settled; entry 13
moved to `Scheduled` on the strength of it. **This entry did not move**, for the reason its own paragraphs
above give: the claim it tracks is still false, and a decision about *how* the claim will be demonstrated is
not the claim becoming true. The seam's **design** is separately deferred until Lap 1 of the Entity Framework
work — so the sequence closing this entry is Lap 1, then the seam, then a green
`dotnet test --filter "Scope=Contract"` against the Entity Framework implementation.

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

---

## 10 — A second Data Access Layer implementation in this repository — SQLite, MSSQL, or relocating the Entity Framework one

**Status:** **Rejected**, in all four variants that were put. Not deferred, and not "not yet" — the reasoning
below is structural rather than a matter of timing, so a future proposal has to defeat the argument rather than
wait for the budget.

This entry answers a question the owner asked directly, together with three variants of it that the question
turned out to contain. It exists so that the answer is not re-derived, and so that the *reason* — which is the
durable part — survives the question.

### How it arose

During a `ProphetsWay.EFTools` 3.x design pass, two assertions in this suite were found to be marked
`Scope=Contract` while actually encoding choices of the in-memory store. They are recorded separately as
[entry 11](#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays). The immediate reaction to finding
them was to wonder whether `ProphetsWay.Example.DataAccess.NoDB` should be swapped for a SQLite-backed store,
"since we've had a few quirks shake out because of the fake db instance all being in memory" — and from there,
whether SQLite should be a second implementation and SQL Server a third.

### First, a distinction that has to be resolved before the question can be answered

**"SQLite second, MSSQL third" conflates two implementations with one implementation on two providers.**

`ProphetsWay.EFTools`'s approved 3.x plan — owner decision **D4**, recorded in
[that repository's purpose-and-scope.md](../../ProphetsWay.EFTools/docs/purpose-and-scope.md#owner-decisions--2026-08-15)
— certifies its **single** EF Core implementation on **two provider legs**: SQLite in-memory as the fast
continuous-integration gate, and a SQL Server container for provider fidelity. **D2** and **D8** in the same
table make the same split explicit at the level of public wording: relational providers generally are in scope,
SQLite and SQL Server are *certified*.

So SQLite and SQL Server are already accounted for, and they are accounted for as **configuration of one Data
Access Layer**, in a different repository, on that repository's version line. Nothing in this repository has to
be built to obtain them, and building something here would not add a third implementation — it would add a
second *copy* of a story already told.

Once that is separated out, the real options are the four below.

### The options, and the verdict on each

| Option | Verdict |
| --- | --- |
| **A. Status quo** — `.NoDB` here, `.EF` in `ProphetsWay.EFTools`, EF certified on SQLite and SQL Server | **Accepted.** Zero new work, and it is already the strongest available form of the argument |
| **B. Replace `.NoDB` with a SQLite-backed implementation** | **Rejected.** It would destroy the argument, not improve it — see below |
| **C. Move or mirror `ProphetsWay.Example.DataAccess.EF` into this repository** | **Rejected.** The two implementations being in *different repositories* is the property, not an accident of layout |
| **D. A hand-written ADO.NET `ProphetsWay.Example.DataAccess.MSSQL`, bypassing Entity Framework** | **Rejected.** The strongest version of the argument on paper, and the one whose cost is most disproportionate to what it adds |

### B — why replacing `.NoDB` is the worst of the four

**`.NoDB` is not a fake database. It is the argument.**

The claim this repository makes is that the same suite passes against *radically* different storage. A
`Dictionary<int, T>` with a hand-rolled undo log versus a relational engine is radical. Two relational engines
is a configuration change. Swapping `.NoDB` for SQLite would leave the repository demonstrating that a
relational store behaves like a relational store — which nobody doubted, and which is not what
`ProphetsWay.BaseDataAccess` claims.

**And the premise of the swap is backwards.** The two mis-scopes were not `.NoDB` failing. They were `.NoDB`
*working*: it satisfied assertions that a normalized store physically cannot, and by satisfying them it exposed
that the assertions were specifying the wrong thing. That defect is only visible from a store whose physical
constraints differ from the specification's assumptions. **A second relational store would never have surfaced
it** — it would have agreed with the first one and the mis-scope would have survived undetected, wearing a
green tick. `.NoDB` earned its place by being the thing that disagreed.

The owner reached this conclusion independently and it is accepted here without qualification. It is recorded
in [entry 11](#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays) as the durable part.

### C — why consolidating the Entity Framework implementation here is rejected

This is the most tempting of the four, because the objection to it sounds like mere convention: the swap
demonstration is currently split across two repositories, and a reader has to clone a second one to see both
halves. Consolidating looks like it buys visibility for a fraction of option D's cost.

**It is rejected because the split is load-bearing.** [purpose-and-scope.md](purpose-and-scope.md#in-scope)
already states it under **In Scope**: the argument requires two implementations in *different repositories*
with *different storage*, not two in this one. Two implementations sitting side by side in one solution
demonstrate that a well-factored solution can have two Data Access Layers — a much weaker and much less
surprising claim. What makes the paradigm's claim land is that a **separately versioned, separately released,
independently authored** implementation in another repository satisfies a suite it never saw, without the suite
changing. Move it here and the demonstration becomes a local arrangement.

There is also a concrete mechanical objection. `ProphetsWay.EFTools` consumes this repository as a **git
submodule**. Mirroring `.EF` into this repository would create a cycle: this repository would contain a copy of
a project that lives in a repository that contains a copy of this repository. That is the duplication problem
[entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) explicitly records as *not* being
the current situation, and it would be a genuine regression to introduce it.

**The real complaint underneath option C is valid, and it has a cheaper answer.** If the objection is "a reader
cannot see both halves," the fix is a README paragraph naming the second implementation, where it lives, and
what it demonstrates — not relocating a project. That belongs to `README Author` and is already adjacent to
[entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts), whose resolution is the precondition
for stating the claim unqualified at all.

### D — why a hand-written ADO.NET implementation is rejected despite being the best argument

Judged purely as an argument, **D is the strongest of the four.** Dictionary, then an object-relational mapper,
then raw SQL is a genuinely three-point line, and the third point is the one that most convincingly rules out
"it only works because Entity Framework is doing something clever." That is a real gain and it should not be
pretended otherwise.

It is rejected on **cost measured in the currency this repository actually spends**, which
[The Bar](#the-bar-everything-here-is-judged-against) names as the reader's working memory, not build time:

- **A permanent third copy of seven Data Access Objects**, plus an aggregate Data Access Layer, plus
  connection, command, mapping and transaction plumbing that exists in no other implementation here. Every
  future contract change — every new numbered rule on `IDepartmentDao`, every entity — is then a three-place
  edit rather than a one-place edit, forever.
- **Continuous integration needs a real SQL Server.** `ProphetsWay.EFTools` is taking on exactly that cost
  under [its FR 11](../../ProphetsWay.EFTools/docs/feature-requests.md), with a container leg and the retirement
  of blanket `LocalTestsOnly`. Taking it on here too doubles that work and puts it in the one repository whose
  virtue is that `git clone` followed by `dotnet test` is the entire onboarding.
- **It makes the repository bigger to read at exactly the point it is already near its ceiling.** Seven entities
  is stated as near the limit; a third full implementation is a larger addition than any entity ever proposed
  and rejected here.
- **Hand-written ADO.NET is the least representative thing a reader will go on to write.** The audience is
  someone deciding whether to adopt the paradigm; almost all of them will implement it over an object-relational
  mapper. The third point on the line is the one fewest readers need.

**What would reopen it:** somebody actually building a non-Entity-Framework Data Access Layer over these
contracts — in `ProphetsWay.BPA` or a consumer — for a reason of their own. At that point the implementation
exists because it is wanted, and the question becomes whether to *point at* it, which is cheap. Building one
speculatively to complete a three-point line is
[grid completeness](#the-bar-everything-here-is-judged-against) with a larger price tag.

### The cheaper alternative that was raised, and an honest verdict on it

It was put that the two mis-scopes were found by **reading the contract**, not by running a second store — so a
conformance checklist, or a deliberately hostile minimal Data Access Layer implementing the contracts as
adversarially as possible, might catch the same class of defect for a fraction of the cost.

**As a substitute for a second implementation: no. It is a consolation prize, and it fails on the specific
defect that prompted the question.** An adversarial in-memory Data Access Layer can satisfy mis-scope 1
trivially, by denormalizing exactly as `.NoDB` does. The assertion is only *impossible* for a store with
normalization constraints, so no amount of hostility from another in-memory store surfaces it. What surfaced it
was a second implementation with **different physical constraints** — which is `ProphetsWay.EFTools`, which
already exists and is already the mechanism. The lesson is that the existing mechanism worked, not that a new
one is needed.

**As a cheap control against the *other* mis-scope: yes, and it is worth doing.** Mis-scope 2 — a
`Contract`-scoped assertion pinning a string literal that no contract states — is catchable by a review rule
costing nothing at all. That is split out as [entry 12](#12--a-traceability-rule-for-contract-scoped-assertions)
rather than buried here, because it stands on its own and should not inherit this entry's rejection.

### What this entry does not decide

It does not touch `ProphetsWay.EFTools`'s own plan. SQLite and SQL Server as **provider legs of the Entity
Framework implementation** are approved there under D2, D4 and D8 and are unaffected by anything above.
Rejecting a second implementation *here* is not a comment on certifying one implementation on two providers
*there*.

---

## 11 — The two mis-scoped `Contract` assertions, and why `.NoDB` stays

**Status:** **Done — 2026-08-16.** Both halves are applied to the working tree. Previously `Scheduled`
with one half outstanding. This is a decided matter, not an open bug — it is recorded so that a future
reader finding either assertion does not diagnose it fresh, and does not re-propose replacing `.NoDB` on
the strength of it.

**What closed it, verified rather than inherited.** [UserDaoTests.cs](../ProphetsWay.Example.Tests/UserDaoTests.cs)
was opened on this date. The class-level `[Trait("Scope", "Contract")]` is **gone**, replaced by
**seven method-level traits — five `Contract` and two `Characterization`** — each carrying `<remarks>` that
name the sentence in `IUserDao` making it characterization, including an explicit instruction not to
promote it back.

**Three things landed rather than the one this entry authorized, and the difference is worth naming:**

1. `ShouldGetCustomFunctionality` was retraited to `Characterization` — **this is mis-scope 2, as
   authorized.**
2. `ShouldCallCustomUserFunctionality` was **also** retraited to `Characterization`, and its `<remarks>`
   say so in as many words: *"It was `Contract` until this pass."* **This is a third mis-scope, which this
   entry did not name.** It fails the same test as mis-scope 2 and for the same reason — it asserted a
   no-throw promise that `IUserDao` explicitly declines to make. Recorded here rather than as a new entry
   because it is the same defect in the same method group, found in the same pass; it is evidence for
   [entry 12](#12--a-traceability-rule-for-contract-scoped-assertions), not a separate finding.
3. `ShouldNotAdoptTheInstanceHandedToCustomUserFunctionality` was **added** as a new `Contract` test. This
   entry anticipated it as an option — *"keep a `Contract` test asserting only 'something was written'"* —
   and that is the shape `Test Designer` chose.

**Why `Done` and not left `Scheduled`:** every change this entry authorized is in the tree, and what
remains of it — the `.NoDB` decision below — is recorded reasoning rather than pending work. **Flagged for
owner confirmation** only because item 2 above went beyond the authorization; if the owner would rather the
third mis-scope carry its own number, say so and it will be filed as a new entry rather than folded here.

### What was wrong

The `Scope` trait partition is load-bearing: `Contract` is the subset any conforming Data Access Layer must
pass, and `dotnet test --filter "Scope=Contract"` is only a usable gate if every test in it is genuinely
binding. Two assertions were in `Contract` and should not have been.

**1 — `SnapshotDeepCopyTests.ShouldReadANavigationPropertyEditBackInsideTheTransactionThatSubmittedIt`.** It
required `Update(user)` to make `user.Company.Name` read back with the edit **while the `Companies` row keeps
the old name**. `.NoDB` satisfies this because `UserDao.Update` writes a deep copy of the user into the Users
table, so a user's view of a company and the Companies table are physically separate data and may legitimately
disagree. **A normalized relational store cannot do this** — one row, one name, read back through a join — and
an implementation that cascaded the write through the navigation property would be rewriting `Company`, `Job`
and `Department` rows the caller never named. It is a property of the in-memory store's row shape, not
something the SNAPSHOT RULE on `IExampleDataAccess` asks of anybody.

**Applied.** The test now carries `[Trait("Scope", "Characterization")]` and `<remarks>` stating why, including
an explicit instruction that it must not be promoted back. Note that the fix required moving `Contract` from
the class to each method, because **xUnit accumulates traits rather than letting a method override a class** —
a class-level `Contract` would have left the test selected by the filter no matter what the method declared.
The class documents this.

**2 — `UserDaoTests.ShouldGetCustomFunctionality`.** It asserts
`co2.Whatever.ShouldBe("custom functionality triggered")`. That literal is a `private const
CustomFunctionalityStamp` in `ProphetsWay.Example.DataAccess.NoDB.Daos.UserDao`, and the `<remarks>` on
[`IUserDao`](../ProphetsWay.Example.DataAccess/IDaos/IUserDao.cs) **explicitly decline to specify** what
`CustomUserFunctionality` does: *"states no behavior of its own, and none is implied here — what it does, and
what if anything it writes back onto the caller's instance, is the implementation's to define."* A
`Contract`-scoped test therefore demands of every implementer a value the contract deliberately refuses to name.

~~**Outstanding.** `UserDaoTests` carries a single class-level `[Trait("Scope", "Contract")]` covering five
`[Fact]`s, so the same restructure is needed: replace it with five method-level traits, four `Contract` and one
`Characterization`, and add `<remarks>` pointing at the sentence in `IUserDao` that makes it characterization.~~
**Applied — 2026-08-16.** The description above is retained struck through because it is what was
authorized, and what landed is close but not identical: the class-level trait is gone and the methods carry
their own, but there are **seven** of them rather than five — five `Contract`, two `Characterization` —
because a third mis-scope was found and a new `Contract` test was added in the same pass. See the status
block at the top of this entry.

The other two assertions in the method — that `Id` round-trips and that `Whatever` *changed* — are genuinely
contractual, so an alternative shape is to keep a `Contract` test asserting only "something was written" and
move only the literal into a `Characterization` sibling. That choice belongs to `Test Designer`; **this entry
records that the retrait is authorized, not the wording of it.** **`Test Designer` took that alternative** —
`ShouldNotAdoptTheInstanceHandedToCustomUserFunctionality` is the resulting `Contract` test.

### The counts moved — and the predictions below were all wrong

The partition was `Contract` 138 / `Characterization` 2 / `Dispatcher` 20, total 160. This entry then
predicted **137 / 3 / 20** with mis-scope 1 applied and **136 / 4 / 20** with mis-scope 2 applied, on a total
that "stays 160."

**All three of those figures are superseded. The tree as of 2026-08-16 is `Contract` 139 /
`Characterization` 5 / `Dispatcher` 20, total 164 — 328 executions over the two legs.** Verified by a static
count of every `[Trait("Scope", …)]` in `ProphetsWay.Example.Tests/`: five method-level `Characterization`
traits (`CompanyDaoTests`, `DataAccessTransactionTests`, `SnapshotDeepCopyTests`, and **two** in
`UserDaoTests`), and two class-level `Dispatcher` traits covering the 11 and 9 facts in
`ConventionShowcaseTests` and `ExceptionPassthroughShowcaseTests`.

**The prediction failed because the total was assumed fixed.** Retraiting moves a test between buckets and
cannot change the sum, so "the total stays 160" was sound arithmetic about retraits and wrong about the
world — four tests were **added** in the same period, two of them closing a gate hole where a cascading
`Update` had been passing all 138 `Contract` tests. A count in this file is a fact about a tree, and it goes
stale the moment anyone writes a test. **Do not quote 137 / 3 / 20, 136 / 4 / 20, 160, or 324 executions
from anywhere.**

[`README.md`](../README.md), `AGENTS.md` and [repo-profile.md](repo-profile.md) each quote a triple; each is
its own owner's to correct, none should be corrected twice, and the figure they should all reach is
**164 / 139 / 5 / 20**.

**`CHANGELOG.md` line 80 quotes it too and must be left alone.** It sits under the `v3.0.0` heading, where 138
was the true count. Correcting a shipped release's notes to match a later tree is not a fix.

### The decision that matters: `.NoDB` stays

The finding prompted the question of whether the in-memory store should be replaced by SQLite. **It should
not**, and the reasoning is the durable part of this entry:

> `.NoDB` is not a fake database — it is the argument. The paradigm's claim is that the same tests pass against
> *radically* different storage; a dictionary versus a relational engine is radical, two relational engines is a
> configuration change. And the two mis-scopes were not `.NoDB` failing. They were `.NoDB` **working** —
> satisfying assertions a normalized store physically cannot, and thereby exposing defects in the specification
> that a second relational store would never have surfaced.

The full option analysis, including moving the Entity Framework implementation here and adding a hand-written
ADO.NET one, is [entry 10](#10--a-second-data-access-layer-implementation-in-this-repository--sqlite-mssql-or-relocating-the-entity-framework-one).
All variants are **Rejected**.

### The generalisable lesson, which is why this is written down at all

**A `Contract` assertion that only one implementation's physical data layout can satisfy is a specification
defect, and it is invisible from inside that implementation.** Both mis-scopes were found the same way — by
someone attempting a second implementation with different constraints and asking "can I satisfy this?" That is
the detector, and it is the reason
[entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) is worth more than its face value:
advancing the `ProphetsWay.EFTools` submodule pointer is not only a correctness fix for that repository, it is
**this repository's audit of its own `Contract` scope.** Expect it to find more.

---

## 12 — A traceability rule for `Contract`-scoped assertions

**Status:** **Proposed.** Trivial, and the only control that would have caught
[mis-scope 2](#11--the-two-mis-scoped-contract-assertions-and-why-nodb-stays) before an implementer hit it.

### The rule

> A `Scope=Contract` assertion must trace to a stated rule — a numbered rule in a Data Access Object interface's
> `<remarks>`, one of the Data-Access-Layer-wide rules on `IExampleDataAccess`, or a documented behaviour of
> `IBaseDataAccess`. If nothing states it, the assertion is `Characterization`.

Mis-scope 2 fails this immediately and visibly: the test asserts a literal, and the interface's `<remarks>` says
in as many words that the behaviour is unspecified. No second store, no new implementation, and no execution is
needed to see it — only the discipline of asking the question.

Mis-scope 1 **would not** have been caught by it, and that limit should be stated plainly rather than glossed:
that assertion *did* trace to the SNAPSHOT RULE. It traced to a defensible over-reading of a real rule, which is
a subtler failure and needs a second implementation to expose. This rule is a cheap filter, not a proof.

### Why it is Proposed rather than Scheduled

It is a review convention, and this repository has no mechanism that enforces conventions — no `.editorconfig`,
no analyzer, no continuous-integration check on test metadata. The honest options are a paragraph in the
`Scope` trait documentation where the partition is already explained, and an item in whatever review checklist
a `Test Designer` pass follows. Both are cheap; neither is enforcement. It should not be marked `Done` on the
strength of writing a sentence.

**The cost of getting this wrong is asymmetric**, which is why it is worth a rule at all. An assertion wrongly
in `Characterization` is a missed obligation that the next implementer simply does not have to meet. An
assertion wrongly in `Contract` is a demand this repository makes of every implementer in the name of a
specification that does not make it — and since `--filter "Scope=Contract"` is offered as *the* conformance
gate, that is the repository failing at the one job it claims.

### Deliberately not proposed alongside it

A deliberately hostile minimal Data Access Layer, as a way of shaking out over-specified assertions
mechanically. It was weighed under
[entry 10](#10--a-second-data-access-layer-implementation-in-this-repository--sqlite-mssql-or-relocating-the-entity-framework-one)
and rejected: an adversarial in-memory store can satisfy mis-scope 1 by denormalizing exactly as `.NoDB` does,
so it does not catch the class of defect that prompted the question. `ConventionShowcase/` already hosts
deliberately mis-wired Data Access Layers, and their subject is the reflection convention in
`ProphetsWay.BaseDataAccess` — not the domain contracts. Extending them to police contract scope would give
that folder a second, unrelated job.

---

## 13 — A seam letting another repository point this suite at its own implementation

**Status:** **Scheduled** — moved from `Proposed` on **2026-08-16** by owner decision. Filed 2026-08-16 while
re-triaging [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts) after the
`ProphetsWay.EFTools` submodule pointer advanced. **This is the entry the pointer advance created**, and
nothing in this index covered it.

**Read the next section before reading anything else here.** The status moved because the *direction* is
now committed; **the seam's design is not**, and that is deliberate rather than unfinished.

### Owner decision — 2026-08-16: the direction is approved, the design is deferred

The owner resolved the A-or-B fork posed by
[ProphetsWay.EFTools FR 6](../../ProphetsWay.EFTools/docs/feature-requests.md#6--rebuild-prophetswayeftoolstests-on-the-3x-factory-and-scope-traits)
in favour of **shape B — the upstream seam, which is this entry.**

- **Shape A is declined.** A duplicate local suite inside `ProphetsWay.EFTools` **ends the demonstration this
  repository exists to provide**: two copies of the assertions that must be kept in step, and the moment they
  diverge *"the tests do not change to accommodate it"* stops being checkable. That option is closed, not
  parked. It is the fourth row of [Shapes worth weighing](#shapes-worth-weighing--the-decision-is-the-direction-not-the-mechanism)
  below, and it is now struck.
- **What is committed is the direction and nothing more.** Nobody has yet attempted to satisfy the 3.1.0
  contracts in Entity Framework, so **the seam's requirements are unknown** — what it must carry, whether the
  Entity Framework suite needs per-class construction, a shared fixture, provider selection, or none of those.
  The seam is to be designed **once Lap 1 of the Entity Framework work has shown what it must carry.**
- **Therefore: the absence of a seam design in this repository is not outstanding work.** A later reader must
  not mistake a committed *direction* for an approved *design*, and must not open a defect because no seam
  exists yet. The trigger for designing it is Lap 1, not this decision.

**This does not reopen [entry 8](#8--selecting-the-implementation-from-configuration-instead-of-a-code-edit),
and nothing here should be read as softening its rejection.** Entry 8 declined *configuration-driven*
selection — an environment variable or a `.runsettings` parameter. Shape B asks only that the construction
line be **reachable from a repository that cannot edit it**. The obvious, unconditional default line stays
exactly where it is. That distinction is the whole of why this entry exists separately from entry 8, and it
must survive any future summary of either.

### The problem

`ProphetsWay.EFTools` consumes this repository as a **pinned git submodule** and is under a standing
instruction never to edit files under `ProphetsWay.Example/` from that side — edits happen here and the
pointer moves. That arrangement is correct and is what
[entry 10](#10--a-second-data-access-layer-implementation-in-this-repository--sqlite-mssql-or-relocating-the-entity-framework-one)
protects.

It also means the Entity Framework repository **cannot run this suite against its own implementation.**
Verified by opening [`TestDataAccessFactory.cs`](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs):

```csharp
public static IExampleDataAccess Create()
{
    //>>> The one line to change to point this suite at another implementation. <<<
    return new ExampleDataAccess();
}
```

`CreateAs<T>()` calls `Create()` and casts. The method is `static`, takes no argument, consults nothing, and
names the in-memory implementation directly. `BaseUnitTests<T>` sources every subject from it.

**Before 3.0.0 there was a seam and it has been removed.** `BaseUnitTests<T>` used to declare
`protected abstract T GetIExampleDataAccess { get; }`, and `ProphetsWay.EFTools.Tests` supplied the Entity
Framework Data Access Layer by inheriting the test classes and overriding it — six adapter files doing
nothing else. **That is precisely how the swap demonstration was made real from the other side, and the
single-construction-site refactor removed it** without anything replacing it.

So the instruction in those `<remarks>` — *"to run this suite against a different implementation … change
the single `return` … and nothing else"* — is true for a reader of this repository and **unreachable for the
one repository that actually wants to do it.**

### Why this matters more than it looks

This repository's product is a single claim: *the same tests pass against radically different storage.* The
tests being **unmodified** is the whole argument. Right now that argument is demonstrable only by a reader
editing one line locally and running it — a thought experiment with a compile step, not a second
implementation actually passing.

**Nothing is currently broken by this** — the Entity Framework side is mid-flight and non-compiling for
three other reasons. But it means [entry 5](#5--advance-the-eftools-submodule-pointer-onto-the-3x-contracts)
cannot be closed by work in that repository alone, which is not what entry 5 says. Entry 5's *"nothing in
this repository changes"* was written when the inheritance hook existed. **It is now false**, and this entry
is the correction rather than a rewrite of entry 5's reasoning.

### This is not [entry 8](#8--selecting-the-implementation-from-configuration-instead-of-a-code-edit)

**Read that entry before reacting to this one, and do not treat its rejection as covering this.** Entry 8
declines reading the choice from an environment variable or `.runsettings`, because one obvious line beats a
lookup whose other half a reader must go and find. **That reasoning is accepted here without qualification
and this entry does not attack it.**

The difference is *who* is asking:

| | Entry 8 | This entry |
|---|---|---|
| Asks for | The choice to be made **without a code edit** | The choice to be **reachable from a repository that cannot make the edit** |
| Motivated by | Continuous-integration convenience | The central claim being demonstrable at all |
| Costs the reader | The obvious line stops being obvious | Nothing, if the obvious line remains the default |

A seam that leaves `Create()` returning `new ExampleDataAccess()` as its visible, unconditional default
satisfies entry 8's objection completely. That is the bar any proposal here must clear.

### Shapes worth weighing — the decision is the direction, not the mechanism

Still deliberately not narrowed. The owner's 2026-08-16 decision chose **that** a seam exists, not **which**
one; the choice interacts with
[ProphetsWay.EFTools FR 6](../../ProphetsWay.EFTools/docs/feature-requests.md#6--rebuild-prophetswayeftoolstests-on-the-3x-factory-and-scope-traits)
and belongs to whoever picks that up **after Lap 1**.

| Shape | Note |
|---|---|
| **Restore an overridable hook on `BaseUnitTests<T>`**, defaulting to `TestDataAccessFactory.CreateAs<T>()` | Closest to what existed, and the adapters return. The reason it was removed — six files of pure ceremony — was a real cost, and it comes back. **See the sketch below**, which is the only shape anyone has written down concretely — and it is a sketch, not a design decision |
| **A settable `Func<IExampleDataAccess>` on `TestDataAccessFactory`**, defaulted to the current line | Smallest change; the default stays visible and obvious. Introduces mutable static state into a suite that currently has none, which is a genuine objection |
| **Ship the tests as a package or shared-source item** a second repository consumes and parameterizes | The most correct and the most expensive. It is the conformance kit in [ProphetsWay.BaseDataAccess FR 1](../../ProphetsWay.BaseDataAccess/docs/feature-requests.md) in all but name, and it should not be built twice |
| ~~**Do nothing; the Entity Framework repository writes its own suite**~~ | **Struck 2026-08-16 — this is shape A and the owner declined it.** Kept rather than deleted because the reason is the point: two copies of the assertions that must be kept in step, and the moment they diverge *"the tests do not change to accommodate it"* stops being checkable |

#### A mechanism that was proposed and cannot work — recorded so it is not re-proposed

**The suggestion:** change `TestDataAccessFactory.Create()` from `public static` to `protected`, and have a
class inside `ProphetsWay.EFTools` override it.

**It does not compile, for two independent reasons.** Verified 2026-08-16 by opening
[`TestDataAccessFactory.cs`](../ProphetsWay.Example.Tests/TestDataAccessFactory.cs), whose declaration is
`public static class TestDataAccessFactory` and whose member is `public static IExampleDataAccess Create()`:

1. **A `static` class cannot declare a `protected` member.** Protected access exists for derived types, and a
   static class can have none.
2. **A `static` method is never virtual.** There is nothing to override even if the accessibility were legal.

This is not a near-miss to be repaired with a keyword. Anything built on "override the factory method" is
answering the problem with a mechanism C# does not have.

#### A viable shape that was sketched — explicitly **not** adopted

Recorded at the same fidelity as the failed one, so the next reader starts from something real:

```csharp
// SKETCH ONLY — not a design decision, not approved, not scheduled.
protected virtual IExampleDataAccess CreateDataAccess() => TestDataAccessFactory.Create();
```

A **virtual hook with a default** on `BaseUnitTests<T>` rather than the abstract property that used to be
there. The single obvious line in `Create()` stays intact and stays the default that every test in this
repository takes, which is what satisfies [entry 8](#8--selecting-the-implementation-from-configuration-instead-of-a-code-edit)'s
objection; a derived suite in another repository gets something that genuinely exists to override.

**It is a candidate, not the design.** It was written down during the decision that chose shape B and carries
none of that decision's authority. Its known cost is the one the first row of the table above names — the
adapter classes come back — and whether that cost is worth paying is exactly what Lap 1 is expected to inform.

### The strongest argument against doing anything

**The single construction site is one of this repository's best thirty seconds**, and every shape above
makes it slightly less true that *one line* is the whole story. A reader who opens
`TestDataAccessFactory.cs` and finds a `Func` field, or a virtual hook, has been handed a mechanism where
they were previously handed a fact. That is a real loss in a repository whose product is clarity, and it is
paid by every reader to benefit exactly one consumer.

The counter is that the one consumer is the *point* — the Entity Framework implementation is the second half
of the argument, not an incidental user — and a demonstration nobody can actually run is not clearer than a
mechanism that works. But the owner should weigh it rather than be told it is settled.

### Explicitly out of scope for this entry

Anything that makes this repository contain or reference a second implementation. That is
[entry 10](#10--a-second-data-access-layer-implementation-in-this-repository--sqlite-mssql-or-relocating-the-entity-framework-one),
**Rejected in all four variants**, and nothing here reopens it. A seam is not an implementation.

---

## 14 — Restoring `DateTimeKind` on a `Department` reached as a navigation property

**Status:** **Proposed** — 2026-08-16. Filed as the record of an owner decision that **narrowed** a contract
rule, and of the gap that narrowing deliberately leaves open. Nothing in this index covered the ground; it is
a new entry rather than an extension of one.

**The rule text is not restated here.** The binding wording is the `<remarks>` on
[`IDepartmentDao`](../ProphetsWay.Example.DataAccess/IDaos/IDepartmentDao.cs), which an `Interface Architect`
is applying the narrowing to. That file is the source of truth; this entry is the reasoning behind it.

### The decision — 2026-08-16

`IDepartmentDao` rule 18's **retrieval** clause binds **`IDepartmentDao`'s own reads only.**

A `Department` reached as a **navigation property** through another Data Access Object's include carries the
`DateTimeKind` the provider supplied — in practice `Unspecified`. That is **stated behaviour, not a defect**.

### Why the rule had to be narrowed rather than enforced

- **Relational providers do not persist `DateTimeKind`.** A value written as `Utc` comes back `Unspecified`
  on every provider in play. Honouring the retrieval clause therefore requires the Data Access Object to
  **re-stamp `Kind` after materialization** — there is no cheaper mechanism, because the information is gone
  by the time the row arrives.
- **Timestamp normalization is a per-Data-Access-Object mechanism.** The `ProphetsWay.EFTools` 3.x design
  does the re-stamping through a `NormalizeRetrievedTimestamp` hook declared **only on the soft-delete Data
  Access Object bases** — the ones that own the three timestamps in the first place.
- **A hard Data Access Object has no such hook.** When `UserDao` materializes `User.Department`, it is
  `UserDao`'s query doing the work, and `DepartmentDao`'s hook is not on it and cannot be. Enforcing the
  broad reading would mean every hard Data Access Object knowing how to normalize every soft entity reachable
  from its own — which is the opposite of the per-Data-Access-Object arrangement everything else uses.

### Precedent — this is the same shape as `ApplyReadFilter`, already accepted

An included `Department` **already bypasses `DepartmentDao`'s soft-delete read filter** and arrives populated
even when soft-deleted, for exactly the same structural reason: the including Data Access Object owns the
query. That behaviour is documented and accepted.

**So the timestamp bypass is a second consequence of a boundary that was already drawn, not a new
concession.** Anyone meeting it and reaching for "this is an inconsistency" should read it as the pair it
belongs to.

### The rejected alternative — recorded so it is not re-proposed

**A global `DateTimeKind`-restoring value converter on the context.** Declined on two grounds:

1. **It is indiscriminate.** It applies to *every* `DateTime` column in the model, not the three this contract
   governs. A contract about `CreatedDate`/`UpdatedDate`/`DeletedDate` would be enforced by a mechanism that
   also silently relabels every unrelated date a consumer maps.
2. **Its reach is accidental.** It would take effect only for consumers who happen to derive from an optional
   base type, so the rule would hold or not hold depending on a choice unrelated to the rule.

### The cost this imposes on a consumer — stated plainly rather than buried

An `Unspecified` `DateTime` passed to `.ToLocalTime()` is **treated as local and shifted by the machine's
offset.** The result is a **silently wrong value, not an exception** — no throw, no warning, and a difference
that is invisible on a machine running in UTC and wrong everywhere else.

That is the whole reason this is filed as a request rather than closed as a note. The behaviour is defensible;
its failure mode is quiet, which is the kind that reaches production.

### What this repository looks like today — verified, not assumed

Verified 2026-08-16 by searching `ProphetsWay.Example.Tests/` for `DateTimeKind`, `ShouldBeUtcStamp` and
`ShouldContainStamp`. Every timestamp-`Kind` assertion in the suite is in
[DepartmentDaoTests.cs](../ProphetsWay.Example.Tests/DepartmentDaoTests.cs) or
[DepartmentDataAccessTests.cs](../ProphetsWay.Example.Tests/DepartmentDataAccessTests.cs), and every one of
them runs against `IDepartmentDao`'s own reads — directly, or through the dispatcher's `Get<Department>`,
which lands on the same Data Access Object. **No test asserts `Kind` on a `Department` reached through a
navigation property.**

**The narrowing therefore costs this suite no assertion and breaks no test.** It is a narrowing of what was
*claimed*, not of what was *checked* — which is precisely why it needs a durable record: nothing would have
failed to tell anyone.

### What the request actually is

Not "undo the narrowing." The open question is whether anything should be **built** for a consumer who needs
`Kind` to survive an include — a normalization pass the including Data Access Object can opt into, an
entity-level convention, or explicit guidance and nothing else.

Judged against [The Bar](#the-bar-everything-here-is-judged-against), a demonstration here would cost domain
space to teach a mechanism that does not exist yet, so **nothing is proposed for this repository's suite at
this time.** What is proposed is that the question stay open and attached to this reasoning.

### What would close it

- A consumer meeting the silent `.ToLocalTime()` shift in practice — which turns this from a stated boundary
  into a reported defect and changes the calculus.
- Or the Entity Framework implementation reaching a point where a normalization pass on the including side is
  cheap, at which point the narrowing can be revisited on evidence rather than on design.

Until then the answer is: **the boundary is where the mechanism is**, and a reader who finds an `Unspecified`
timestamp on an included `Department` has found this entry rather than a bug.


