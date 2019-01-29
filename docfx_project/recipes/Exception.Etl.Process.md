# Exceptions in ETL process

## Types of calls

Even if there are several ways to run an ETL process, there are two categories of calls:

- The call that will raise an exception if something in the job fails. The calls that are described the recipe [Execute an Etl process](Execute.Etl.Process.md#source-code-example) correspond to this type
- The call that will stop if the job fails. The execution status and the  cause of the problem is into the task result object.

## Try/Catch

The exception must be caught on the `Wait()` method of the task. In this case, the expected exception is the regular exception type we get when the underlying process of a task raise an exception: `System.AggregateException`.
This exception contains an InnerException that has the type `Paillave.Etl.Recipes.DefineProcess.SimpleJobThrowException`. `SimpleJobThrowException` gives access to the trace that is at the source of the end of this process. This trace permits to know in which node happened the exception, and possibly witch row when through at this moment. The exception that is the root cause of the problem is in the `InnerException` of `SimpleJobThrowException`.

This will run the ETL process defined in the recipe [Define an Etl process](Define.Etl.Process.md#source-code-example)

# [Inline call execution](#tab/InlineCallExecution)

[!code-csharp[Main](../../src/Samples/Paillave.Etl.Recipes/DefineProcess/SimpleJobThrowException.cs?name=InlineMethodWay)]

# [Ask runner instance creation](#tab/InstanceRunnerCreation)

[!code-csharp[Main](../../src/Samples/Paillave.Etl.Recipes/DefineProcess/SimpleJobThrowException.cs?name=StaticMethodWay)]

# [Create runner instance](#tab/CreateRunnerInstance)

[!code-csharp[Main](../../src/Samples/Paillave.Etl.Recipes/DefineProcess/SimpleJobThrowException.cs?name=InstanceMethodWay)]

***

## Getting the execution status