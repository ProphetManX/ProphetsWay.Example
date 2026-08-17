# v3.1.1 — not yet released
This line is open.  The owner will not tag it until everything ```ProphetsWay.EFTools``` needs from this 
repository is in place, so further work may land under this same number and the entry below describes the 
state of the line rather than the contents of a release.

Nothing in the two Data Access Layer projects changed behavior — no ```.cs``` file under 
```ProphetsWay.Example.DataAccess``` or ```ProphetsWay.Example.DataAccess.NoDB``` gained or lost a line of 
executable code.  What changed is the specification those projects carry, the test project that enforces it, 
and one seam in the test project that lets a different repository run the enforcement against its own 
implementation.  Read the second and third sections below before upgrading anything written against the 3.1.0 
contract text: one of them tightens what an implementation must do, and one of them withdraws something a 
caller was previously promised.

### A second repository can now point this suite at its own Data Access Layer
```TestDataAccessFactory``` gained ```Use```:

```c#
	public static void Use(Func<IExampleDataAccess> implementation)
```

Since 3.0.0 the factory has been the only place in the test project that names an implementation, and the 
promise attached to it was that changing its one ```return``` repoints the whole suite.  That promise held for 
anyone who could edit the file.  ```ProphetsWay.EFTools``` consumes this repository as a pinned git submodule 
under a standing instruction never to edit a file underneath it — so the one repository that most wants to run 
this suite against its own Data Access Layer was the one repository that could not.  ```Use``` is the way in, 
and it is what turns this repository's central claim — that the same tests pass against different 
implementations — into something checkable rather than asserted.

A consuming assembly writes one file and derives from nothing:

```c#
	internal static class TestSeam
	{
		[ModuleInitializer]
		internal static void PointTheSuiteAtEntityFramework()
		{
			TestDataAccessFactory.Use(() => new ExampleDataAccess(Constants.ConnectionString));
		}
	}
```

A delegate is taken rather than an instance, so construction inputs the default does not need — a provider, a 
connection string, a context factory — are closed over in the consumer and supplied nowhere else.  Nothing 
here assumes a parameterless constructor on the consumer's side, and the delegate is invoked once per 
instance rather than once in total, because the suite builds a fresh Data Access Layer for every test and 
several tests build a second alongside it.

**The seam latches on first use.** ```Use``` after ```Create``` has already handed an instance out throws 
```InvalidOperationException``` rather than swapping mid-run, because xUnit runs collections in parallel and 
a suite half-run against the wrong Data Access Layer reports a plausible mixture of passes and failures and 
names nothing.  Both methods take the same lock, so there are two outcomes and no third: either every 
instance in the run comes from the supplied delegate, or the call throws.  A ```[ModuleInitializer]``` is 
named above rather than a fixture because the runtime runs it before the test runner can construct a test 
class, whereas a class or collection fixture is constructed when its own collection starts and another 
collection may already be several tests in.  Passing null throws ```ArgumentNullException```; null is not a 
way to ask for the default back, because one seam set once is the whole value and a reset is a second seam.

**The default is unchanged and is still the one obvious line.** Nothing in this repository calls ```Use```, 
nothing consults an environment variable or a runner parameter, and the mis-wired Data Access Layers under 
```ConventionShowcase``` construct their own and are untouched by the seam — they are the subject of their 
tests rather than the implementation under test.  Reasoning and the constraints that shaped the mechanism are 
in ```docs/feature-requests.md``` entry 13.

### Two contract rules that now bind every Data Access Object
```IExampleDataAccess``` gained an **identifier rule** and a **row count rule**, bringing it to four 
Data-Access-Layer-wide rules alongside the snapshot and ordering rules.

