CREATE TABLE [dbo].[Companies] (
    [Id]    INT           IDENTITY (1, 1) NOT NULL,
    [Name]  VARCHAR (MAX) NULL,
    [Other] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED ([Id] ASC)
);

