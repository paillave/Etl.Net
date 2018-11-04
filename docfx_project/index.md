# Etl.Net

## What it is

Etl.Net is a complete ETL for .net developers who need to embed SSIS-like features into their applications.

It contains every transformation and capabilities of SSIS:

- Data stream transformations like `derived column`, `convert`, `union`, `join`, `lookup`, `pivot`, `unpivot`, `aggregate`...
- Data sources like Excel, flat files such as csv, SQL Server, Entity framework...
- Tracing the complete activity with automatic filtering and saving capability
- Parameterization of a process

## Why it was made

### The ETL reference for Microsoft developers: SSIS

> SSIS is a fantastic tool.

SSIS is a great straight forward tool to process big chunks of data. It supports out of the box a lot of formats, protocols, and databases. It contains a quiet exhaustive mass processing operators. It is very fast. It exposes a very well done server for scheduling, parameterization, tracing, and activity reporting. Its development tool is very clear. Deployments are very simple and straight forward procedures...

> So... Why Etl.net?

### It is free of charges

> Nothing to pay.

SSIS makes part of SQL server. SQL Server, even if it makes part of the less expensive database engine, is not free of charges.

### It is multiplatform

> Can be run on nearly any kind of computer or even portable device.

Development are made on .Net Standard 2.0. This permits Etl.Net to be used on any platform that supports .net: Windows, Linux or mac with .net core or even mobile devices with application that are implement with xamarin.

### It is open source

> Get advantages of the open source community.

Anybody can push a new request for it to be improved so that some developers may implement it for you. You can even add your own amendments for them to be candidates as new features at the next release. Of course, open source involves complete transparency about what Etl.Net does under the hood.

### It is made for developers

> Out of a business intelligence context, SSIS can be very cumbersome.

When it is about including ETL process in an applications, many developers don't even consider using business intelligence tools such as SSIS. This is fairly understandable given the big commitment it involves:

- SSIS packages are in a different location than the application it self. It is not within the application sources, and it won't be deployed in the same way and at the same place
- Executing the full solution on a development computer for debug or even simple development purpose is complex. Usually, once source of an application are retrieved from source control, a hit on F5 should be sufficient to successfully execute the application. With an SSIS process, developing the solution needs an SSIS deployment somewhere prior to execute the application, with all the necessary setups. During debug of the application it self, it won't be possible to enter into debug of the ETL process if it is hosted in SSIS.
- SSIS needs to be installed on development computers. This makes the solution more complex to apprehend where normal application simply need dotnet core + visual studio code or visual studio community to be executed with a simple F5. Moreover, depending on the context, installing SSIS on a development computer is not always easy or possible (security policies).

> A regular developer needs to download his sources, hit F5, and debug.

### It is simple and easy... really

> Simply add references to Nuget

Just add a reference to the core package (Etl.Net) and some of its extensions depending on the need (Etl.Net.TextFile, Etl.Net.EntityFrameworkCore...) from Nuget and you can develop and execute on the go. The rest is a very simple development extremely close to Linq.

### It is designed to be very simple to extend

> SSIS extension is a horribly complex process compared to usual .net tool kits and frameworks.

When it is about SSIS extensibility , you have to swim quiet deep in the abyss of hell: after dealing with the very complex SDK to extend it and the big amount of lines of codes to implement your extension, you will have to ensure it is well deployed in the GAC of your development computer, but also on the GAC of every SSIS server where it will be deployed. To use it for development, you must also install it correctly on your development computer so that Sql Server Data Tools for BI recognizes it and makes it available in your visual studio toolbox.

> An Etl.Net extension can be developed, debugged and used within in 5mn... literally.

Etl.net extensibility is based on a simple development wether it is embedded within the application, in another shared assembly or a Nuget package. It is as simple as a couple of lines of code since a set of base classes permits extensions to be easily implemented for most of typical use cases.

## Installation

No installation is necessary. Simple reference to a nuget packages are enough.

# [dotnet core](#tab/dotnetcore)

Powershell:

```powershell
dotnet add package etl.net
```

# [dotnet framework](#tab/dotnetframework)

Powershell:

```powershell
nuget install etl.net
```

***

## Quickstart

### Prepare the application

Command line:

```powershell
dotnet new console -n SimpleQuickstart
cd SimpleQuickstart
dotnet add package Etl.Net --version 1.0.124-alpha
dotnet add package Etl.Net.TextFile --version 1.0.124-alpha
```

### Implement the process

[!code-csharp[Main](../src/Samples/SimpleQuickstart/Program.cs)]