The identifier rule: ```Insert``` assigns the identifier of the row it stored onto the instance the caller 
passed in, before it returns, so a caller reads the identifier off its own instance *after* the call.  It is 
the write-back the snapshot rule's closing sentence anticipates, and the only value ```Insert``` writes back 
unless a Data Access Object states another.  It reads the same for an ```int```, a ```long``` and a 
```Guid```.  What becomes of an identifier the caller pre-assigned is deliberately left unspecified — an 
identity column cannot honor a supplied value and a client-generated ```Guid``` reasonably can — so pass an 
entity with its identifier at its default and depend on neither answer.  ```ICompanyResourceDao``` is outside 
the rule because ```CompanyResource``` carries no identifier at all.

The row count rule: ```Update``` and ```Delete``` return ```1``` when the argument identified a row the 
operation applied to and ```0``` when it identified none — never negative, never more than one.  A return of 
```0``` throws nothing.  **The clause most easily lost is that ```Update``` reports that a row matched, not 
that a value changed**: updating a row with values identical to the ones already stored returns ```1```.  An 
implementation returning whatever its storage layer reports as rows-*modified* returns ```0``` there, while 
the same implementation over a layer reporting rows-*matched* returns ```1``` — two conforming Data Access 
Layers disagreeing about the same call, which is the exact failure this repository exists to disprove.

**Both are conventions elected here, not changes to ```ProphetsWay.BaseDataAccess```.** That package 
documents the identifier write-back as "a convention left to the implementation — this library neither 
performs it nor verifies that it happened", and describes the counts as "typically 1".  Those statements are 
correct, unchanged, and right for a library that cannot know what its implementations store.  What these two 
rules do is commit *this* Data Access Layer to the convention.  Do not read them as a promise the base 
package now makes.

If you have written an implementation of ```IExampleDataAccess``` against the 3.1.0 text, this is the part 
that can cost you work.  ```IDepartmentDao``` and ```ICompanyResourceDao``` already stated both rules for 
their own entities; ```ICompanyDao```, ```IJobDao```, ```IUserDao```, ```ITransactionDao``` and 
```IResourceDao``` left them unsaid while the test suite asserted them anyway.  Those five now say so, and 
each carries the rules restated in its own terms.

### Rule 18 narrowed — an included Department carries no promised DateTimeKind
```IDepartmentDao``` rule 18 said that ```CreatedDate```, ```UpdatedDate``` and ```DeletedDate``` carry a 
```DateTimeKind``` of ```Utc``` both on the instance written back to the caller and on an instance later 
retrieved.  **The retrieval half now binds that interface's own reads only** — ```Get```, ```GetAll``` and 
```GetPaged``` on ```IDepartmentDao```.  It expressly does not bind a ```Department``` reached as a 
navigation property of an entity retrieved through another Data Access Object, such as ```User.Department``` 
on a user returned by ```IUserDao```, which carries whatever kind the provider supplied — typically 
```Unspecified``` from a relational one.  Relational providers do not persist a ```DateTimeKind```, and 
restoring one is a per-Data-Access-Object mechanism that a hard Data Access Object such as ```UserDao``` does 
not have for these three timestamps.

**State the cost plainly, because the failure is silent.** An ```Unspecified``` value handed to 
```ToLocalTime``` is taken for local time and shifted by the machine's offset.  Nothing throws; you get a 
wrong timestamp.  A caller reading one of these three stamps off an included department must apply 
```DateTime.SpecifyKind(value, DateTimeKind.Utc)``` explicitly rather than trust the kind it finds:

```c#
	var user = da.Get(new User { Id = id });
	var created = DateTime.SpecifyKind(user.Department.CreatedDate.Value, DateTimeKind.Utc);
```

This is the same shape as rule 9, which is equally why a department reached through ```User.Department``` 
comes back populated even when it is soft-deleted: an include sits outside the mechanisms the retrieving Data 
Access Object applies to its own reads.  It is an owner decision, recorded in ```docs/feature-requests.md``` 
entry 14 with the alternative that was declined and why.

