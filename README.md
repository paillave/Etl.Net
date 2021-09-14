# Etl.Net [![Join the chat at https://gitter.im/Etl-Net/Lobby](https://badges.gitter.im/Etl-Net/Lobby.svg)](https://gitter.im/Etl-Net/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

![ETL.NET](streams.jpg)

## Presentation

Implementation of a multi platform reactive ETL for .NET5 working with a similar principle than SSIS, but that is used in the same way than Linq.
The reactive approach for the implementation of this engine ensures parallelized multi streams, high performances and low memory foot print even with million rows to process.

- ETL.NET is fully written in .NET for a multi platform usage and for a straight forward integration in any application. Extend it takes 5mn... literally.
- ETL.NET works with a similar principle than SSIS, with ETL processes to be written in .NET like Linq queries.
- A simple and straight forward ELT.NET runtime for .NET executes ETL processes with no installation required.


| Package | nuget version | nuget downloads |
|-|-|-|
| Etl.Net | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net) |
| Etl.Net.Autofac | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.Autofac.svg)](https://www.nuget.org/packages/Etl.Net.Autofac) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.Autofac.svg)](https://www.nuget.org/packages/Etl.Net.Autofac) |
| Etl.Net.Dropbox | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.Dropbox.svg)](https://www.nuget.org/packages/Etl.Net.Dropbox) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.Dropbox.svg)](https://www.nuget.org/packages/Etl.Net.Dropbox) |
| Etl.Net.EntityFrameworkCore | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Etl.Net.EntityFrameworkCore) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Etl.Net.EntityFrameworkCore) |
| Etl.Net.ExcelFile | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.ExcelFile.svg)](https://www.nuget.org/packages/Etl.Net.ExcelFile) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.ExcelFile.svg)](https://www.nuget.org/packages/Etl.Net.ExcelFile) |
| Etl.Net.ExecutionToolkit | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.ExecutionToolkit.svg)](https://www.nuget.org/packages/Etl.Net.ExecutionToolkit) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.ExecutionToolkit.svg)](https://www.nuget.org/packages/Etl.Net.ExecutionToolkit) |
| Etl.Net.FileSystem | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.FileSystem.svg)](https://www.nuget.org/packages/Etl.Net.FileSystem) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.FileSystem.svg)](https://www.nuget.org/packages/Etl.Net.FileSystem) |
| Paillave.Etl.FromConfigurationConnectors | [![NuGet](https://img.shields.io/nuget/v/Paillave.Etl.FromConfigurationConnectors.svg)](https://www.nuget.org/packages/Paillave.Etl.FromConfigurationConnectors) |[![NuGet](https://img.shields.io/nuget/dt/Paillave.Etl.FromConfigurationConnectors.svg)](https://www.nuget.org/packages/Paillave.Etl.FromConfigurationConnectors) |
| Etl.Net.Ftp | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.Ftp.svg)](https://www.nuget.org/packages/Etl.Net.Ftp) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.Ftp.svg)](https://www.nuget.org/packages/Etl.Net.Ftp) |
| Etl.Net.Mail | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.Mail.svg)](https://www.nuget.org/packages/Etl.Net.Mail) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.Mail.svg)](https://www.nuget.org/packages/Etl.Net.Mail) |
| Etl.Net.Sftp | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.Sftp.svg)](https://www.nuget.org/packages/Etl.Net.Sftp) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.Sftp.svg)](https://www.nuget.org/packages/Etl.Net.Sftp) |
| Etl.Net.SqlServer | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.SqlServer.svg)](https://www.nuget.org/packages/Etl.Net.SqlServer) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.SqlServer.svg)](https://www.nuget.org/packages/Etl.Net.SqlServer) |
| Etl.Net.TextFile | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.TextFile.svg)](https://www.nuget.org/packages/Etl.Net.TextFile) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.TextFile.svg)](https://www.nuget.org/packages/Etl.Net.TextFile) |
| Etl.Net.XmlFile | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.XmlFile.svg)](https://www.nuget.org/packages/Etl.Net.XmlFile) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.XmlFile.svg)](https://www.nuget.org/packages/Etl.Net.XmlFile) |
| Etl.Net.Zip | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.Zip.svg)](https://www.nuget.org/packages/Etl.Net.Zip) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.Zip.svg)](https://www.nuget.org/packages/Etl.Net.Zip) |
| Paillave.EntityFrameworkCoreExtension | [![NuGet](https://img.shields.io/nuget/v/Paillave.EntityFrameworkCoreExtension.svg)](https://www.nuget.org/packages/Paillave.EntityFrameworkCoreExtension) |[![NuGet](https://img.shields.io/nuget/dt/Paillave.EntityFrameworkCoreExtension.svg)](https://www.nuget.org/packages/Paillave.EntityFrameworkCoreExtension) |

## Examples

### Unzip it, read it, save it, report it

Read all zip files from a folder, unzip csv files that are inside, parse them, exclude duplicates, upsert them into database, and report new or pre existing id corresponding to the email.

```csharp
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
