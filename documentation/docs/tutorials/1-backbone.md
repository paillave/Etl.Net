---
sidebar_position: 1
---

# Backbone

![TrackCheck](/img/azure-app-service-platform-bot-construction.svg)

This is a detailed step by step tutorial of a typical simple ETL process.

- Unzip files with the name pattern `*.zip`
- Parse csv files with the name pattern `*.csv` from zip files
- Feed a table `Person` with the list of people from files
- Exclude duplicates
- Insert if the email is not found, update otherwise
- We won't use extensions for Entity Framework here but with extensions that directly deal with Sql Server

Csv files in zip files have the following format:

```csv
email,first name,last name,date of birth,reputation
tmp0@coucou.com,aze0,rty0,2000-05-10,45
tmp1@coucou.com,aze1,rty1,2000-01-11,145
tmp2@coucou.com,aze2,rty2,2000-02-12,245
tmp3@coucou.com,aze3,rty3,2000-03-13,345
tmp4@coucou.com,aze4,rty4,2000-04-14,445
```

The target table is the following:

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

## Create the project

```sh
dotnet new console -o SimpleTutorial
cd SimpleTutorial
```

Add reference to `Etl.Net`, the core of ETL.NET with all its common operators:

```sh
dotnet add package Paillave.EtlNet.Core
```

## Create an empty process

First, create an empty ETL process definition.

This process definition is a method that must receive as a parameter a stream of a single element that is the transmitted value when the process is run. In our situation this value is a `string` that represents the path where to find zip files.

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
    // TODO: Define the ETL process here
}
```

## Create the runner

We will create a runner with `StreamProcessRunner` from the package `Paillave.EtlNet.ExecutionToolkit` by providing the ETL process.

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
```

## Trigger the runner

Once we have the runner, we can trigger it by providing the value of the single initial event.

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
var res = await processRunner.ExecuteAsync(args[0]);
```

## Catch the success or failure of an execution

The execution returns an objects that gives information about the execution like the fact a failure occurred.

```cs
Console.Write(res.Failed ? "Failed" : "Succeeded");
```

## Full source code at this stage

We now have the backbone of a console application that run an ETL process

```cs title="Program.cs"
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            var res = await processRunner.ExecuteAsync(args[0]);
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            // TODO: Define the ETL process here
        }
    }
}
```
