CREATE TABLE [dbo].[Departments] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (MAX) NULL,
    [Description] VARCHAR (MAX) NULL,
    [CreatedDate] DATETIME2 (7) NOT NULL,
    [UpdatedDate] DATETIME2 (7) NULL,
    [DeletedDate] DATETIME2 (7) NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED ([Id] ASC)
);
