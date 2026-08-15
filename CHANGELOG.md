# v3.1.0
### The test suite did not change, and that is the point
Not one ```.cs``` file was touched in this release.  The 160 tests are byte-identical to the ones 3.0.0 
shipped, so when they pass against the retargeted projects they are evidence *about* the retarget rather than 
a result the retarget could have shaped.  A suite edited in the same change as the build it validates proves 
only that someone made it green.

The whole release is target frameworks, one dependency bump, one dead property removed, one connection string 
generalized, and documentation.  Behavior is unchanged.

### Target frameworks
```ProphetsWay.Example.DataAccess``` and ```ProphetsWay.Example.DataAccess.NoDB``` moved from 
```netstandard2.0;net48;net8.0;net9.0``` to ```netstandard2.0;net10.0```, and ```ProphetsWay.Example.Tests``` 
from ```net48;net8.0;net9.0``` to ```net48;net10.0```.  The test project also gained explicit 
```RootNamespace```, ```AssemblyName``` and ```IsTestProject``` values.

.NET 8 and .NET 9 both reach end of life on 10 November 2026, so neither is worth carrying into a release 
made now.  Nobody is stranded by dropping them: ```netstandard2.0``` remains, and every runtime that resolved 
a ```net8.0``` or ```net9.0``` asset resolves that one instead.

The explicit ```net48``` **library** target went with them, for a different reason — it was never earned.  
```netstandard2.0``` already reaches .NET Framework 4.8, so a second assembly compiled for it added build time 
and bought nothing.

The ```net48``` **test** leg was deliberately kept, and it is stronger now than it was.  With no ```net48``` 
library asset to bind to, it binds the Data Access Layer's ```netstandard2.0``` output — the exact assembly a 
.NET Framework consumer actually receives.  That leg is also the only place 
```Activator.CreateInstance<T>()``` exception-passthrough behavior is verified, since .NET Framework wraps a 
throwing constructor there and .NET Core does not.  The library/test target lists differing is deliberate, 
not drift.

### Now requires ProphetsWay.BaseDataAccess 3.1.0
Up from 3.0.0.  That release retargets the package to ```netstandard2.0;net10.0``` on the same reasoning; the 
contracts are unchanged.

### Removed a property that never did anything
```ProphetsWay.Example.DataAccess.csproj``` carried 
```<NullableContextOptions>enable</NullableContextOptions>```.  That was the .NET Core 3.0 *preview* spelling 
of the property that shipped as ```<Nullable>```, and MSBuild ignores it entirely — nullable reference types 
were never actually on, whatever the file appeared to claim.  It is deleted rather than corrected, because 
```netstandard2.0``` pins the shared compilation to C# 7.3 and nullable reference types cannot be enabled 
there anyway.  Removing it makes the file honest about what the compiler is doing.

### The publish profile no longer names a machine
```ProphetsWay.Example.localhost.publish.xml``` had ```Data Source=Terebellum``` in it, which worked on 
exactly one computer.  It is now ```Data Source=localhost```, with Integrated Security and no credentials.  
The file stays tracked on purpose — a repository whose job is to be read benefits from shipping a publish 
profile that works.

### Documentation
Two new documents live under ```docs/```.  ```purpose-and-scope.md``` states what this repository is for and 
the bar a change has to clear to belong in it.  ```feature-requests.md``` is a durable index, entries 1–9, of 
what was considered and what was decided.

Entries 1–4 are four contract behaviors this repository does not demonstrate.  They were previously restated 
as prose in ```AGENTS.md```, where they read as an open list with no owner; each now carries a status and the 
reasoning behind it — two Rejected, one Deferred, one Proposed.  Ending that duplication is the reason they 
moved.  ```docs/repo-profile.md``` and the per-repository section of ```AGENTS.md``` were refreshed to match.

