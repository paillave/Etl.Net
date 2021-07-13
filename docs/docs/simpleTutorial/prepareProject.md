---
sidebar_position: 2
---

# Part 1: Prepare project

## Create the project

```sh
dotnet new console -o SimpleTutorial
cd SimpleTutorial
```

## Add reference to ETL.NET

- `Etl.Net` The core of ETL.NET with all its common operators
- `Etl.Net.FileSystem` Extensions to interact with the local file system: read a file, list files from folder, write file on the file system
- `Etl.Net.Zip` Extension to Unzip and Zip files
- `Etl.Net.TextFile` Extensions to serialize or deserialize text files (delimited or fixed width)
- `Etl.Net.SqlServer` Extensions to interact with Sql Server **without** Entity Framework
- `Etl.Net.ExecutionToolkit` ETL.NET runtime and related

```sh
dotnet add package Etl.Net
dotnet add package Etl.Net.FileSystem
dotnet add package Etl.Net.Zip
dotnet add package Etl.Net.TextFile
dotnet add package Etl.Net.SqlServer
dotnet add package Etl.Net.ExecutionToolkit
```
