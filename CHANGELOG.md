# v3.0.0
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
implementation gets its isolation from the provider.

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
Up from 2.3.0, and developed against ```3.0.0-481.Alpha```.  That release makes ```IBaseDataAccess``` extend 
```IDisposable```, so ```ExampleDataAccess``` now implements ```Dispose```.  It is idempotent, never throws, 
and rolls back an open transaction.  It deliberately does not clear the in-memory store, because here the 
store stands in for the database itself rather than for a connection to it.  The generic ```IBaseDataAccess<T>``` 
form was removed in 3.0.0, so ```IExampleDataAccess``` no longer refers to it.

### Fixed
- ```GetCustomCompanyFunction``` threw ```DivideByZeroException``` against an empty store, and silently 
treated a negative id as zero.
- Several DAOs generated surrogate keys from one shared static ```Random``` under different locks.  
```Random``` is not thread safe, and once its state is corrupted it can return ```0``` forever, producing 
duplicate keys.  Keys are now sequential via ```Interlocked.Increment```, which is also what a real identity 
column does.
- ```TransactionDao.GetCount``` and ```GetPaged``` were reading without taking the lock every other read takes.
- A test isolation race in ```ShouldGetGenericPaged```, and three more of the same shape that had not failed yet.

### Build and tooling
- Target frameworks for the Data Access Layer projects are now ```netstandard2.0;net48;net8.0;net9.0```, 
down from ```net461;net471;net48;net50;net60;net70;net80;net90```, and the tests target ```net48;net8.0;net9.0```.  
This strands nobody — ```netstandard2.0``` was added in the same change and covers every target dropped.
- The database project moved from the legacy SSDT format to the ```Microsoft.Build.Sql``` SDK, so the solution 
builds with ```dotnet build``` for the first time.  It previously failed on the ```.sqlproj```.
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