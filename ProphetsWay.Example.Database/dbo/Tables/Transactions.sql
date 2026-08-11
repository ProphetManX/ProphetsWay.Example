CREATE TABLE [dbo].[Transactions] (
    [Id]           BIGINT          IDENTITY (1, 1) NOT NULL,
    [DateOfAction] DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
    [UserId]       INT             NULL,
    [CompanyId]    INT             NULL,
    [Amount]       DECIMAL (19, 4) NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Transactions_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Transactions_Companies] FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Companies] ([Id])
);
