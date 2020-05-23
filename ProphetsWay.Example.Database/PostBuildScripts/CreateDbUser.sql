USE [master]
GO

/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [testUser]    Script Date: 5/2/2019 10:07:38 AM ******/

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'testUser')
	CREATE LOGIN [testUser] WITH PASSWORD=N'TdGjIVuDNbPL8acHUum/I0XkYatB/HFEDNE8J6dmg6A=', DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO



USE [$(DatabaseInstance)]
GO

/****** Object:  User [testUser]    Script Date: 5/2/2019 10:08:33 AM ******/
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'testUser')
	CREATE USER [testUser] FOR LOGIN [testUser] WITH DEFAULT_SCHEMA=[dbo]
GO

