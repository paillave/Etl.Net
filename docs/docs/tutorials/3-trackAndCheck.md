---
sidebar_position: 6
---

# Execute, Track & Check

![TrackCheck](/img/gauges-fast-and-scalable.svg)

ETL.NET can show what will happen, what happens, and what happened under the hood in details.

## Get the estimated execution plan

We will get the estimated execution plan with `GetDefinitionStructure` method of the runner:

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
var structure = processRunner.GetDefinitionStructure();
```

The method will return the list of nodes that are composing the ETL process, and all the links between them.

For each node, an information regarding to possible performance and memory impact is given: `1` = low and `3` = heavy

```json
{
  "StreamToNodeLinks": [
    {
      "InputName": "Stream",
      "TargetNodeName": "extract files from zip",
      "SourceNodeName": "list all required files"
    },
    {
      "InputName": "Stream",
      "TargetNodeName": "parse file",
      "SourceNodeName": "extract files from zip"
    },
    {
      "InputName": "InputStream",
      "TargetNodeName": "exclude duplicates based on the Email",
      "SourceNodeName": "parse file"
    },
    {
      "InputName": "SourceStream",
      "TargetNodeName": "upsert using Email as key and ignore the Id",
      "SourceNodeName": "exclude duplicates based on the Email"
    },
    {
      "InputName": "Stream",
      "TargetNodeName": "define row to report",
      "SourceNodeName": "upsert using Email as key and ignore the Id"
    },
    {
      "InputName": "MainStream",
      "TargetNodeName": "write summary to file",
      "SourceNodeName": "define row to report"
    },
    {
      "InputName": "Stream",
      "TargetNodeName": "save log file",
      "SourceNodeName": "write summary to file"
    }
  ],
  "Nodes": [
    {
      "NodeName": "-ProcessRunner-",
      "TypeName": "-ProcessRunner-",
      "PerformanceImpact": 1,
      "MemoryFootPrint": 1,
      "IsRootNode": false
    },
    {
      "NodeName": "list all required files",
      "TypeName": "Cross apply FileSystemValuesProvider`2",
      "PerformanceImpact": 3,
      "MemoryFootPrint": 2,
      "IsRootNode": false
    },
    {
      "NodeName": "extract files from zip",
      "TypeName": "Cross apply UnzipFileProcessorValuesProvider",
      "PerformanceImpact": 2,
      "MemoryFootPrint": 2,
      "IsRootNode": false
    },
    {
      "NodeName": "parse file",
      "TypeName": "Cross apply FlatFileValuesProvider`2",
      "PerformanceImpact": 3,
      "MemoryFootPrint": 1,
      "IsRootNode": false
    },
    {
      "NodeName": "exclude duplicates based on the Email",
      "TypeName": "Distinct",
      "PerformanceImpact": 1,
      "MemoryFootPrint": 3,
      "IsRootNode": false
    },
    {
      "NodeName": "upsert using Email as key and ignore the Id",
      "TypeName": "SqlServerSave",
      "PerformanceImpact": 3,
      "MemoryFootPrint": 1,
      "IsRootNode": false
    },
    {
      "NodeName": "define row to report",
      "TypeName": "Select",
      "PerformanceImpact": 1,
      "MemoryFootPrint": 1,
      "IsRootNode": false
    },
    {
      "NodeName": "write summary to file",
      "TypeName": "ToFileValue",
      "PerformanceImpact": 2,
      "MemoryFootPrint": 1,
      "IsRootNode": false
    },
    {
      "NodeName": "save log file",
      "TypeName": "WriteToFile",
      "PerformanceImpact": 3,
      "MemoryFootPrint": 3,
      "IsRootNode": false
    }
  ],
  "PerformanceImpact": 2,
  "MemoryFootPrint": 2
}
```

It is possible to visualize it with a sankey diagram (by default, will work only if a debugger is attached to the current process):

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
var structure = processRunner.GetDefinitionStructure();
structure.OpenEstimatedExecutionPlan();
```

The diagram will be shown in the web browser:

![estimatedExPlan](/img/tutorial/estimatedExPlan.png)