### Verification
160 tests on each of the two legs, ```net48``` and ```net10.0``` — 320 executions, all green, with both legs 
confirmed to have run independently rather than inferred from a combined total.  Azure DevOps build 
```3.1.0.496```, both checks passing.


# v3.0.0
### Pointing the whole test suite at a different Data Access Layer is now one line
```TestDataAccessFactory``` is the only place in the test project that names an implementation.  
```BaseUnitTests<T>``` used to declare ```protected abstract T GetIExampleDataAccess```, so every test 
class supplied its own instance and swapping implementations meant editing about a dozen files.  Changing 
the single ```return``` in ```TestDataAccessFactory.Create``` now points all 160 tests at another Data 
Access Layer — which is what this repository has always claimed and could not actually deliver.

Every test also carries an xUnit ```Scope``` trait, with none left untagged:

```
	dotnet test --filter "Scope=Contract"
```

- ```Scope=Contract``` — 138 tests, the ones any conforming implementation has to pass.  That filter is what 
a newly written Data Access Layer runs against itself.
- ```Scope=Characterization``` — 2 tests, pinning behavior this implementation chose and the contract does 
not require, so another implementation may fail them and still conform.  
```ShouldExposeUncommittedWritesToAnotherInstance```, because an in-memory store offers no isolation and a 
database does, and ```ShouldGetCustomCompanyFunction```, because this implementation reads the argument as 
a position in the set and wraps round the end, while the interface says nothing about what it means.
- ```Scope=Dispatcher``` — 20 tests, exercising the reflection convention in ```ProphetsWay.BaseDataAccess``` 
rather than any Data Access Layer at all.

The two xUnit collections merged into one ```SharedStore```.  Rule 10 below means the join tests now insert 
real ```Company``` and ```Resource``` rows, which race the exact whole-set ```Company``` counts in the 
transaction tests.  The suite's only parallelism was the cost.

### Two contract rules that now bind every Data Access Object
```IExampleDataAccess``` gained a Data-Access-Layer-wide **ordering rule**.  The order ```GetAll``` and 
```GetPaged``` return entities in is unspecified, but it is stable across calls for as long as the stored 
data is unchanged, so successive paged windows partition a full pass with no overlap and no omission.  It is 
the general form of ```IDepartmentDao``` rule 11 and it is worth saying why it is written down: an in-memory 
store satisfies it incidentally, through the insertion order of the dictionary holding its rows, while SQL 
Server guarantees no order at all without an explicit ```ORDER BY```.  A SQL-backed implementation that 
omits one passes every test today and starts failing intermittently at some future row count.

```ICompanyResourceDao``` gained **rule 10**: a caller must name a company that exists and a resource that 
exists.  That replaced a promise that referential integrity may or may not be enforced — wording under which 
two conforming implementations could behave differently for the same call, which is the exact failure this 
repository exists to disprove.  The check itself stays optional for stores that cannot perform one, but a 
call that succeeds against such a store is no evidence the rows are there, and a caller relying on the old 
leniency is writing code that will not port.

### Two new entities showcasing shapes the paradigm supports
```Department``` is the first soft-delete entity in this example, implementing ```IBaseSoftIdEntity<int>```.
```Delete``` stamps ```DeletedDate``` instead of removing the row, ```Get``` still returns a deleted department 
so a foreign key reference still resolves, and ```GetAll```, ```GetPaged``` and ```GetCount``` leave it out.  
```User``` now carries a ```Department``` navigation property so that last point is demonstrated rather than 
claimed.  ```IDepartmentDao``` also declares a custom ```Restore``` method, included purely to illustrate that 
you can add whatever members your own domain needs — it is not a pattern the paradigm asks you to follow.

```CompanyResource``` is a join entity with no identifier at all, implementing only ```IBaseEntity```.  Its DAO 
deliberately does not inherit ```IBaseDao<T>``` and declares just ```Insert```, ```Delete``` and ```GetAll```, 
to show that the DAO interfaces are a menu you pick from rather than a set you must implement.  It is 
documented as a novelty, not as the norm.

