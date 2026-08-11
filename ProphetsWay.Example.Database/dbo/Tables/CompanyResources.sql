CREATE TABLE [dbo].[CompanyResources] (
    [CompanyId]  INT              NOT NULL,
    [ResourceId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_CompanyResources] PRIMARY KEY CLUSTERED ([CompanyId] ASC, [ResourceId] ASC),
    CONSTRAINT [FK_CompanyResources_Companies] FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Companies] ([Id]),
    CONSTRAINT [FK_CompanyResources_Resources] FOREIGN KEY ([ResourceId]) REFERENCES [dbo].[Resources] ([Id])
);