The chart looks pretty simple and straight forward, but it can be very useful when lot of nodes are interacting.

## Check the result

Once a ETL process has completed, we can get more details than its success or failure. Getting the amount of rows issued by each node can be very useful.

```cs
var res = await processRunner.ExecuteAsync(args[0], executionOptions);
Console.Write(res.Failed ? "Failed" : "Succeeded");
var counters = res.StreamStatisticCounters;
var estimatedExecutionPlan = res.JobDefinitionStructure;
```

`estimatedExecutionPlan` will be exactly the same than `processRunner.GetDefinitionStructure()`.

`counters` will contain the following:

```json
[
  {
    "Counter": 1,
    "SourceNodeName": "-ProcessRunner-"
  },
  {
    "Counter": 2,
    "SourceNodeName": "list all required files"
  },
  {
    "Counter": 9,
    "SourceNodeName": "extract files from zip"
  },
  {
    "Counter": 45,
    "SourceNodeName": "parse file"
  },
  {
    "Counter": 45,
    "SourceNodeName": "exclude duplicates based on the Email"
  },
  {
    "Counter": 45,
    "SourceNodeName": "upsert using Email as key and ignore the Id"
  },
  {
    "Counter": 45,
    "SourceNodeName": "define row to report"
  },
  {
    "Counter": 1,
    "SourceNodeName": "write summary to file"
  },
  {
    "Counter": 1,
    "SourceNodeName": "save log file"
  }
]
```

It is also possible to show the Sankey diagram with ribbons that have the width depending on the count of rows that were issued by the originating node:

```cs
var res = await processRunner.ExecuteAsync(args[0], executionOptions);
Console.Write(res.Failed ? "Failed" : "Succeeded");
res.OpenActualExecutionPlan();
```

The diagram will be shown in the web browser:

![actualExecutionPlan](/img/tutorial/actualExecutionPlan.png)

## Get the error if it occurs

When an error occurred during the execution process (`ExecutionStatus.Failed` return by method `ExecuteAsync`) the property `ExecutionStatus.ErrorTraceEvent` informs where and what happened:

```cs
if (res.Failed)
    Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
```

## Trace everything that goes through nodes