### Transactions are actually demonstrated now
```TransactionStart```, ```TransactionCommit``` and ```TransactionRollBack``` previously threw 
```NotImplementedException```.  They now work, backed by an undo log scoped to the Data Access Layer instance, 
so the paradigm's transaction contract finally has a working reference implementation to read.  One documented 
limitation is accepted: another Data Access Layer instance can see writes that have not been committed, the 
equivalent of ```READ UNCOMMITTED```.  That is the price of an in-memory store — a database-backed 
implementation gets its isolation from the provider.  The test pinning that is tagged 
```Scope=Characterization```, so an implementation that does provide isolation fails it correctly.

### Breaking: every DAO now returns snapshots
Five of the seven DAOs used to alias.  The store held the very instance you passed in, and ```Get``` handed 
that same instance back, so editing a retrieved entity silently changed stored data.  Every DAO now returns 
deep snapshots and reads its arguments rather than adopting them.  Mutating a retrieved entity no longer 
reaches the store, and neither does mutating an argument after the call returns, nor mutating a nested entity 
such as ```user.Company```.  Two entities retrieved separately for the same row are now independent instances.

This is what makes a rollback actually work.  Under aliasing, fetching a row, editing it and calling 
```Update``` had already changed the store before ```Update``` ran, so a rollback restored the edit instead of 
reversing it.  Three tests were depending on that behavior and have been corrected.

Two other behavior changes come with it.  ```Update``` against an identifier that matches nothing now returns 
```0``` and stores nothing, where it previously inserted.  ```Update``` and ```Delete``` across all DAOs now 
return the real number of rows affected instead of a hardcoded ```1```.

### Now requires ProphetsWay.BaseDataAccess 3.0.0
Up from 2.3.0.  That release makes ```IBaseDataAccess``` extend ```IDisposable```, so ```ExampleDataAccess``` 
now implements ```Dispose```.  It is idempotent, never throws, and rolls back an open transaction.  It deliberately 
does not clear the in-memory store, because here the store stands in for the database itself rather than for 
a connection to it.  The generic ```IBaseDataAccess<T>``` form was removed in 3.0.0, so ```IExampleDataAccess``` 
no longer refers to it.

### Fixed
- ```GetCustomCompanyFunction``` threw ```DivideByZeroException``` against an empty store, and silently 
treated a negative id as zero.
- Several DAOs generated surrogate keys from one shared static ```Random``` under different locks.  
```Random``` is not thread safe, and once its state is corrupted it can return ```0``` forever, producing 
duplicate keys.  Keys are now sequential via ```Interlocked.Increment```, which is also what a real identity 
column does.
- ```TransactionDao.GetCount``` and ```GetPaged``` were reading without taking the lock every other read takes.
- A test isolation race in ```ShouldGetGenericPaged```, and three more of the same shape that had not failed yet.

### The database project can back the contracts now
```Departments``` and ```CompanyResources``` were added — the latter with a composite primary key on the 
pair and foreign keys to both parents — along with ```Users.DepartmentId``` and its foreign key, so the two 
new entities have somewhere to live.

Three corrections to what was already there.  ```Transactions.Amount``` was ```DECIMAL``` with no precision, 
which SQL Server reads as ```DECIMAL(18, 0)``` — zero decimal places — silently rounding away the fractional 
part of every amount stored; it is now ```DECIMAL(19, 4)```.  ```Transactions.DateOfAction``` moved from 
```DATETIME```/```GetDate()``` to ```DATETIME2(7)```/```SYSUTCDATETIME()```, and the table gained foreign 
keys to ```Users``` and ```Companies```.  ```Resources.Name``` went from ```VARCHAR(50) NOT NULL``` to 
```VARCHAR(MAX) NULL```, having previously rejected a ```Resource``` the in-memory implementation accepts.

