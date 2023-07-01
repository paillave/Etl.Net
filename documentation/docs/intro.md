---
sidebar_position: 1
---

# Introduction

![Quick start](/img/build-first-app-bot-tutorial.svg)

Let's discover **ETL.NET in 5 minutes**.

## What is ETL.NET?

ETL.NET is a set of .NET libraries that permits to embed regular business intelligence ETL features into any .NET application.

It contains every transformation and capability of SSIS:

- ETL.NET provides every operator that is necessary to make ANY transformation of any data source. Implemented operators of ETL.NET are inspired by the ones that are offered by SQL: `Map`, `Join`, `Sort`, `Distinct`, `Lookup`, `Top`, `Pivot`, `Cross Apply`, `Union`, `Group By`, `Aggregate`, ...
- Data sources like Excel, flat files such as csv, SQL Server, Xml, Entity framework...
- Tracing the complete activity with automatic filtering and saving capability
- Parameterization of a process
- Something missing? Not a problem! Any kind of extension can implemented in a snap of a finger to create new types of data source/destination or any type of operator. ETL.NET has been designed for it.

It also permits to interact out of the box with:

- File system: read or write
- FTP, FTPS: read or write
- SFTP: read or write
- Email: send or get attachment from received emails
- Dropbox: read or write
- Zip: read
- Something missing? Not a problem either! Extending ETL.NET for this purpose is simple and easy as well!

It permits to trace **in detail** the entire activity of a process and use these traces in any possible way.

Once it is deployed, provides a very simple way for administrators to setup external connectivity.

And so much more; imagination is the limit.

## The ETL reference for Microsoft developers: SSIS

> SSIS is a fantastic tool.

SSIS is a great straight forward tool to process big chunks of data. It supports a lot of formats, protocols, and databases out of the box. It contains a quiet exhaustive set of mass processing operators. It is very fast. It exposes a very well done server for scheduling, parameterization, tracing, and activity reporting. Its development tool is very clear. Deployments are very simple and straight forward procedures...

> So... Why ETL.NET?

## It is multiplatform

> Can be run on nearly any kind of computer or even portable devices.

Developments are made on .NET. This permits ETL.NET to be used on any platform that supports .NET: Windows, Linux or macOS.

## It is open source

> Get advantages of the open source community.

Anybody can push a new request for it to be improved so that some developers may implement it for you. You can even add your own amendments for them to be candidates as new features at the next release. Of course, open source involves complete transparency about what ETL.NET does under the hood.

## It is made for developers

> Out of a business intelligence context, SSIS can be very cumbersome.

When it is about including ETL process in an application, many developers don't even consider using business intelligence tools such as SSIS. This is fairly understandable given the big commitment it involves:

- SSIS packages are in a different location than the application it self. It is not within the application sources, it won't be deployed in the same way, it won't be deployed at the same place.
- Executing the full solution on a development computer for debug or even simple development purpose is complex. Usually, in the world of development, once sources of an application are retrieved from source control, a hit on F5 should be sufficient to successfully execute and debug the application. With an SSIS process making part of the solution, executing and debugging the application needs an SSIS deployment with all its setups prior to run it. During debug of the application it self, it won't be possible to debug the ETL process as it is hosted in SSIS server.
- SSIS needs to be installed on development computers. This makes the solution more complex to apprehend where normal application simply need dotnet core + visual studio code or visual studio community to be executed with a simple F5. Furthermore, depending on the context, installing SSIS on a development computer is not always easy or possible (security policies in companies).

> With ETL.NET, a regular developer needs to download his sources, hit F5, and debug.

## It is simple and easy... really

> Simply add references to Nuget

Just add a reference from Nuget to the core package (ETL.NET) and to some of its extensions depending on the need (ETL.NET.TextFile, ETL.NET.EntityFrameworkCore...) and you can develop and execute on the go. It is a very simple development extremely close to Linq.

## It is designed to be very simple to extend

> Building SSIS extension is a horribly complex process compared to usual .NET tool kits and frameworks.

When it is about SSIS extensibility, you have to swim quiet deep in the abyss of hell: after dealing with the very complex SDK to extend it and the big amount of lines of codes to implement your extension, you will have to ensure it is well deployed in the GAC of your development computer, on the GAC of every SSIS server where it will be deployed. Finally, for it to be used in developments, it must also be installed correctly on your development computer so that Sql Server Data Tools for BI recognizes it and makes it available in the visual studio toolbox.

> An ETL.NET extension can be developed, debugged and used within in 15mn... literally.

ETL.NET extensibility is based on a simple development wether it is embedded within the application, in another shared assembly or a Nuget package. It is as simple as a couple of lines of code since a set of base classes permits extensions to be easily implemented for most of typical use cases.

## Visual designer?

> Hey, but... This is an ETL and... there is no visual designer?!!!

No, there is not...

:::note

Nevertheless, later on, a ReactJs component that works along an ASP.NET web api service might be done so that developers can permit end users to build their own simple but yet powerful ETL.NET processes within any React application. But still, it will be a high level wrapper of ETL.NET and nothing else than C# code will permit to get the full power of it.

Any team who is interested in having such a feature coming soon may consider having a look... [Here!](https://github.com/sponsors/paillave) :wink:

:::

## Want to know more?

Go to the [how it works section](/docs/quickstart/principle) if you are interested in the way ETL.NET works behind the scenes.
