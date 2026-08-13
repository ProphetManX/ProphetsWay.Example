USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Users ON

--Insert and update only. Rows outside this seed set are removed by PurgeSeedData.sql, which runs
--child-to-parent ahead of every seed - an order this MERGE is in no position to know about.
MERGE dbo.Users AS Target
USING (VALUES 
	(1, 'Bugs', 'What''s up Doc?', 1, 2, 'User', 1),
	(2, 'Michael Scott', 'That''s what she said!', 2, 1, 'Admin', 0)	
) AS Source (Id, Name, Whatever, CompanyId, JobId, RoleStr, RoleInt)
	ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET
		Name = Source.Name, 
		Whatever = Source.Whatever,
		CompanyId = Source.CompanyId, 
		JobId = Source.JobId, 
		RoleStr = Source.RoleStr,
		RoleInt = Source.RoleInt
WHEN NOT MATCHED BY Target THEN 
	INSERT (Id, Name, Whatever, CompanyId, JobId, RoleStr, RoleInt)
	VALUES (
		Source.Id, 
		Source.Name, 
		Source.Whatever,
		Source.CompanyId, 
		Source.JobId, 
		Source.RoleStr,
		Source.RoleInt
	);

SET IDENTITY_INSERT dbo.Users OFF
