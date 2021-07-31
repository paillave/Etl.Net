---
sidebar_position: 2
---

# Installation & Execution

![How it works](/img/azure-cloud-apps-scalability-rocket.svg)

No actual installation is necessary. Just add a NuGet reference to the core package into a .NET project. On top of it add any NuGet reference to the necessary extensions packages depending on what is needed.

The core package is `Paillave.Etl` that must be referenced into anything that deals with ETL.NET. Other packages `Paillave.Etl.*` are extensions.

These are the types of extensions:

- Operators for extra processes, input or output
- Extra behavior related to the runtime

Once references are added, the ETL process can be described in .NET and run anywhere in the application.

## ETL.NET packages

| NuGet Package | Type | Purpose |
| - | - | - |
| `Paillave.Etl` | Core | Contains the streaming mechanism, the runtime, and all essential operators that can be found in any regular ETL |
| `Paillave.Etl.Autofac` | Runtime extension | Some operators like Entity Framework Core extension or Sql Server extensions may need some resources to work like a connection. This work with a Dependency Injection pattern. ETL.NET has an internal and very primitive DI implementation, but it can be replaced by autofac |
| `Paillave.Etl.Dropbox` | Input/Output | Read or write file directly on dropbox |
| `Paillave.Etl.EntityFrameworkCore` | Input/Output | Bulkload, save, upsert, read, make lookups using Entity Framework Core context |
| `Paillave.Etl.ExcelFile` | Input/Output | Read or write Excel files tables |
| `Paillave.Etl.ExecutionToolkit` | Runtime extension | Visualization of processes and libraries to make a console application dedicated to ETL.NET executions |
| `Paillave.Etl.FileSystem` | Input/Output | Read or write files on the local file system |
| `Paillave.Etl.FromConfigurationConnectors` | Runtime extension | Provides connectors to the runtime using a configuration file |
| `Paillave.Etl.Ftp` | Input/Output | Read or write files on FTP or FTPS |
| `Paillave.Etl.Mail` | Input/Output | Send emails, or read emails attached files from SMTP folders |
| `Paillave.Etl.Sftp` | Input/Output | Read or write files on SFTP |
| `Paillave.Etl.SqlServer` | Input/Output | Upsert, read into Sql Server directly using drivers |
| `Paillave.Etl.TextFile` | Input/Output | Parse or create text files in csv (separated or fixed size columns) |
| `Paillave.Etl.XmlFile` | Input/Output | Parse XML files. Note: writing XML is not implemented at the moment |
| `Paillave.Etl.Zip` | Input/Output | Read files from zipped files. Note: creating a zip file is not implemented at the moment |

## Definition of the ETL process and its execution

The definition will be done in a method that receives the trigger stream as a parameter.

The execution will be done by calling the runtime.

Here is the simplest way to define and trigger an ETL the start using a string parameter from the command line:

```cs title="Program.cs"
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace MyEtlApplication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            processRunner.DebugNodeStream += (sender, e) => { /* PLACE A CONDITIONAL BREAKPOINT HERE FOR DEBUG ex: e.NodeName == "parse file" */ };
            var executionOptions = new ExecutionOptions<string> { TraceProcessDefinition = DefineTraceProcess };
            var res = await processRunner.ExecuteAsync(args[0], executionOptions);
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            // TODO: Define the ETL process here
        }
        private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
        {
            // TODO: Define the ETL process to handle traces here
        }
    }
}
```

## Learn more

The most essential features are explained in the [tutorial](/docs/tutorials/backbone).

Other common patterns and recipes are described [here](/).
