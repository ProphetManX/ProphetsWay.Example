USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Transactions ON

--Insert and update only. Rows outside this seed set are removed by PurgeSeedData.sql, which runs
--child-to-parent ahead of every seed - an order this MERGE is in no position to know about.
MERGE dbo.Transactions AS Target
USING (VALUES 
	(1, SYSUTCDATETIME(), 1, 2, 2000),
	(2, SYSUTCDATETIME(), 2, 1, 3000)	
) AS Source (Id, DateOfAction, UserId, CompanyId, Amount)
	ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET
		DateOfAction = Source.DateOfAction, 
		UserId = Source.UserId,
		CompanyId = Source.CompanyId, 
		Amount = Source.Amount
WHEN NOT MATCHED BY Target THEN 
	INSERT (Id, DateOfAction, UserId, CompanyId, Amount)
	VALUES (
		Source.Id, 
		Source.DateOfAction, 
		Source.UserId,
		Source.CompanyId, 
		Source.Amount
	);

SET IDENTITY_INSERT dbo.Transactions OFF
