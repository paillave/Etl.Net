create database TestEtl
go
use TestEtl
go
CREATE LOGIN TestEtl WITH PASSWORD = 'TestEtl.TestEtl';  
GO
CREATE USER TestEtl FOR LOGIN TestEtl;
GO  
ALTER ROLE [db_owner] ADD MEMBER [TestEtl]
GO
