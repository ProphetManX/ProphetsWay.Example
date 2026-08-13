USE [$(DatabaseInstance)]
GO

/*
	Empties the seeded tables so the seed MERGEs that follow rebuild them from nothing. This is the
	only place the purge happens, and it runs before any seed script.

	The order is child-to-parent, dictated by the foreign keys - each table is only deletable once
	everything referencing it is already empty:

		CompanyResources	FK_CompanyResources_Companies, FK_CompanyResources_Resources
		Transactions		FK_Transactions_Users, FK_Transactions_Companies
		Users			FK_Users_Companies, FK_Users_Jobs, FK_Users_Departments
		Companies		referenced by Users, Transactions and CompanyResources
		Jobs			referenced by Users

	Departments and Resources are parents in that graph but have no seed script, so their rows are
	not ours to remove. If either is ever seeded, its purge belongs here - Departments after Users,
	Resources after CompanyResources.

	CompanyResources has no seed script either, so its seed set is empty and every row goes. It is
	purged rather than ignored because leaving it populated blocks the Companies purge outright.

	Whole tables rather than "everything outside the seed set", because a surviving seed row is free
	to point at a non-seed parent - a transaction kept for its Id still pinning the user it names.
	The seeds re-insert their rows with explicit identifiers under IDENTITY_INSERT, so the values
	CreateTransactions references survive the round trip unchanged.

	DELETE rather than TRUNCATE: TRUNCATE is illegal on a table a foreign key references, and it
	reseeds IDENTITY, which would put a later application insert back on Id 1 and collide with a
	seed row.

	Deleting from an empty table is a no-op, so a first deploy runs this clean.
*/

DELETE FROM dbo.CompanyResources;
DELETE FROM dbo.Transactions;
DELETE FROM dbo.Users;
DELETE FROM dbo.Companies;
DELETE FROM dbo.Jobs;
GO
