---
sidebar_position: 1
---

# Deal with files and protocols

ETL.NET has a complete set of features to deal with files. The core concept of the file management is the interface `IFileValue`:

```csharp title="IFileValue.cs"
using System;
using System.IO;

namespace Paillave.Etl.Core
{
    public interface IFileValue
    {
        string Name { get; }
        Stream GetContent();
        void Delete();
        Type MetadataType { get; }
        IFileValueMetadata Metadata { get; }
        string SourceType { get; }
    }
    public interface IFileValueMetadata
    {
        string Type { get; }
    }
}
```

Every file, wherever it comes from is handled as an event payload that has a type that implements `IFileValue`.

`IFileValue` is some sort of file wrapper. Its property `SourceType` gives the information about what extension issued it. By convention `Metadata` contains connectivity information (except any password) and where it comes from.

There is no need to implement this interface unless a ETL.NET file extension needs to be implemented.

:::note

This page explains how to get and drop files on different source types. Go [here](/docs/recipes/createFiles) to learn about actually creating a file.

:::

## Get or write files on the local file system

:::note

These direct extension methods are implement only for the file system. For other file sources, use dedicated file value providers or connectors.

:::

### Get files

Extensions from `Paillave.EtlNet.FileSystem` directly contains a method to get files from the file system: `CrossApplyFolderFiles`

From a stream of `string`, recursively get every zip file that is in each folder given by the source stream:

```cs
stream
    .CrossApplyFolderFiles("list all required files", "*.zip", true)
    .Do("print file name to console", i => Console.WriteLine(i.Name));
```

From a stream of `<SomeType>`, recursively get every zip file that is in a computed folder name from the source stream:

```cs
stream
    .CrossApplyFolderFiles("list all required files", i => $"Input/{i.Name}" ,"*.zip", true)
    .Do("print file name to console", i => Console.WriteLine(i.Name));
```

### Write files

The extension `WriteToFile` permits to write the content of the file into a file of the file system.

```cs
stream
    .Select("create file", someContent => FileValueWriter.Create("fileExport.txt").Write(someContent))
    .WriteToFile("write to folder", i => i.Name);
```

:::note

`FileValueWriter` is the api the permits to create the content of a file in a `IFileValue`.

:::

## Get files from different source types

Every input/output extension has a file value provider. Here we are going to get files from the local file system or from an FTP server.

Recursively gets all csv files from a fixed root folder in the current file system:

```cs
stream
    .CrossApply("Get files", new FileSystemFileValueProvider("SRC","Misc Source files", rootFolder, "*.csv"))
    .Do("print file name to console", i => Console.WriteLine(i.Name));
```

Gets all csv files from a fixed FTP folder:

```cs
var connectionParameters = new FtpAdapterConnectionParameters
{
    Server = "my.ftp.server",
    Login = "my.login",
    Password = "P@~~VV0RD",
};
var providerParameters = new FtpAdapterProviderParameters
{
    SubFolder = "filesToPick"
};
stream
    .CrossApply("Get files", new FtpFileValueProvider("SRC", "Misc Source files", "files from ftp", connectionParameters, providerParameters))
    .Do("print file name to console", i => Console.WriteLine(i.Name));
```

## Get or Write file on any source type: Connectors

ETL.NET provides a system for this purpose that permits to access any source type in a complete abstract way: Connectors.

A connector is an abstract input or output connection point. Every connector input or output is defined in any way (FTP, SFTP, EMail, File System, dropbox...) before triggering the process.

A connector is identified by its code. And that's this code that is used in the process to be referred.
Whatever the way they are created, connector definitions are embedded into an instance of `FileValueConnectors` that must be set to the property `Connectors` of the `ExecutionOptions` that will be given to the `ExecuteAsync` of the process runner.

Here we will create 2 input connectors: "PTF" and "POS", and one output connector: "OUT".

An input connector is created by registering a **file value provider** taken from the necessary extension.
An output connector is created by registering a **file value processor** taken from the necessary extension.

