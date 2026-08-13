USE [$(DatabaseInstance)]
GO

SET IDENTITY_INSERT dbo.Jobs ON

--Insert and update only. Rows outside this seed set are removed by PurgeSeedData.sql, which runs
--child-to-parent ahead of every seed - an order this MERGE is in no position to know about.
MERGE dbo.Jobs AS Target
USING (VALUES
	(1, 'Owner', 'Either owns the company, or at least thinks he does!'), 
	(2, 'Lackey', 'Some Chump')
) AS Source (Id, Name, Something)
	ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET
		Name = Source.Name,
		Something = Source.Something
WHEN NOT MATCHED BY Target THEN 
	INSERT (Id, Name, Something)
	VALUES (
		Source.Id, 
		Source.Name, 
		Source.Something
	);

SET IDENTITY_INSERT dbo.Jobs OFF

