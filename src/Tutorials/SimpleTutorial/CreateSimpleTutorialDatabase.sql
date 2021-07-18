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

IF OBJECT_ID('[dbo].[ExecutionTrace]', 'U') IS NOT NULL
DROP TABLE [dbo].[ExecutionTrace]
GO
CREATE TABLE [dbo].[ExecutionTrace]
(
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- Primary Key column
    [ExecutionId] UNIQUEIDENTIFIER NOT NULL,
    [DateTime] DATETIME2 NOT NULL,
    [EventType] NVARCHAR(255) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
);
GO


IF OBJECT_ID('[dbo].[Person]', 'U') IS NOT NULL
DROP TABLE [dbo].[Person]
GO
CREATE TABLE [dbo].[Person]
(
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- Primary Key column
    [Email] NVARCHAR(255) NOT NULL UNIQUE, -- Business Key column
    [FirstName] NVARCHAR(50) NOT NULL,
    [LastName] NVARCHAR(50) NOT NULL,
    [DateOfBirth] DATE NOT NULL,
    [Reputation] INT NULL
);
GO