**This is a breaking change to the specification**, and it is worth being blunt about which direction it 
breaks.  An implementation that could not restore the kind on an include was non-conforming under the old 
text and is conforming under the new one, so nothing that was passing starts failing.  The party who loses is 
the *caller* who read the old rule and wrote code trusting it.  Nothing is published to nuget.org, so no 
restore breaks and no assembly needs rebuilding — the artifact this repository ships is the contract text, 
and the contract text withdrew a promise.

### Four new tests, one retraited, and the counts that follow
The suite is **164 tests on each of the ```net48``` and ```net10.0``` legs — 328 executions** — partitioned 
```Contract``` 139, ```Characterization``` 5, ```Dispatcher``` 20.  The three sum to the total, and that sum 
is the check: a mismatch means a test is untraited or double-traited.

Two of the new tests are ```Contract``` and close a gate hole.  ```dotnet test --filter "Scope=Contract"``` 
is offered as *the* conformance gate for a newly written Data Access Layer, and two implementation shortcuts 
passed all 138 of the previous ones.  ```ShouldNotWriteRelatedRowsWhenUpdateIsGivenAnEditedNavigationGraph``` 
edits ```Company```, ```Job``` and ```Department``` through a retrieved user's navigation properties, calls 
```Update```, and asserts the three rows the caller never named are unchanged while the row it did name 
carries its edit — a Data Access Layer that attaches the incoming graph as modified, the natural shortcut for 
an Entity Framework implementation, rewrites reference data every other user sharing those rows can see.  
```ShouldNotAdoptTheInstanceHandedToCustomUserFunctionality``` pins the one thing ```IUserDao``` does state 
about that member, that the instance is read rather than adopted, which an implementation written as 
```_users[user.Id] = user``` violates while satisfying every other assertion in the suite.

The other two new tests are ```Characterization```, and one existing test moved from ```Contract``` to 
```Characterization```.  A ```Contract``` assertion is an obligation placed on every future implementer, so 
one that traces to no stated rule does not belong in that scope.  ```ShouldGetCustomFunctionality``` was 
asserting a ```private const``` of the in-memory ```UserDao``` that no interface names, and the uncommitted 
half of the rolled-back-transaction navigation test was asserting a row shape only a denormalizing store can 
produce; that test was split, its contract half kept and renamed, and its characterization half given its own 
name.  ```ShouldCallCustomUserFunctionality``` was separated out for the same reason, so that an 
implementation whose custom member throws is reported as characterization rather than as a contract failure.

```SnapshotDeepCopyTests``` and ```UserDaoTests``` consequently declare ```Scope``` per method rather than on 
the class, joining ```CompanyDaoTests``` and ```DataAccessTransactionTests```.  That is required rather than 
stylistic: **xUnit accumulates traits rather than letting a method override its class**, so a class-level 
```Contract``` on a class with any ```Characterization``` test leaves that test selected by 
```--filter "Scope=Contract"``` no matter what the method declares.

### Two helpers xUnit was warning about
```EditEveryFieldAfterTheCall``` and ```AssertEveryStampIsUtc``` in ```DepartmentDaoTests``` were 
```public static``` and are now ```private static```.  Neither is a test — they are a shared edit and a 
shared assertion — and the visibility was the defect that made the analyzer say otherwise.  That clears four 
```xUnit1013``` warnings, two on each leg, and **the build is now warning-free**.

### Documentation
```AGENTS.md```, ```docs/repo-profile.md```, ```docs/purpose-and-scope.md``` and 
```docs/feature-requests.md``` were re-verified against source rather than against each other, and corrected.  
Superseded suite counts were swept out, and the description of the ```ProphetsWay.EFTools``` submodule 
pointer was corrected — it had been described as a vendored copy pinned before 3.0.0, and it is a submodule 
now pinned at 3.1.0.  Entry 11 is closed as ```Done```; entries 13 and 14 were filed for the seam and the 
rule 18 narrowing.

### Verification
164 tests passing on each leg independently — ```net10.0``` and ```net48``` — for 328 executions, with the 
three scope filters run separately and returning 139, 5 and 20.  A clean rebuild of the test project reports 
0 warnings and 0 errors.


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