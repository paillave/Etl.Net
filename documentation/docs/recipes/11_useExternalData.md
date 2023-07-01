---
sidebar_position: 11
---

# Inject values in a process

## From trigger stream

The most natural and straight give a value to a process execution is to pass it as a parameter.

```cs
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace Demo
{
    class ProcessArguments
    {
        public string AStringValue { get; set; }
        public int AnIntValue { get; set; }
    }
    class Program3
    {
        static async Task Main3(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<ProcessArguments>(DefineProcess);
            var res = await processRunner.ExecuteAsync(new ProcessArguments
            {
                AStringValue = args[0],
                AnIntValue = 564
            });
        }
        private static void DefineProcess(ISingleStream<ProcessArguments> contextStream)
        {
            contextStream.Do("show process params on console", i => Console.WriteLine($"{i.AStringValue}: {i.AnIntValue}"));
        }
    }
}
```

## From dependency injection

It can happen that some values to be recovered later without necessarily dealing with the contextStream.
This is where dependency injection can be used:

```cs
using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace Demo
{
    class SomeExternalValue
    {
        public string AStringValue { get; set; }
        public int AnIntValue { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            var executionOptions = new ExecutionOptions<string>
            {
                Resolver = new SimpleDependencyResolver()
                    .Register(new SomeExternalValue
                    {
                        AStringValue = "injected string",
                        AnIntValue = 658
                    })
            };
            var res = await processRunner.ExecuteAsync("transmitted parameter", executionOptions);
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .ResolveAndSelect("get some values", o => o
                    .Resolve<SomeExternalValue>()
                    .Select((context, injected) => $"{context}-{injected.AStringValue}:{injected.AnIntValue}"));
        }
    }
}
```

Within a custom operator, accessing the dependency injection resolver is done this way: `base.ExecutionContext.DependencyResolver.Resolve<MyTypeToResolve>()`

In a value provider it is directly given as a parameter in the `PushValues` method.
