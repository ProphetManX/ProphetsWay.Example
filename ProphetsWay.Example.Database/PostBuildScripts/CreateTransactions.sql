USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Transactions ON

MERGE dbo.Transactions AS Target
USING (VALUES 
	(1, GetDate(), 1, 2, 2000),
	(2, GetDate(), 2, 1, 3000)	
) AS Source (Id, DateOfAction, UserId, CompanyId, Amount)
	ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET
		DateOfAction = Source.DateOfAction, 
		UserId = Source.UserId,
		CompanyId = Source.CompanyId, 
		JobId = Source.Amount
WHEN NOT MATCHED BY Target THEN 
	INSERT (Id, DateOfAction, UserId, CompanyId, Amount)
	VALUES (
		Source.Id, 
		Source.DateOfAction, 
		Source.UserId,
		Source.CompanyId, 
		Source.Amount
	)
WHEN NOT MATCHED BY Source THEN
	DELETE;

SET IDENTITY_INSERT dbo.Transactions OFF
