USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Users ON

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
	)
WHEN NOT MATCHED BY Source THEN
	DELETE;

SET IDENTITY_INSERT dbo.Users OFF