```cs
var executionOptions = new ExecutionOptions<string>
{
    Connectors = new FileValueConnectors()
        .Register(new FileSystemFileValueProvider("PTF", "Portfolios", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Portfolios.csv"))
        .Register(new FileSystemFileValueProvider("POS", "Positions", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Positions.csv"))
        .Register(new FileSystemFileValueProcessor("OUT", "Result", Path.Combine(Environment.CurrentDirectory, "OutputFiles"))),
};
var res = await processRunner.ExecuteAsync(args, executionOptions);
```

Using connectors is done the following way:

```cs {2,14,16}
contextStream
    .FromConnector("Get portfolio files", "PTF")
    .CrossApplyTextFile("Parse portfolio file", FlatFileDefinition.Create(i => new
    {
        SicavCode = i.ToColumn("SicavCode"),
        SicavName = i.ToColumn("SicavName"),
        SicavType = i.ToColumn("SicavType"),
        PortfolioCode = i.ToColumn("PortfolioCode"),
        PortfolioName = i.ToColumn("PortfolioName")
    }).IsColumnSeparated(','))
    .Do("print portfolio names to console", i => Console.WriteLine(i.PortfolioName));

contextStream
    .FromConnector("Get position files", "POS")
    .Do("print position file names to console", i => Console.WriteLine(i.Name))
    .ToConnector("Save copy of position file", "OUT");
```

## Create connectors from config file

All this is possible with the runtime extension `Paillave.EtlNet.FromConfigurationConnectors`.

Creating connectors from a config file is less straight forward for the developer, but the level of flexibility that is then delegated to the infrastructure administrator or to a user is million times worth the effort.

The concept is to provide a json in a string variable that contains all the setups that will be parsed to be given to the ExecutionOptions.

First, create a json configuration parser. This parser must be aware of adapters that will be available for the ETL process. Here, we want our process to be able to access FTP, SFTP, the file system and EMails:

```cs
var configurationFileValueConnectorParser = new ConfigurationFileValueConnectorParser(
    new FileSystemProviderProcessorAdapter(),
    new FtpProviderProcessorAdapter(),
    new SftpProviderProcessorAdapter(),
    new MailProviderProcessorAdapter());
```

Once the parser is created, the configuration can be parsed to build the connectors that will be set to the `ExecutionOptions`:

```cs
var executionOptions = new ExecutionOptions<string[]>
{
    Connectors = configurationFileValueConnectorParser.GetConnectors(connectorDefinitions),
};
var res = await processRunner.ExecuteAsync(args, executionOptions);
```

To have the same connectors than the ones that are hard coded above, the json definition must be the following:

```json
{
    "inputFilesForSomePartner": {
        "Type": "FileSystem",
        "Connection": {
            "RootFolder": "<path to the root folder of files used for some partner>"
        },
        "Providers": {
            "PTF": {
                "FileNamePattern": "*.Portfolios.csv",
                "SubFolder": "InputFiles"
            },
            "POS": {
                "FileNamePattern": "*.Positions.csv",
                "SubFolder": "InputFiles"
            }
        },
        "Processors": {
            "OUT": { "SubFolder": "OutputFiles" }
        }
    }
}
```

:::note

Properties of `connection`, `processor` and `providers` are the same than the ones of the connection parameter, the processor parameter and the provider parameter object that is given to a file value provider or processor.

:::

Here is a more complex definition that uses other communication types but that is supposed to be the same to the eyes of the process that will use it:

```json
{
    "inputFilesForSomePartnerFTP": {
        "Type": "Ftp",
        "Connection": {
            "Server": "ftp.server",
            "Login": "login",
            "Password": "password",
            "RootFolder": "myBusinessHere"
        },
        "Providers": {
            "POS": {
                "FileNamePattern": "*.Positions.csv",
                "SubFolder": "exports"
            }
        }
    },
    "someEmailsConnectors": {
        "Type": "Mail",
        "Connection": {
            "Server": "sdfsdfg",
            "Login": "login",
            "Password": "pass"
        },
        "Processors": {
            "OUT":{
                "Body": "Hello, <br>Here is your file.",
                "Subject": "New file",
                "From": "noreply@dummy.com",
                "FromDisplayName": "ETL.NET",
                "To": "an.email@a.company.com"
            }
        },
        "Providers": {
            "PTF":{
                "AttachmentNamePattern": "*.Portfolios.csv",
                "SetToReadIfBatchDeletion": true,
                "OnlyNotRead": true,
                "FromContains": "@the.sender.com"
            }
        }
    }
}
```

