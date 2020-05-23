USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Jobs ON

MERGE dbo.Jobs AS Target
USING (VALUES
	(1, 'Owner', 'Either owns the company, or at least thinks he does!'), 
	(2, 'Lackey', 'Some Chump')
) AS Source (Id, Name, Something)
	ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET
		Name = Source.Name,
		Something = Source.Something
WHEN NOT MATCHED BY Target THEN 
	INSERT (Id, Name, Something)
	VALUES (
		Source.Id, 
		Source.Name, 
		Source.Something
	)
WHEN NOT MATCHED BY Source THEN
	DELETE;

SET IDENTITY_INSERT dbo.Jobs OFF

