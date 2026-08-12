# ProphetsWay.Example


Build Status:  
[![Build Status](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_apis/build/status/ProphetManX.ProphetsWay.Example?repoName=ProphetManX%2FProphetsWay.Example&branchName=main)](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_build/latest?definitionId=24&repoName=ProphetManX%2FProphetsWay.Example&branchName=main)

---

## Why this repository exists

You have been told to swap SQL Server for PostgreSQL, or to put Entity Framework behind you, or simply to
test your service layer without a database. You open the code and find `DbContext` in your business logic,
`SqlConnection` in your controllers, and a test suite that only passes when a server is listening on 1433.
That is not a data access change. That is a rewrite of everything that touches data access.

[ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess) is a paradigm for
avoiding it: entity markers, capability-composed DAO interfaces, one aggregate interface your business logic
injects, and an optional reflection dispatcher. **This repository is the worked example that proves the
paradigm holds** — a real domain, a real implementation, and a test suite written against the interfaces
rather than against the implementation.

The same test suite passes against a completely different data access layer.
[ProphetsWay.EFTools](https://github.com/ProphetManX/ProphetsWay.EFTools) carries an Entity Framework
implementation of the very same `IExampleDataAccess` contract, and the tests do not change to accommodate
it. That is the entire argument, and it is why this repository is worth twenty minutes of your time.

**Highlights**

- **A DAL you can replace.** Business logic depends on `IExampleDataAccess` and on entities. Nothing else.
- **Tests that outlive the implementation.** One factory method names the implementation for the whole
  suite. Change its single `return`, run `dotnet test --filter "Scope=Contract"`, and the assertions come
  along for free.
- **Contracts specified, not implied.** `IDepartmentDao` carries 19 numbered rules; `ICompanyResourceDao`
  carries 10. Two implementations that both pass cannot differ in a way that matters.
- **The awkward parts are demonstrated, not hidden.** Soft delete, a keyless entity, transactions, disposal,
  and a folder of deliberately mis-wired DALs showing exactly how the dispatcher fails.

---

## Read this first: the demonstration

One method in the test project names an implementation. `Create` on
[TestDataAccessFactory](ProphetsWay.Example.Tests/TestDataAccessFactory.cs) is the only place in the entire
suite where `new ExampleDataAccess()` appears:

```csharp
public static IExampleDataAccess Create()
{
	//>>> The one line to change to point this suite at another implementation. <<<
	return new ExampleDataAccess();
}
```

Every test class inherits `BaseUnitTests<T>`, which asks the factory for the data access layer and disposes
it after each test:

```csharp
public abstract class BaseUnitTests<T> : IDisposable
{
	protected T _da;

	public BaseUnitTests()
	{
		_da = TestDataAccessFactory.CreateAs<T>();
	}

	public void Dispose()
	{
		(_da as IDisposable)?.Dispose();
	}
}
```

`T` is usually one of the DAO interfaces `IExampleDataAccess` aggregates rather than the aggregate itself,
and no generic constraint can express "an interface `IExampleDataAccess` happens to inherit" — so
`CreateAs<T>` is a checked cast that names the interface an implementation stopped implementing, rather than
throwing a bare `InvalidCastException` out of a constructor.

A concrete test class therefore names the interface it exercises, and nothing else:

```csharp
[Collection(TestCollections.SharedStore)]
[Trait("Scope", "Contract")]
public class DepartmentDaoTests : BaseUnitTests<IDepartmentDao>
{
	// ...every test in the class is written against IDepartmentDao
}
```

### Running this suite against your own DAL

Change one `return`:

> **Illustrative** — not currently present in the repo.

```csharp
public static IExampleDataAccess Create()
{
	return new MyEntityFrameworkDataAccess();
}
```

Then run the tests any conforming implementation has to pass:

```
dotnet test --filter "Scope=Contract"
```

Nothing else in the suite changes. That is what "swappable" means when it is real rather than aspirational.

### Why the filter, and what it leaves out

Every test carries a `Scope` trait saying who it binds.

| `Scope` | Tests | Who has to pass it |
|---|---|---|
| `Contract` | 138 | Every implementation of `IExampleDataAccess`. These are the rules the interfaces state. |
| `Characterization` | 2 | This implementation only. Another DAL may legitimately fail them. |
| `Dispatcher` | 20 | Nobody's DAL. They pin the reflection convention in `ProphetsWay.BaseDataAccess` itself. |

**The honest answer to "will all 160 of these pass against my implementation?" is: all but two, and here is
exactly which two.**

- `CompanyDaoTests.ShouldGetCustomCompanyFunction` — `ICompanyDao.GetCustomCompanyFunction(int)` stands in
  for whatever query your domain adds beyond the surface it inherits, and the interface deliberately says
  nothing about what its argument means. This implementation reads it as a position in the set and wraps
  round the end, so asking for 100 against three stored companies returns one of them. An implementation
  that read it as an identifier would return `null` and be exactly as conforming.
- `DataAccessTransactionTests.ShouldExposeUncommittedWritesToAnotherInstance` — pins `READ UNCOMMITTED`, the
  isolation this in-memory store does not provide. `IBaseDataAccess` specifies no isolation level, so a DAL
  that gets isolation from a real database **fails this test correctly**.

The `Dispatcher` tests live in `ConventionShowcase/` and construct their own deliberately mis-wired DALs.
They never touch the factory, so swapping the suite onto another implementation must leave them exactly as
they are — they are tests to read, not a target to hit.

That split is the point. A suite claiming total portability would be hiding the two places a different
implementation is allowed to differ, and hiding them is how a paradigm gets found out.

---

## What is in the repository

Four projects. The first one is the contract; everything else either implements it or tests it.

#### ProphetsWay.Example.DataAccess

The Data Access Layer contract, as specified by
[ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess). It defines the
entities the example needs — `Company`, `Job`, `User`, `Transaction`, `Resource`, `Department` and
`CompanyResource` — the `Roles` enum one of them uses, an `I*Dao` interface per entity, and the aggregate
`IExampleDataAccess` that anyone writing a DAL implementation fills out.

Entities are tagged with the entity contract that matches their key: `IBaseIdEntity<int>` for `Company`,
`Job`, `User` and `Department`, `IBaseIdEntity<long>` for `Transaction`, `IBaseIdEntity<Guid>` for
`Resource`, and the bare `IBaseEntity` for `CompanyResource`, which has no identifier at all. `Department`
adds `IBaseSoftIdEntity<int>` for soft delete.

DAO interfaces are composed from the capability interfaces — `IBaseDao<T>` for Get/Insert/Update/Delete,
`IBaseGetAllDao<T>` for `GetAll`, `IBasePagedDao<T>` for `GetPaged` and `GetCount` together — and add
whatever custom members the domain needs. `IExampleDataAccess` aggregates all seven DAO interfaces
alongside `IBaseDataAccess`, and that single interface is what business logic injects.


#### ProphetsWay.Example.DataAccess.NoDB

An in-memory implementation, written so that this DAL — and any implementation of it — can be unit tested
without a database, locally and in a build pipeline alike.
[ProphetsWay.EFTools](https://github.com/ProphetManX/ProphetsWay.EFTools) implements the same contract on
Entity Framework and reuses these tests.

`DataStore` is a static class holding one `StoreTable` per entity, standing in for the database itself
rather than for a connection to it. Every write passes through it and hands an undo entry to a
`TransactionLog` on the way, which is how a rollback knows what there is to reverse.

#### ProphetsWay.Example.Database

A SQL Server database project on the `Microsoft.Build.Sql` SDK, so it builds with `dotnet build` on any
platform. Deploy it if you want to tinker against a real server. Post-deployment scripts seed the tables so
the database is not empty; the data is deliberate nonsense.

The SDK-style format comes with a tooling constraint: **Visual Studio 2022 and Visual Studio 2026 cannot
open an SDK-style `.sqlproj`.** Both are limited to the legacy SSDT project format, which targets
.NET Framework 4.x — a limitation of Visual Studio's SQL project system, not of this project. SSMS 22,
VS Code and the .NET CLI all handle it. See
[Building & Testing Locally](#building--testing-locally) for what to do if Visual Studio is your editor.

#### ProphetsWay.Example.Tests

The most useful part of the repository. xUnit and Shouldly, 160 tests, run across `net48`, `net8.0` and
`net9.0`. By default they run against the in-memory implementation, but every test class takes its DAL from
`TestDataAccessFactory.Create` — point that one method at any class implementing `IExampleDataAccess`,
backed by anything you like, and the suite tests your implementation instead. Every test carries a `Scope`
trait, so you can run only the ones your implementation is bound by.

---

## Install

There is no NuGet package for this repository — it is a reference implementation, not a library. Clone it
and read it, or copy the shape into your own solution.

```
git clone https://github.com/ProphetManX/ProphetsWay.Example.git
```

The contracts it implements do ship as a package:

```
dotnet add package ProphetsWay.BaseDataAccess
```
```
Install-Package ProphetsWay.BaseDataAccess
```

Targets: the two data access layer projects build for `netstandard2.0`, `net48`, `net8.0` and `net9.0`; the
test project runs on `net48`, `net8.0` and `net9.0`. The solution references the released
`ProphetsWay.BaseDataAccess` 3.0.0.

---

## Quick Start

> **Illustrative** — not currently present in the repo.

```csharp
using ProphetsWay.Example.DataAccess;
using ProphetsWay.Example.DataAccess.Entities;
using ProphetsWay.Example.DataAccess.NoDB;

// The one and only line that names an implementation.
using (IExampleDataAccess da = new ExampleDataAccess())
{
	var engineering = new Department { Name = "Engineering", Description = "Builds the thing." };

	da.Insert(engineering);   // Id and CreatedDate are generated and written back onto engineering
	da.Delete(engineering);   // a soft delete: DeletedDate is stamped, the row stays

	var stillThere = da.Get(new Department { Id = engineering.Id });
	var live = da.GetAll(new Department());

	// stillThere is not null and carries a DeletedDate.
	// live contains no department with that Id.
}
```

What you just saw: an identifier and a timestamp generated by the DAL, a delete that does not delete, and a
retrieval that still resolves the row so a foreign key pointing at it does not dangle. `User.Department`
exists precisely so that last point is demonstrated rather than claimed.

---

## Core Concepts

**Contracts live in one project; implementations live in others.** `ProphetsWay.Example.DataAccess` holds
the entities, the `I*Dao` interfaces and the aggregate `IExampleDataAccess`. It references no storage
technology and never will. The moment one appears in a public signature there, the DAL stops being
swappable and the paradigm has failed.

**The DAO interfaces are a menu, not a mandate.** Compose the capabilities your entity needs, add your own
members alongside them, or — as `ICompanyResourceDao` does — inherit none of them and declare exactly what
you support.

**The snapshot rule is binding on every DAO.** Anything returned by `Get`, `GetAll` or `GetPaged` is a deep
snapshot, and anything handed to `Insert`, `Update` or `Delete` is *read*, not adopted. Mutating what came
back does not change stored data; mutating an argument after the call returns does not reach the store.
This is not fussiness — an in-memory store that hands back the object it is holding gives a caller a way to
change stored data that no database can reproduce, and the claim that the same tests pass against either
DAL would be quietly false. It is also what lets a rollback reverse an `Update`. The rule is stated in full
in the `<remarks>` on [IExampleDataAccess](ProphetsWay.Example.DataAccess/IExampleDataAccess.cs); read it
there, not here.

**Two ways to call the same DAL.** Every member is reachable as an ordinary interface call
(`da.Insert(dept)`), and the inherited CRUD members are additionally reachable through the reflection
dispatcher on `BaseDataAccess` (`da.Insert<Department>(dept)`). Custom members such as `Restore` and
`GetCustomCompanyFunction` are interface calls only — which is the point they make. The tests replay the
same setup helpers down both paths to prove the two agree.

---

## Domain model

| Entity | Identifier | Contract | What it demonstrates |
|---|---|---|---|
| `Company` | `int` | `IBaseIdEntity<int>` | Paging, plus a custom retrieval on the DAO |
| `Job` | `int` | `IBaseIdEntity<int>` | The plain `GetAll` shape |
| `User` | `int` | `IBaseIdEntity<int>` | Navigation properties and a custom DAO member |
| `Transaction` | `long` | `IBaseIdEntity<long>` | A non-`int` key and a two-level entity graph |
| `Resource` | `Guid` | `IBaseIdEntity<Guid>` | A `Guid` key assigned by the entity's constructor |
| `Department` | `int` | `IBaseSoftIdEntity<int>` | Soft delete, plus a custom `Restore` |
| `CompanyResource` | none | `IBaseEntity` | A keyless join, and the edge of what the dispatcher can do |

| DAO interface | Inherits | Adds |
|---|---|---|
| `ICompanyDao` | `IBasePagedDao<Company>` | `GetCustomCompanyFunction(int id)` |
| `IJobDao` | `IBaseGetAllDao<Job>` | — |
| `IUserDao` | `IBaseDao<User>` | `CustomUserFunctionality(User user)` |
| `ITransactionDao` | `IBasePagedDao<Transaction>` | — |
| `IResourceDao` | `IBaseGetAllDao<Resource>` | — |
| `IDepartmentDao` | `IBaseGetAllDao<Department>`, `IBasePagedDao<Department>` | `Restore(Department item)` |
| `ICompanyResourceDao` | **nothing** | `Insert`, `Delete`, `GetAll` |

`Roles` is an enum, carried on `User` twice — as `RoleStr` and `RoleInt`.

---

## Three things that will bite you

These came out of building this release. They are the traps a prospective adopter hits first, and none is
obvious from the API surface.

### 1. `Delete(company)` and `Delete<Company>(company)` are different code paths

Omit the type argument and C# binds straight to your own `Delete(Company)` at compile time. No reflection
runs and no convention is checked. Supply it and the call goes through the dispatcher on `BaseDataAccess`,
which resolves your method by name and signature at run time.

> **Illustrative** — not currently present in the repo.

```csharp
IExampleDataAccess da = new ExampleDataAccess();

da.Delete(company);            // compile-time binding to your own Delete(Company). No dispatcher involved.
da.Delete<Company>(company);   // the dispatcher: resolved by reflection, convention enforced at run time.
```

**A data access layer can be thoroughly tested through its concrete methods and still be completely
mis-wired for every generic caller.** The library cannot warn you — the concrete call compiles and works.
If your consumers use the generic form, test the generic form. That is why `DepartmentDataAccessTests` and
`CompanyResourceDataAccessTests` exist alongside the DAO test classes: they replay the same setup helpers
through the dispatcher.

### 2. The visibility rules for methods and for identifier setters are opposites

A convention **method** that is not a public instance method is invisible to the dispatcher and fails
exactly as though you had never written it. `StaticMethodDal` in the showcase does nothing wrong except
being `static`.

An identifier **property setter** that is `private`, `protected`, `internal` or `init` works fine. The
convention requires the identifier to be *assignable*, not *publicly* assignable, and reflection can assign
a non-public setter.

Both rules are defensible on their own. Together they look arbitrary until you see them side by side, which
is why `IdentifierShowcaseDal` puts `PrivateSetterIdentifierEntity` in the same file as the two entities
that fail.

### 3. `Update` returns `1` on a soft-deleted row while `Delete` returns `0` on one

Not a contradiction. `1` means *a row matched and was written*. `0` means *nothing changed*.

Updating a soft-deleted department really does write to it — the department stays deleted, but its data
changed, so the count is `1`. Deleting an already-deleted department changes nothing: the existing
`DeletedDate` is deliberately not refreshed, so the timestamp keeps reporting when the department was
actually deleted, and the count is `0`. `Delete` is idempotent, and so is `Restore`.

> **Illustrative** — not currently present in the repo.

```csharp
var dept = new Department { Name = "Finance" };
da.Insert(dept);

da.Delete(dept).ShouldBe(1);    // stamps DeletedDate
da.Delete(dept).ShouldBe(0);    // already deleted, nothing changed, stamp untouched

dept.Description = "Renamed after the fact.";
da.Update(dept).ShouldBe(1);    // a row matched and was written. It stays deleted.

da.Restore(dept).ShouldBe(1);   // back in GetAll, GetPaged and GetCount
da.Restore(dept).ShouldBe(0);   // already live
```

The binding statement of all of this is the numbered CONTRACT on
[IDepartmentDao](ProphetsWay.Example.DataAccess/IDaos/IDepartmentDao.cs). Read it there — a copy in a
README drifts.

---

## Common Scenarios

### Soft delete

`Department` is the showcase. `Delete` stamps `DeletedDate` rather than removing the row; `Get` still
returns the department so a `User.Department` reference resolves; `GetAll`, `GetPaged` and `GetCount`
exclude it and must agree with one another. `Restore` clears the stamp.

`Restore` is a custom method, included to show that you can add whatever members your domain needs and
reach them through the aggregate interface like any other call. **It is not a pattern the paradigm asks you
to follow** — a domain that genuinely needs user-facing reversible removal usually wants an explicit
archive flag modeled as part of the domain.

Full contract: [IDepartmentDao](ProphetsWay.Example.DataAccess/IDaos/IDepartmentDao.cs).

### An entity with no identifier

`CompanyResource` is a pure many-to-many join carrying nothing but two foreign keys. It implements
`IBaseEntity` and nothing else, and its DAO deliberately inherits `IBaseDao<T>` not at all — no `Update`
(there is no non-key field to change) and no `Get` (the natural key is a pair, which `IBaseDao<T>` cannot
express).

Through the dispatcher, `Insert<CompanyResource>`, `Delete<CompanyResource>` and `GetAll<CompanyResource>`
all work, because none of them resolves an identifier. `Get<CompanyResource>(id)` throws
`DataAccessConventionException` and can never be made to work.

**A join must name rows that exist.** Rule 10 binds the caller: the company and the resource have to be
there. An implementation over a store that enforces referential integrity rejects a join naming a row that
is not, throwing an exception of the storage layer's own; one whose store cannot enforce it is not obliged
to check, so a call that succeeds there is no evidence the rows exist. The tests insert a real `Company` and
a real `Resource` before every join for exactly that reason — an arrangement built on synthetic identifiers
is a suite that only ever passes against a lenient store.

**Treat this as a novelty, not the norm.** Give an entity an identifier by default: it costs one column, it
makes the entity addressable by the dispatcher, it lets a single row be updated in place, and it stops
being optional the moment the join grows a field of its own.

Full contract: [ICompanyResourceDao](ProphetsWay.Example.DataAccess/IDaos/ICompanyResourceDao.cs).

### Transactions

`TransactionStart`, `TransactionCommit` and `TransactionRollBack` live on the data access layer, not on a
DAO, so one transaction covers every write made through the instance whatever entity it touched.

> **Illustrative** — not currently present in the repo.

```csharp
using (IExampleDataAccess da = new ExampleDataAccess())
{
	da.TransactionStart();

	da.Insert(new Department { Name = "Temporary" });
	da.Insert(new Company { Name = "Also temporary" });

	da.TransactionRollBack();   // both writes are reversed, newest first
}
```

The in-memory implementation backs this with an undo log scoped to the DAL instance — a write reaches the
store immediately, so the only way back is to reverse each one in turn. Transactions do not nest: a second
`TransactionStart` throws `InvalidOperationException`, and so does committing or rolling back with nothing
open.

**One accepted limitation:** another DAL instance can read writes this one has not committed — the
equivalent of `READ UNCOMMITTED`. That is the price of an in-memory store with nowhere to put an
uncommitted row; a database-backed implementation gets isolation from its provider. A test pins it — traited
`Scope=Characterization`, because a DAL that does provide isolation fails it correctly — so a reader can
tell an accepted tradeoff from a defect.

### Disposal

`IBaseDataAccess` extends `IDisposable`, so every implementation supplies `Dispose` — `BaseDataAccess`
declares it abstract, which means even a DAL holding nothing disposable writes an explicit override.
`ExampleDataAccess.Dispose` is idempotent, never throws, and rolls back an open transaction: an unclosed
transaction is an abandoned one. Every other member throws `ObjectDisposedException` once disposed.

It deliberately does **not** clear the store. Disposing one data access layer no more empties the database
than closing one connection does. Dispose what you created; leave what you were handed alone.

### Passing `null` as a type selector

The `item` parameter on `GetAll`, `GetPaged` and `GetCount` is a **type selector only**. Its values are
never read, and it arrives as `null` whenever the call comes through the dispatcher.

Through a specific DAO interface you can pass `null` yourself. Through the aggregate you cannot write a
bare `null` — several `GetAll` overloads match and the call is ambiguous — so exercise the rule through one
DAO or through the dispatcher:

> **Illustrative** — not currently present in the repo.

```csharp
ICompanyResourceDao dao = new ExampleDataAccess();
dao.GetAll(null);              // fine: one overload, so there is nothing to disambiguate

IExampleDataAccess da = new ExampleDataAccess();
// da.GetAll(null);            // does not compile: too many GetAll overloads match
da.GetAll<Department>();       // the dispatcher passes the null selector for you
```

An implementation that reads `item` inside one of those three methods compiles happily and throws
`NullReferenceException` the first time a generic caller reaches it.

---

## The Convention Showcase

`ProphetsWay.Example.Tests/ConventionShowcase/` holds data access layers that are deliberately broken, one
mistake each, named for the mistake, so you meet each failure mode here rather than in your own code:

| DAL | The mistake |
|---|---|
| `MissingMethodDal` | The convention method is simply not declared |
| `WrongReturnTypeDal` | `GetAll` declares `IEnumerable<Company>`, which is not assignable to `IList<Company>` |
| `StaticMethodDal` | Correct in every respect except that the method is `static` |
| `BaseTypeParameterDal` | The parameter is a base class or an interface rather than the entity type |
| `IdentifierShowcaseDal` | A correct DAL, used to show what a badly shaped **entity** does |
| `ThrowingDal` | Correct, and throws — used to prove the exception arrives unwrapped |

Every one fails with `DataAccessConventionException`, and no test asserts on message text: the wording is
not part of the contract. These tests are traited `Scope=Dispatcher` rather than `Contract` — they pin the
reflection convention in `ProphetsWay.BaseDataAccess`, not any DAL, so they hold whatever implementation the
factory returns and are excluded from a `Scope=Contract` run. Two properties are worth knowing before you
write your own DAL.

- **The declared return type is checked before the method is invoked**, so a mis-declared `Update` or
  `Delete` cannot write to your database and only then report itself.
- **An exception your DAL throws arrives as itself.** Reflection's default is to wrap it in a
  `TargetInvocationException`; `ProphetsWay.BaseDataAccess` 3.0.0 removed that wrapper, and
  `ExceptionPassthroughShowcaseTests` is the regression guard.

```csharp
using (var dal = new MissingMethodDal())
{
	Should.Throw<DataAccessConventionException>(() => dal.Update<Company>(new Company()));
}
```

The full specification of the convention — method names and signatures, required visibility, required
declared return types, and how the identifier property is resolved — is the `<remarks>` on
`DataAccessConventionException` in
[ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess). This folder is the
illustrated companion to it, not a replacement.

---

## Architecture & Design Decisions

**Behavior is specified in prose, in the interface, in numbers.** `IDepartmentDao` carries 19 numbered
rules and `ICompanyResourceDao` carries 10, each followed by a `WHY` section. Two conforming DALs that
disagree about whether a negative `skip` throws or silently no-ops are two DALs that are not actually
interchangeable, so the contract answers it and the tests enforce the answer down both call paths.

**Business rules were kept out on purpose.** `Update` on a soft-deleted department is allowed. Refusing it
would be a *business rule*, and a DAL that quietly enforces policy of its own cannot be reasoned about from
the outside and cannot be replaced, because the replacement would have to rediscover the policy.

**The in-memory store is process-wide; the transaction is not.** `DataStore` stands in for the database
itself, not for a connection to it, which is why disposal does not clear it and why a rollback replays an
undo log rather than restoring a snapshot of the whole store — restoring would discard writes made by other
instances that were never part of this transaction. `IBaseDataAccess` scopes a transaction to the instance,
and the implementation follows that exactly.

**Surrogate keys are sequential, via `Interlocked.Increment`.** Which is also what a real identity column
does, and unlike a shared `Random` it is thread safe.

**Every test class that touches the store runs in one xUnit collection.** Every implementation here writes
to one process-wide store, so two classes running at once are two threads writing to the same tables, and
any assertion phrased over a whole set races. There used to be two collections: the join tests shared no
entity type with anything else, because they named synthetic company and resource identifiers and no row was
ever created for them. Rule 10 on `ICompanyResourceDao` ended that — a join must now name rows that exist,
so those tests insert `Company` and `Resource` rows of their own, which puts them against the exact
whole-set counts in `DataAccessTransactionTests`. One collection, `TestCollections.SharedStore`, is the
honest consequence; `BaseUnitTests.cs` carries the full reasoning and what splitting it again would take.
The `ConventionShowcase` classes carry no collection at all — their DALs never reach the store.

---

## Building & Testing Locally

```
git clone https://github.com/ProphetManX/ProphetsWay.Example.git
cd ProphetsWay.Example
dotnet restore
dotnet build
dotnet test
```

The whole solution — database project included — builds with the .NET CLI. No database server is required
to run the tests: they use the in-memory implementation by default.

Every test carries a `Scope` trait, so you can run a subset:

```
dotnet test --filter "Scope=Contract"
dotnet test --filter "Scope=Characterization"
dotnet test --filter "Scope=Dispatcher"
```

`Contract` is the set a new implementation of `IExampleDataAccess` has to pass — see
[Why the filter, and what it leaves out](#why-the-filter-and-what-it-leaves-out).

To tinker against real SQL Server, publish `ProphetsWay.Example.Database`.

### If you open this in Visual Studio

Visual Studio 2022 and Visual Studio 2026 cannot open `ProphetsWay.Example.Database`. Both are limited to
the legacy SSDT project format, which targets .NET Framework 4.x, and this project is SDK-style. The
solution file compounds it: it still registers the project under the legacy SSDT project type GUID
(`{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}`) while the project file itself is SDK-style, so Visual Studio
reaches for the legacy project system and then fails, typically with a target framework error mentioning
`net472`.

| Tool | Database project |
|---|---|
| .NET CLI (`dotnet build`) | Works, on any OS, and produces the `.dacpac` |
| VS Code | Works |
| SSMS 22 | Works — opens the project and builds the solution. The supported GUI path |
| Visual Studio 2022 / 2026 | Cannot open it |

You have three options:

- Use SSMS 22 or VS Code for the database project.
- Unload the database project in Visual Studio and work on the three C# projects. They build and test
  normally without it.
- Convert the `.sqlproj` back to the legacy SSDT format, giving up cross-platform CLI builds.

### Known gaps

- The database project has tables for `Companies`, `Jobs`, `Users`, `Transactions` and `Resources`. It has
  **no** `Departments` or `CompanyResources` tables, so the schema is behind the contracts. Anyone building
  a SQL-backed implementation of `IExampleDataAccess` has to add them.
- Uncommitted writes are visible to other DAL instances — see
  [Transactions](#transactions).
- The database project does not open in Visual Studio — see
  [If you open this in Visual Studio](#if-you-open-this-in-visual-studio).

---

## Contributing

Issues and pull requests are welcome at
[github.com/ProphetManX/ProphetsWay.Example](https://github.com/ProphetManX/ProphetsWay.Example).

If you change a behavioral rule, change it in the interface's `<remarks>` first — that is where the
contract lives — then in the tests, then in the implementation. A rule that exists only in an
implementation is not a rule.


## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/ProphetManX/ProphetsWay.Example/tags). 

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## Authors

* **G. Gordon Nasseri** - *Initial work* - [ProphetManX](https://github.com/ProphetManX)

See also the list of [contributors](https://github.com/ProphetManX/ProphetsWay.Example/graphs/contributors) who participated in this project.

## License

MIT — see the [LICENSE](LICENSE) file for details.


