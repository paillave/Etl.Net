---
sidebar_position: 3
---

# Part 2: Define empty process

## Create an empty process and call it

First, create an empty ETL process definition.

This process definition is a method that must receive as a parameter a stream of a single element that is the transmitted value when the process is run. In our situation this value is a `string` that represents the path where to find zip files.

```cs {3,12-15}
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            // TODO: plug all underlying processes that will follow the start of the process
        }
    }
}
```

## Create the runner

We will create a runner with `StreamProcessRunner` from the assembly `Etl.Net.ExecutionToolkit` by providing the ETL process.

```cs {4,12}
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core.Streams;
using Paillave.Etl;

namespace Paillave.Etl.SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            // TODO: plug all underlying processes that will follow the start of the process
        }
    }
}
```

## Trigger the runner

Once we have the runner, we can trigger it by providing the value of the single initial event.

```cs {13}
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core.Streams;
using Paillave.Etl;

namespace Paillave.Etl.SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            var res = await processRunner.ExecuteAsync(args[0]);
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            // TODO: plug all underlying processes that will follow the start of the process
        }
    }
}
```

## Catch the success or failure of an execution

The execution returns an objects that gives information about the execution like the fact a failure occurred.

```cs {14}
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core.Streams;
using Paillave.Etl;

namespace Paillave.Etl.SimpleTutorial
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
            // TODO: plug all underlying processes that will follow the start of the process
        }
    }
}
```
