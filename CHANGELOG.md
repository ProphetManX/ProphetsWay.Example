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