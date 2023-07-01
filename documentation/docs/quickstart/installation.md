---
sidebar_position: 2
---

# Installation & Execution

![How it works](/img/azure-cloud-apps-scalability-rocket.svg)

No actual installation is necessary. Just add a NuGet reference to the core package into a .NET project. On top of it, add any NuGet reference to the necessary extensions packages depending on what is needed.

The core package is `Paillave.EtlNet.Core` that must be referenced into anything that deals with ETL.NET. Other packages `Paillave.EtlNet.*` are extensions.

These are the types of extensions:

- Operators for extra processes, input or output
- Extra behavior related to the runtime

Once references are added, the ETL process can be described in .NET and run anywhere in the application.

## ETL.NET packages

| NuGet Package | Type | Purpose |
| - | - | - |
| `Paillave.EtlNet.Core` | Core | Contains the streaming mechanism, the runtime, and every essential operator that can be expected in any regular ETL |
| `Paillave.EtlNet.Autofac` | Runtime extension | Some operators like Entity Framework Core extension or Sql Server extension may need some resources to work like a connection. This works using Dependency Injection. ETL.NET offers a very primitive DI implementation out of the box that can be replaced by this autofac wrapper implementation |
| `Paillave.EtlNet.Dropbox` | Input/Output | Read or write file directly on dropbox |
| `Paillave.EtlNet.EntityFrameworkCore` | Input/Output | Bulkload, save, upsert, read, make lookups using Entity Framework Core context |
| `Paillave.EtlNet.ExcelFile` | Input/Output | Read or write Excel files tables |
| `Paillave.EtlNet.ExecutionToolkit` | Runtime extension | Visualization of processes and libraries to make a console application dedicated to ETL.NET executions |
| `Paillave.EtlNet.FileSystem` | Input/Output | Read or write files on the local file system |
| `Paillave.EtlNet.FromConfigurationConnectors` | Runtime extension | Provides connectors to the runtime using a configuration file |
| `Paillave.EtlNet.Ftp` | Input/Output | Read or write files on FTP or FTPS |
| `Paillave.EtlNet.Mail` | Input/Output | Send emails, or read emails attached files from SMTP folders |
| `Paillave.EtlNet.Sftp` | Input/Output | Read or write files on SFTP |
| `Paillave.EtlNet.SqlServer` | Input/Output | Upsert, read into Sql Server directly using drivers |
| `Paillave.EtlNet.TextFile` | Input/Output | Parse or create text files in csv (separated or fixed size columns) |
| `Paillave.EtlNet.Bloomberg` | Input/Output | Parse bloomberg response files. Note: creating a request file is not implemented at the moment |
| `Paillave.EtlNet.XmlFile` | Input/Output | Parse XML files. Note: writing XML is not implemented at the moment |
| `Paillave.EtlNet.Zip` | Input/Output | Read files from zipped files. Note: creating a zip file is not implemented at the moment |
| `Paillave.EtlNet.Pdf` | Input/Output | Read pdf files, by telling the structure of paragraphs. |
| `Paillave.EtlNet.Scheduler` | Input/Output | Submit ticks depending based on cron setups. |

## Definition of the ETL process and its execution

The definition will be done in a method that receives the trigger stream as a parameter.

The execution will be done by calling the runtime.

Traces can be handled by a process defined using the stream of events given by the runtime.

Here is the simplest way to define and trigger an ETL that starts using a string parameter from the command line:

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

Learn about the [most essential features](/docs/tutorials/backbone), and get it touch with [common patterns and recipes](/docs/recipes/dealWithFiles).
