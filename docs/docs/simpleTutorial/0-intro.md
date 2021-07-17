---
sidebar_position: 1
---

# Simple: Introduction

![Quick start](/img/build-first-app-bot-tutorial.svg)

This is a detailed step by step tutorial of a typical simple ETL process.

- Unzip files with the name pattern `*.zip`
- Parse csv files with the name pattern `*.csv` from zip files
- Feed a table `Person` with the list of people from files
- Exclude duplicates
- Insert if the email is not found, update otherwise
- We won't use extensions for Entity Framework here but with extensions that directly deal with Sql Server

## Source Csv file

Files in zip files have the following format:

```csv
email,first name,last name,date of birth,reputation
tmp0@coucou.com,aze0,rty0,2000-05-10,45
tmp1@coucou.com,aze1,rty1,2000-01-11,145
tmp2@coucou.com,aze2,rty2,2000-02-12,245
tmp3@coucou.com,aze3,rty3,2000-03-13,345
tmp4@coucou.com,aze4,rty4,2000-04-14,445
```

## Target Table

```sql
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
```
