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
      "TargetNodeName": "exclude duplicates",
      "SourceNodeName": "parse file"
    },
    {
      "InputName": "SourceStream",
      "TargetNodeName": "save in DB",
      "SourceNodeName": "exclude duplicates"
    },
    {
      "InputName": "Stream",
      "TargetNodeName": "display ids on console",
      "SourceNodeName": "save in DB"
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
      "TypeName": "Cross apply FileSystemValuesProvider\u00602",
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
      "TypeName": "Cross apply FlatFileValuesProvider\u00602",
      "PerformanceImpact": 3,
      "MemoryFootPrint": 1,
      "IsRootNode": false
    },
    {
      "NodeName": "exclude duplicates",
      "TypeName": "Distinct",
      "PerformanceImpact": 1,
      "MemoryFootPrint": 3,
      "IsRootNode": false
    },
    {
      "NodeName": "save in DB",
      "TypeName": "SqlServerSave",
      "PerformanceImpact": 3,
      "MemoryFootPrint": 1,
      "IsRootNode": false
    },
    {
      "NodeName": "display ids on console",
      "TypeName": "Do",
      "PerformanceImpact": 1,
      "MemoryFootPrint": 1,
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
    "SourceNodeName": "exclude duplicates"
  },
  {
    "Counter": 45,
    "SourceNodeName": "save in DB"
  },
  {
    "Counter": 45,
    "SourceNodeName": "display ids on console"
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

## Trace everything that goes through nodes

Before triggering the execution of the ETL process listen the event `DebugNodeStream` of the process runner.

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
processRunner.DebugNodeStream += (sender, e) => { };
```

The eventargs contains a chunk of values (max 1000 values per chunks) in the property `TraceContents`, and the name of the node that emitted it in the property `NodeName`.

Placing a breakpoint in the event handler permits to see all the values that are processes within the ETL. Applying the following condition on the breakpoint will show only chunks of data issued by the node that parses csv files:

```cs
e.NodeName == "parse file"
```

![trackValues](/img/tutorial/trackValues.png)
