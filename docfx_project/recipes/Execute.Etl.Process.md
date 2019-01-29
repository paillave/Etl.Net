# Execute an ETL process

## Description

The execution of a job is an async task as it is made of threads that run in parallel to accomplish the process.

The class that is in charge of executing an ETL job is `Paillave.Etl.StreamProcessRunner`.

## Source code example

This will run the ETL process defined in the recipe [Define an Etl process](Define.Etl.Process.md#source-code-example)

# [Inline call execution](#tab/InlineCallExecution)

[!code-csharp[Main](../../src/Samples/Paillave.Etl.Recipes/DefineProcess/SimpleJobTests.cs?name=InlineMethodWay)]

# [Ask runner instance creation](#tab/InstanceRunnerCreation)

[!code-csharp[Main](../../src/Samples/Paillave.Etl.Recipes/DefineProcess/SimpleJobTests.cs?name=StaticMethodWay)]

# [Create runner instance](#tab/CreateRunnerInstance)

[!code-csharp[Main](../../src/Samples/Paillave.Etl.Recipes/DefineProcess/SimpleJobTests.cs?name=InstanceMethodWay)]

***
