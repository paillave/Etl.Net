# Etl.Net

![ETL.NET](streams.jpg)
[Go to full documentation](https://paillave.github.io/Etl.Net/)

## Presentation

Implementation of a multi platform reactive ETL for .NET5 working with a similar principle than SSIS, but that is used in the same way than Linq.
The reactive approach for the implementation of this engine ensures parallelized multi streams, high performances and low memory foot print even with million rows to process.

- ETL.NET is fully written in .NET for a multi platform usage and for a straight forward integration in any application. Extend it takes 5mn... literally.
- ETL.NET works with a similar principle than SSIS, with ETL processes to be written in .NET like Linq queries.
- A simple and straight forward ELT.NET runtime for .NET executes ETL processes with no installation required.

| Package | nuget version | nuget downloads |
|-|-|-|
| Paillave.EtlNet.Core | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.Core.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Core) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.Core.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Core) |
| Paillave.EtlNet.Autofac | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.Autofac.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Autofac) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.Autofac.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Autofac) |
| Paillave.EtlNet.Dropbox | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.Dropbox.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Dropbox) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.Dropbox.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Dropbox) |
| Paillave.EtlNet.EntityFrameworkCore | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Paillave.EtlNet.EntityFrameworkCore) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Paillave.EtlNet.EntityFrameworkCore) |
| Paillave.EtlNet.ExcelFile | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.ExcelFile.svg)](https://www.nuget.org/packages/Paillave.EtlNet.ExcelFile) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.ExcelFile.svg)](https://www.nuget.org/packages/Paillave.EtlNet.ExcelFile) |
| Paillave.EtlNet.ExecutionToolkit | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.ExecutionToolkit.svg)](https://www.nuget.org/packages/Paillave.EtlNet.ExecutionToolkit) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.ExecutionToolkit.svg)](https://www.nuget.org/packages/Paillave.EtlNet.ExecutionToolkit) |
| Paillave.EtlNet.FileSystem | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.FileSystem.svg)](https://www.nuget.org/packages/Paillave.EtlNet.FileSystem) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.FileSystem.svg)](https://www.nuget.org/packages/Paillave.EtlNet.FileSystem) |
| Paillave.EtlNet.FromConfigurationConnectors | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.FromConfigurationConnectors.svg)](https://www.nuget.org/packages/Paillave.EtlNet.FromConfigurationConnectors) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.FromConfigurationConnectors.svg)](https://www.nuget.org/packages/Paillave.EtlNet.FromConfigurationConnectors) |
| Paillave.EtlNet.Ftp | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.Ftp.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Ftp) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.Ftp.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Ftp) |
| Paillave.EtlNet.Mail | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.Mail.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Mail) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.Mail.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Mail) |
| Paillave.EtlNet.Sftp | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.Sftp.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Sftp) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.Sftp.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Sftp) |
| Paillave.EtlNet.SqlServer | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.SqlServer.svg)](https://www.nuget.org/packages/Paillave.EtlNet.SqlServer) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.SqlServer.svg)](https://www.nuget.org/packages/Paillave.EtlNet.SqlServer) |
| Paillave.EtlNet.TextFile | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.TextFile.svg)](https://www.nuget.org/packages/Paillave.EtlNet.TextFile) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.TextFile.svg)](https://www.nuget.org/packages/Paillave.EtlNet.TextFile) |
| Paillave.EtlNet.XmlFile | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.XmlFile.svg)](https://www.nuget.org/packages/Paillave.EtlNet.XmlFile) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.XmlFile.svg)](https://www.nuget.org/packages/Paillave.EtlNet.XmlFile) |
| Paillave.EtlNet.Zip | [![NuGet](https://img.shields.io/nuget/v/Paillave.EtlNet.Zip.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Zip) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EtlNet.Zip.svg)](https://www.nuget.org/packages/Paillave.EtlNet.Zip) |
| Paillave.EntityFrameworkCoreExtension | [![NuGet](https://img.shields.io/nuget/v/Paillave.EntityFrameworkCoreExtension.svg)](https://www.nuget.org/packages/Paillave.EntityFrameworkCoreExtension) | [![NuGet](https://img.shields.io/nuget/dt/Paillave.EntityFrameworkCoreExtension.svg)](https://www.nuget.org/packages/Paillave.EntityFrameworkCoreExtension) |

## Examples

### Unzip it, read it, save it, report it

Read all zip files from a folder, unzip csv files that are inside, parse them, exclude duplicates, upsert them into database, and report new or pre existing id corresponding to the email.

```
dotnet new console -o SimpleTutorial
cd SimpleTutorial
dotnet add package Paillave.EtlNet.Core
dotnet add package Paillave.EtlNet.FileSystem
dotnet add package Paillave.EtlNet.Zip
dotnet add package Paillave.EtlNet.TextFile
dotnet add package Paillave.EtlNet.SqlServer
```

```csharp
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl.TextFile;
using Paillave.Etl.SqlServer;
using System.Data.SqlClient;
using System.Linq;

namespace SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            processRunner.DebugNodeStream += (sender, e) => { };
            using (var cnx = new SqlConnection(args[1]))
            {
                cnx.Open();
                var executionOptions = new ExecutionOptions<string>
                {
                    Resolver = new SimpleDependencyResolver().Register(cnx),
                };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
                if (res.Failed)
                    Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
            }
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApplyFolderFiles("list all required files", "*.zip", true)
                .CrossApplyZipFiles("extract files from zip", "*.csv")
                .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
                {
                    Email = i.ToColumn("email"),
                    FirstName = i.ToColumn("first name"),
                    LastName = i.ToColumn("last name"),
                    DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
                    Reputation = i.ToNumberColumn<int?>("reputation", ".")
                }).IsColumnSeparated(','))
                .Distinct("exclude duplicates based on the Email", i => i.Email)
                .SqlServerSave("upsert using Email as key and ignore the Id", o => o
                    .ToTable("dbo.Person")
                    .SeekOn(p => p.Email)
                    .DoNotSave(p => p.Id))
                .Select("define row to report", i => new { i.Email, i.Id })
                .ToTextFileValue("write summary to file", "report.csv", FlatFileDefinition.Create(i => new
                {
                    Email = i.ToColumn("Email"),
                    Id = i.ToNumberColumn<int>("new or existing Id", ".")
                }).IsColumnSeparated(','))
                .WriteToFile("save log file", i => i.Name);
        }
        private class Person
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public int? Reputation { get; set; }
        }
    }
}
```

### Run it, debug it, track it, log it

Execute an ETL process, debug it by tracking debug events using the IDE debugger, catch execution events and log it into database.

```cs
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl.TextFile;
using Paillave.Etl.SqlServer;
using System.Data.SqlClient;

namespace SimpleTutorial
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
      processRunner.DebugNodeStream += (sender, e) => { /* PLACE A CONDITIONAL BREAKPOINT HERE FOR DEBUG */ };
      using (var cnx = new SqlConnection(args[1]))
      {
        cnx.Open();
        var executionOptions = new ExecutionOptions<string>
        {
          Resolver = new SimpleDependencyResolver().Register(cnx),
          TraceProcessDefinition = DefineTraceProcess,
          // UseDetailedTraces = true // activate only if per row traces are meant to be caught
        };
        var res = await processRunner.ExecuteAsync(args[0], executionOptions);
        Console.Write(res.Failed ? "Failed" : "Succeeded");
        if (res.Failed)
          Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
      }
    }
    private static void DefineProcess(ISingleStream<string> contextStream)
    {
      // TODO: define your ELT process here
    }
    private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
    {
      traceStream
        .Where("keep only summary of node and errors", i => i.Content is CounterSummaryStreamTraceContent || i.Content is UnhandledExceptionStreamTraceContent)
        .Select("create log entry", i => new ExecutionLog
          {
            DateTime = i.DateTime,
            ExecutionId = i.ExecutionId,
            EventType = i.Content switch
            {
              CounterSummaryStreamTraceContent => "EndOfNode",
              UnhandledExceptionStreamTraceContent => "Error",
              _ => "Unknown"
            },
            Message = i.Content switch
            {
              CounterSummaryStreamTraceContent counterSummary => $"{i.NodeName}: {counterSummary.Counter}",
              UnhandledExceptionStreamTraceContent unhandledException => $"{i.NodeName}({i.NodeTypeName}): [{unhandledException.Level.ToString()}] {unhandledException.Message}",
              _ => "Unknown"
            }
          })
        .SqlServerSave("save traces", o => o.ToTable("dbo.ExecutionTrace"));
    }
    private class ExecutionLog
    {
      public DateTime DateTime { get; set; }
      public Guid ExecutionId { get; set; }
      public string EventType { get; set; }
      public string Message { get; set; }
    }
  }
}
```

### Normalize it

Dispatch rows from a flat file into several tables to normalize data thanks to the correlation mechanism.

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
  var rowStream = contextStream
    .CrossApplyFolderFiles("list all required files", "*.csv", true)
    .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new
    {
      Author = i.ToColumn("author"),
      Email = i.ToColumn("email"),
      TimeSpan = i.ToDateColumn("timestamp", "yyyyMMddHHmmss"),
      Category = i.ToColumn("category"),
      Link = i.ToColumn("link"),
      Post = i.ToColumn("post"),
      Title = i.ToColumn("title"),
    }).IsColumnSeparated(','))
    .SetForCorrelation("set correlation for row");

  var authorStream = rowStream
    .Distinct("remove author duplicates based on emails", i => i.Email)
    .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
    .EfCoreSaveCorrelated("save or update authors", o => o
      .SeekOn(i => i.Email)
      .AlternativelySeekOn(i => i.Name));

  var categoryStream = rowStream
    .Distinct("remove category duplicates", i => i.Category)
    .Select("create category instance", i => new Category { Code = i.Category, Name = i.Category })
    .EfCoreSaveCorrelated("insert categories if doesn't exist, get it otherwise", o => o
      .SeekOn(i => i.Code)
      .DoNotUpdateIfExists());

  var postStream = rowStream
    .CorrelateToSingle("get related category", categoryStream, (l, r) => new { Row = l, Category = r })
    .CorrelateToSingle("get related author", authorStream, (l, r) => new { l.Row, l.Category, Author = r })
    .Select("create post instance", i => string.IsNullOrWhiteSpace(i.Row.Post)
      ? new LinkPost
      {
        AuthorId = i.Author.Id,
        CategoryId = i.Category.Id,
        DateTime = i.Row.TimeSpan,
        Title = i.Row.Title,
        Url = new Uri(i.Row.Link)
      } as Post
      : new TextPost
      {
        AuthorId = i.Author.Id,
        CategoryId = i.Category.Id,
        DateTime = i.Row.TimeSpan,
        Title = i.Row.Title,
        Text = i.Row.Post
      })
    .EfCoreSaveCorrelated("save or update posts", o => o
      .SeekOn(i => new { i.AuthorId, i.DateTime }));
}
```
