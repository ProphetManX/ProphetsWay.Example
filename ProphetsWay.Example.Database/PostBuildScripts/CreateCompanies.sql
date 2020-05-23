USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Companies ON

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
	)
WHEN NOT MATCHED BY Source THEN
	DELETE;

SET IDENTITY_INSERT dbo.Companies OFF

