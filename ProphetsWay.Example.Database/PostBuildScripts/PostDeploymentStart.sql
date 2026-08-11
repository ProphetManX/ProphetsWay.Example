/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/


:r .\CreateDbUser.sql

--Must precede every seed. The seed scripts only insert and update; this is what removes.
:r .\PurgeSeedData.sql

:r .\CreateCompanies.sql
:r .\CreateJobs.sql
:r .\CreateUsers.sql
:r .\CreateTransactions.sql