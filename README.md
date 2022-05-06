# ProphetsWay.Example


Build Status:  
[![Build Status](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_apis/build/status/ProphetManX.ProphetsWay.Example?repoName=ProphetManX%2FProphetsWay.Example&branchName=main)](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_build/latest?definitionId=24&repoName=ProphetManX%2FProphetsWay.Example&branchName=main)

ProphetsWay.Example is a repository that consists of the following projects:
 - ProphetsWay.Example.DataAccess
 - ProphetsWay.Example.DataAccess.NoDB
 - ProphetsWay.Example.Database
 - ProphetsWay.Example.Tests

These projects are to be used by my other libraries I work on, used to illustrate functionality, and allow users to reference some of base class items
without having to write everything before tinkering on their own.

#### ProphetsWay.Example.DataAccess
Example.DataAccess is a simple implementation of a Data Access Layer (DAL) as specified by [ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess). 
It defines a set of Entities/Models that our Example project needs: Company, Job, and User.  There is also a definition of an Enum used within a Model, Roles.
Lastly it defines the interfaces that we can expect to interact with each Model, and a global interface to be used by anyone that flushes out a DAL implementor 
for this Example project.

Because I'm using [ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess) I have tagged all my Models with 
```IBaseIdEntity<int>``` which requires that all my Models will have a property named Id, and in this case is of type 'int'.  
I've also tagged all my Data Access Object (DAO) interfaces with each of  ```IBasePagedDao<Company>```, ```IBaseGetAllDao<Job>```, 
and ```IBaseDao<User>```; each of which define some basic functionalty like the Create, Read, Update, Delete (CRUD).  In this 
project those methods are defined as Insert, Get, Update, and Delete respectively.  Additionaly the interfaces define GetAll, GetPaged, 
and GetCount which are used differently for each Model.  Some DAOs specify extra functionality that is "required" by the project.

Lastly the IExampleDataAccess interface simply requires that all the Entity DAO interfaces are implemented, as well as ```IBaseDataAccess<int>```
which in used in tandem with other base classes within the [ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess) library.


#### ProphetsWay.Example.DataAccess.NoDB
This project was created soley for the purpose of Unit Testing this DAL and any implementation of it when a user doesn't have a database to test against.  
I currently have two libraries that are using this: [ProphetsWay.iBatisTools](https://github.com/ProphetManX/ProphetsWay.iBatisTools) and 
[ProphetsWay.EFTools](https://github.com/ProphetManX/ProphetsWay.EFTools).  By coding an implementation here, I can run Unit Tests locally, but more importantly
also within a build pipeline that won't require a database either.  

Nothing special about this implementation, I used a static class with internal dictionaries to mock up all the pieces of a database without having to write
to an actual database.


#### ProphetsWay.Example.Database
This project is a SQL Project for Visual Studio in .Net Framework 4.5.  It's a MS SQL Server implementation that can be used to create and deploy 
a version of this DALs database to SQL Server.  For anyone that wants to work with a database and tinker, this can be useful to get you setup
quickly without having to build everything yourself.  It includes some basic data to be inserted so the database isn't empty, but said data is 
just garbage/nonsense data.


#### ProphetsWay.Example.Tests
ProphetsWay.Example.Tests is probably the most useful part of the repository, as it is coded so that by default it uses the NoDB implementation
to execute the tests, but each DAO Test file can be overridden with ANY class that implements ```IExampleDataAccess```, meaning any other projects
that tinker with this Example project can build their own implementation using any backend of their choice, and so long as they implement 
```IExampleDataAccess``` you can use these unit tests to test your own implementation.


### Prerequisites
If you wish to implement your own version of ProphetsWay.Example then I would recommend pulling down a copy of [ProphetsWay.BaseDataAccess](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess)
for reference, there is a lengthly writeup of the project on the GitHub page.  You can pull a copy of the source code from GitHub, 
or you can reference the library from NuGet.org from within Visual Studio.

```
Install-Package ProphetsWay.BaseDataAccess 
dotnet add package ProphetsWay.BaseDataAccess 
```



## Running the tests

The library has 21 unit tests currently.  I tried to cover everything possible.  They are created with an XUnit test project, as well as an Example project.


## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/ProphetManX/ProphetsWay.Example/tags). 

## Authors

* **G. Gordon Nasseri** - *Initial work* - [ProphetManX](https://github.com/ProphetManX)

See also the list of [contributors](https://github.com/ProphetManX/ProphetsWay.Example/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details


