create database SimpleTutorial
go
use SimpleTutorial
go
CREATE LOGIN SimpleTutorial WITH PASSWORD = 'TestEtl.TestEtl';  
GO
CREATE USER SimpleTutorial FOR LOGIN SimpleTutorial;
GO  
ALTER ROLE [db_owner] ADD MEMBER [SimpleTutorial]
GO
