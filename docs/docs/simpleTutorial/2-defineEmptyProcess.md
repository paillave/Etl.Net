---
sidebar_position: 3
---

# Backbone

![TrackCheck](/img/azure-app-service-platform-bot-construction.svg)

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

We will create a runner with `StreamProcessRunner` from the assembly `Etl.Net.ExecutionToolkit` by providing the ETL process.

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
```

## Trigger the runner

Once we have the runner, we can trigger it by providing the value of the single initial event.

```cs {13}
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
var res = await processRunner.ExecuteAsync(args[0]);
```

## Catch the success or failure of an execution

The execution returns an objects that gives information about the execution like the fact a failure occurred.

```cs
Console.Write(res.Failed ? "Failed" : "Succeeded");
```

## Full source at this stage

We now have the backbone of a console application that run an ETL process

```cs
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
