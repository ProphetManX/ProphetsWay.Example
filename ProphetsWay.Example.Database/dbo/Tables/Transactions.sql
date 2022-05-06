CREATE TABLE [dbo].[Transactions]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
	[DateOfAction] DATETIME NOT NULL DEFAULT GetDate(), 
	[UserId] INT NULL, 
	[CompanyId] INT NULL, 
	[Amount] DECIMAL NULL
)