### Seed data is purged child-to-parent
Each seed script used to delete the rows outside its own set, in whatever order the scripts happened to run 
— parents before children.  Once the new foreign keys existed that made the *second* deployment against any 
used database fail with error 547 and roll back, and with no purge for ```CompanyResources``` at all, fail 
that way permanently.  This is a defect the build cannot catch.  A new ```PurgeSeedData.sql``` now empties 
the tables child-to-parent ahead of every seed, and the ```WHEN NOT MATCHED BY Source THEN DELETE``` clauses 
came out of the four seed scripts so the ordering lives in exactly one place.

```CreateTransactions.sql``` was never referenced by the deployment chain.  It is now, and the 
```JobId = Source.Amount``` in its ```WHEN MATCHED``` branch — assigning to a column ```Transactions``` does 
not have — is corrected to ```Amount = Source.Amount```.  It had been dead code carrying a latent error, 
invisible because post-deployment scripts are not validated by the build.

### Build and tooling
- Target frameworks for the Data Access Layer projects are now ```netstandard2.0;net48;net8.0;net9.0```, 
down from ```net461;net471;net48;net50;net60;net70;net80;net90```, and the tests target ```net48;net8.0;net9.0```.  
This strands nobody — ```netstandard2.0``` was added in the same change and covers every target dropped.
- The database project moved from the legacy SSDT format to the ```Microsoft.Build.Sql``` SDK, so the solution 
builds with ```dotnet build``` for the first time.  It previously failed on the ```.sqlproj```.  The cost is 
that neither Visual Studio 2022 nor 2026 can open an SDK-style ```.sqlproj``` — a Visual Studio limitation, 
not a defect here.  SSMS 22, VS Code and the .NET CLI all work, and a Visual Studio user can otherwise unload 
the database project and work on the three C# projects, which build and test normally without it.
- FluentAssertions was replaced with Shouldly, as FluentAssertions 8.x requires a paid commercial licence.
- Removed unused ```Newtonsoft.Json``` and ```Microsoft.VSSDK.BuildTools``` references, and dropped a 
```LangVersion``` pin.
- The test count went from 35 to 160.  That includes a ```ConventionShowcase``` folder of deliberately 
mis-wired Data Access Layers demonstrating what ```DataAccessConventionException``` catches, and a check that 
an exception thrown by your own Data Access Layer arrives unwrapped rather than buried in a 
```TargetInvocationException```.

# v2.2.0
### Updated to support .net 7.0, 8.0, and 9.0
Updated the target frameworks to include .net 7.0, 8.0, and 9.0.  Removed support for the deprecated versions that are end of life.  No functional changes have been made.

# v2.1.4
### Fixing a problem found when trying to build/test a physical DB instance
Updating db creation scripts, and unit tests to account for more realistic usage situations.

# v2.1.3
### Missing Tables in DB Project
The Database project had two new tables ```Resources``` and ```Transactions```, but they weren't included 
in the solution/project.  That has been fixed.

# v2.1.2
### Added support for generic CRUD calls

# v2.1.1
### Updated to include 2 new entities that have unique primary key types
- Newly created entities, ```Transaction``` and ```Resource``` to support ```long``` and ```Guid``` primary key types respectively.
- Unit tests have been created as well.
- Updated pipeline to support new yml pipeline repo
- Added support for .Net 6.0



# v2.0.0
### Updated to support .net 5.0
Updated a few things, unfortunately it removed a little bit of functionality, so it counts as a major update, 
even tho it's really quite a minor update.
- Updated projects to support and target .net Framework 5.0, and remove targets for frameworks that are no longer supported
by Microsoft.
- Updated reference for ProphetsWay.BaseDataAccess to v2.0.0


# v1.0.1
Updated reference for ProphetsWay.BaseDataAccess to v1.1.0, and now the DataAccess and Tests projects 
also target .Net Framework 4.8.



# v1.0.0
### Initial proper release.  
Implements all the structural components to help illustrate how to best build a DAL that is based off the 
[ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess) defined paradigm.