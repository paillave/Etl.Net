---
sidebar_position: 6
---

# Make a console application

Making a console application with ETL.NET is dramatically simple thanks to `Paillave.EtlNet.ExecutionToolkit` extensions.

This package contains 2 classes that implements the interface `ITraceReporter`

- `SimpleConsoleExecutionDisplay` to simply display the name of nodes who are completed and shows the number of rows it issued.
- `AdvancedConsoleExecutionDisplay` shows from the start all the operators on a text mode scrollable screen. Every second this screen is updated to show the amount of issued row so far for each operator, and to show what operator has completed.

These extensions are actually simple [traces process definitions](/docs/recipes/handleTraces) that can be found in the property `TraceProcessDefinition` of the interface `ITraceReporter`. This interface has also a method `Initialize` that must be called right before triggering the process.

Here is how to do to show on a simple console the list of tasks that just completed:

```cs
var processRunner = StreamProcessRunner.Create<string[]>(DefineProcess);
ITraceReporter traceReporter = new SimpleConsoleExecutionDisplay();
var executionOptions = new ExecutionOptions<string[]>
{
    TraceProcessDefinition = traceReporter.TraceProcessDefinition,
};
traceReporter.Initialize(structure);
var res = await processRunner.ExecuteAsync(args, executionOptions);
```

If a detailed visual report is necessary `AdvancedConsoleExecutionDisplay` shall be used. But this level of detail required the runtime to emit detailed traces:

```cs
var processRunner = StreamProcessRunner.Create<string[]>(DefineProcess);
ITraceReporter traceReporter = new AdvancedConsoleExecutionDisplay();
var executionOptions = new ExecutionOptions<string[]>
{
    TraceProcessDefinition = traceReporter.TraceProcessDefinition,
    UseDetailedTraces = true
};
traceReporter.Initialize(structure);
var res = await processRunner.ExecuteAsync(args, executionOptions);
```
