CREATE TABLE [dbo].[Jobs] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (MAX) NULL,
    [Something] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_Jobs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