Such a file may be a bit hard to apprehend, even more by considering it is supposed to be filled by a user or an administrator. But we can add validity and intellisense (if a proper json editor is used) to improve all this.
It is possible to create a **json schema file**, that can be redistributed to administrators, and/or set to the json editor for it to give intellisense and checks.

To do this, the parser, once created the proper way (see above), can build a json schema that can validate the json of connector definitions it is supposed to parse. Most of time it will be written in a file:

```cs
var jsonSchema = configurationFileValueConnectorParser.GetConnectorsSchemaJson();
File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "connectorsConfigSchema.json"), jsonSchema);
```

In the json file to define connectors, just add the `$schema` directive this way:

```json {2}
{
    "$schema": "./connectorsConfigSchema.json",
    "inputFiles": {
        "Type": "FileSystem",
        "Connection": { "RootFolder": "<path to the root folder of files used for some partner>" },
        "Providers": {
            "PTF": { "FileNamePattern": "*.Portfolios.csv" },
            "POS": { "FileNamePattern": "*.Positions.csv" }
        },
        "Processors": {
            "OUT": {}
        }
    }
}
```

## Send emails to a different recipients depending on the file

As shown above, it is possible to send and receive files by emails. The example above sends email to a fixed email, but in many occasions the recipient depends on the file itself. The solution for this is to use the destination metadata.

As seen higher a `IFileValue` has a property `Metadata` of the type `IFileValueMetadata`. If this metadata inherits `IFileValueWithDestinationMetadata` instead, `Destinations` can be used by the mail processor (what sends emails).

```cs title="IFileValueWithDestinationMetadata.cs"
using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public interface IFileValueWithDestinationMetadata : IFileValueMetadata
    {
        Dictionary<string, IEnumerable<Destination>> Destinations { get; set; }
    }
    public class Destination
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string StreetAddress { get; set; }
        public string ZipCode { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string Culture {get;set;}
    }
}
```

As an example, here is a way to create a file containing some arbitrary content and to send it by email if the connector is meant for it:

```cs
stream
    .Select("create file", someContent => FileValueWriter
        .Create("fileExport.txt", new Dictionary<string, IEnumerable<Destination>>
            { 
                ["Sale"] = new []{ new Destination { DisplayName = "The display name of the sale", Email = "sale@a.domain.com" } },
                ["Client"] = new []{ new Destination { DisplayName = "The display name of the client", Email = "client@another.domain.com" } }
            })
        .Write(someContent))
    .ToConnector("Send to sale", "SALES")
    .ToConnector("Send to client", "CLIENTS");
```

The configuration file can be this one for test environment:

```json title="connectors.test.json"
{
    "$schema": "./connectorsConfigSchema.json",
    "inputFiles": {
        "Type": "FileSystem",
        "Connection": { "RootFolder": "<path to the root folder of output test files>" },
        "Processors": {
            "SALES": { "SubFolder": "SalesFiles" },
            "CLIENTS": { "SubFolder": "ClientsFiles" }
        }
    }
}
```

And it can be this one for production environment:

```json title="connectors.prod.json"
{
    "$schema": "./connectorsConfigSchema.json",
    "someEmailsConnectors": {
        "Type": "Mail",
        "Connection": {
            "Server": "sdfsdfg",
            "Login": "login",
            "Password": "pass"
        },
        "Processors": {
            "SALES":{
                "Body": "Dear {Destination.DisplayName}, <br>Here is your file.",
                "Subject": "New file",
                "From": "noreply@dummy.com",
                "FromDisplayName": "ETL.NET",
                "ToFromMetadata": true,
                "To": "Sale"
            },
            "CLIENTS":{
                "Body": "Dear {Destination.DisplayName}, <br>Here is your invoice.",
                "Subject": "Invoice",
                "From": "invoice.service@dummy.com",
                "FromDisplayName": "ETL.NET",
                "ToFromMetadata": true,
                "To": "Client"
            }
        },
    }
}
```