Before triggering the execution of the ETL process listen the event `DebugNodeStream` of the process runner.

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
processRunner.DebugNodeStream += (sender, e) => { };
```

The event args `e` contains a chunk of values (max 1000 values per chunks) in the property `TraceContents`, and the name of the node that emitted it in the property `NodeName`.

Placing a breakpoint in the event handler permits to see all the values that are processes within the ETL. Applying the following condition on the breakpoint will show only chunks of data issued by the node that parses csv files:

```cs
e.NodeName == "parse file"
```

![trackValues](/img/tutorial/trackValues.png)

## Catch main events to save them in a log file

We will track each end of nodes, raised errors and save it in a log file in the current directory.

For this, we must make an ETL process dedicated to handle traces:

```cs
private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
{
    traceStream
        .Where("keep only summary of node and errors", i => i.Content is CounterSummaryStreamTraceContent || i.Content is UnhandledExceptionStreamTraceContent)
        .Select("create log entry", i => new
        {
            DateTime = DateTime.Now,
            Type = i.Content switch
            {
                CounterSummaryStreamTraceContent => "EndOfNode",
                UnhandledExceptionStreamTraceContent => "Error",
                _ => "Unknown"
            },
            Message = i.Content switch
            {
                CounterSummaryStreamTraceContent counterSummary => $"{i.NodeName}: {counterSummary.Counter}",
                UnhandledExceptionStreamTraceContent unhandledException => $"{i.NodeName}({unhandledException.Type}): [{unhandledException.Level.ToString()}] {unhandledException.Message}",
                _ => "Unknown"
            }
        })
        .ToTextFileValue("write log file", "log.csv", FlatFileDefinition.Create(i => new
        {
            DateTime = i.ToDateColumn("datetime", "yyyy-MM-dd hh:mm:ss:ffff"),
            Type = i.ToColumn("event type"),
            Message = i.ToColumn("details"),
        }).IsColumnSeparated(','))
        .WriteToFile("save log file", i => i.Name);
}
```

Then we must provide this specific ETL process when executing the process by setting it in `ExecutionOptions`.

```cs
var executionOptions = new ExecutionOptions<string>
{
    Resolver = new SimpleDependencyResolver().Register(cnx),
    TraceProcessDefinition = DefineTraceProcess
};
```

The output file will contain the following:

```csv title="log.csv"
datetime,event type,details
2021-07-15 12:26:39:7094,EndOfNode,-ProcessRunner-: 1
2021-07-15 12:26:39:7425,EndOfNode,list all required files: 2
2021-07-15 12:26:39:8204,EndOfNode,extract files from zip: 9
2021-07-15 12:26:40:2900,EndOfNode,parse file: 45
2021-07-15 12:26:40:2900,EndOfNode,exclude duplicates: 45
2021-07-15 12:26:40:2901,EndOfNode,save in DB: 45
2021-07-15 12:26:40:2901,EndOfNode,display ids on console: 45
```

:::note

If, for some reasons, actual values that are being issued by some nodes are needed to be accessed, `UseDetailedTraces` must be flagged in options that are transmitted to `ExecuteAsync`. To catch the content, `RowProcessStreamTraceContent` must be included in the trace ETL process.

`UseDetailedTraces`  is set to `false` by default for performance purposes.

```cs
var executionOptions = new ExecutionOptions<string>
{
    Resolver = new SimpleDependencyResolver().Register(cnx),
    TraceProcessDefinition = DefineTraceProcess,
    UseDetailedTraces = true
};
```

:::

## Full source

This piece of program is a typical process to make a reliable upsert of the content of every zipped csv file in a folder, with process summary and error logging... Ready to deploy in production! :champagne: :beer: :cocktail: :clinking_glasses: :beers:

```cs title="Program.cs"
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
            processRunner.DebugNodeStream += (sender, e) => { };
            using (var cnx = new SqlConnection(args[1]))
            {
                cnx.Open();
                var executionOptions = new ExecutionOptions<string>
                {
                    Resolver = new SimpleDependencyResolver().Register(cnx),
                    TraceProcessDefinition = DefineTraceProcess
                };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
                if (res.Failed)
                    Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
            }
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
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApplyFolderFiles("list all required files", "*.zip", true)
                .CrossApplyZipFiles("extract files from zip", "*.csv")
                .CrossApplyTextFile("parse file", 
                    FlatFileDefinition.Create(i => new Person
                    {
                        Email = i.ToColumn("email"),
                        FirstName = i.ToColumn("first name"),
                        LastName = i.ToColumn("last name"),
                        DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
                        Reputation = i.ToNumberColumn<int?>("reputation", ".")
                    }).IsColumnSeparated(','))
                .Distinct("exclude duplicates based on the Email", i => i.Email)
                .SqlServerSave("upsert using Email as key and ignore the Id", 
                    "dbo.Person", 
                    p => p.Email, 
                    p => p.Id)
                .Select("define row to report", i => new { i.Email, i.Id })
                .ToTextFileValue("write summary to file", 
                    "report.csv", 
                    FlatFileDefinition.Create(i => new
                    {
                        Email = i.ToColumn("Email"),
                        Id = i.ToNumberColumn<int>("new or existing Id", ".")
                    }).IsColumnSeparated(','))
                .WriteToFile("save log file", i => i.Name);
        }
        private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
        {
            traceStream
                .Where("keep only summary of node and errors", i => i.Content is CounterSummaryStreamTraceContent || i.Content is UnhandledExceptionStreamTraceContent)
                .Select("create log entry", i => new
                {
                    DateTime = DateTime.Now,
                    Type = i.Content switch
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
                .ToTextFileValue("write log file", "log.csv", FlatFileDefinition.Create(i => new
                {
                    DateTime = i.ToDateColumn("datetime", "yyyy-MM-dd hh:mm:ss:ffff"),
                    Type = i.ToColumn("event type"),
                    Message = i.ToColumn("details"),
                }).IsColumnSeparated(','))
                .WriteToFile("save log file", i => i.Name);
        }
    }
}
```
