USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Companies ON

--Insert and update only. Rows outside this seed set are removed by PurgeSeedData.sql, which runs
--child-to-parent ahead of every seed - an order this MERGE is in no position to know about.
MERGE dbo.Companies AS Target
USING (VALUES
	(1, 'ACME', 'Great Products for a Great Price!'), 
	(2, 'Dunder Mifflin', 'We sell Paper' )
) AS Source (Id, Name, Other)
	ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET
		Name = Source.Name,
		Other = Source.Other
WHEN NOT MATCHED BY Target THEN 
	INSERT (Id, Name, Other)
	VALUES (
		Source.Id, 
		Source.Name, 
		Source.Other
	);

SET IDENTITY_INSERT dbo.Companies OFF

