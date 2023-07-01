create database BlogTutorial
go
use BlogTutorial
go
CREATE LOGIN BlogTutorial WITH PASSWORD = 'TestEtl.TestEtl';  
GO
CREATE USER BlogTutorial FOR LOGIN BlogTutorial;
GO  
ALTER ROLE [db_owner] ADD MEMBER [BlogTutorial]
GO
