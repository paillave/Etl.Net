---
sidebar_position: 4
---

# Create files

[Handling files in ETL.NET](/docs/recipes/dealWithFiles) involves manipulating streams of payloads implementing the interface `IFileValue`. But how to actually create these payloads from nothing if they are not taken from any file source?

## One file

### One file with no specific format

The first way to create a file, is to create a file with a content with no specific or known format. In this situation, the principle is to create an instance of `FileValueWriter` using `FileValueWriter.Create` static method. `FileValueWriter` implements `IFileValue` and that wraps nearly every method of `StreamWriter`. *Note: all these methods return the current instance so that they can be called in a fluent way.*
Before this, the stream must be aggregated for it to issue lists of rows instead of single rows. To change a stream into a single stream event (`ISingleStream`) the operator `ToList` can be used.

```cs
var streamOfFile = streamOfRows
    .ToList("aggregate all rows")
    .Select("create file", rows => FileValueWriter
        .Create("fileExport.txt")
        .WriteLine("this content has no specific format")
        .Write(String.Join(", ", rows.Select(row => row.Name).ToList())));
```

### One file in CSV or Excel format

In many occasions, writing a file will consist in creating an excel file, or a csv file with fixed width or delimited columns

File creation extensions `Paillave.EtlNet.TextFile` or `Paillave.EtlNet.ExcelFile` can change a stream into a `IFileValue` instance out of the box.

```cs
var streamOfFile = streamOfRows
    .Select("create row to save", i => new { i.Index, i.Name })
    .ToTextFileValue("save into csv file", "fileExport.csv", FlatFileDefinition.Create(i => new
    {
        Index = i.ToNumberColumn<int>("Idx", "."),
        Name = i.ToColumn("Title")
    }).IsColumnSeparated(','));
```

## Many files

### Many files with no specific format

The way to create several files in a single process is to use the `GroupBy` operator.

The first way to use it is only possible if `FileValueWriter` is used. This way to use `GroupBy` is to simply give the grouping key/keys. This way, the operator will issue one event per group containing the list of values contained in the group.

```cs
var streamOfFile = streamOfRows
    .GroupBy("group rows", i => i.CategoryId) 
    // can also be written this way to permit several grouping keys:
    // .GroupBy("group rows", i => new { i.CategoryId }) 
    .Select("create file", rows => FileValueWriter
        .Create($"otherFileExport{rows.FirstValue.CategoryId}.txt")
        .WriteLine($"here is the list of indexes in the category {rows.FirstValue.CategoryId}")
        .Write(String.Join(", ", rows.Aggregation.Select(row => row.Name).ToList())));
```

The other way is to use the `GroupBy` operator by giving a subprocess along with the grouping key. The subprocess is the definition of a process from a stream that will issue every event belonging to the group. With the substream, the `GroupBy` operator will give the first element of the group as it is very likely to be useful to create the file name. To achieve the same than what is above, it is just necessary to reproduce the pattern described higher within the subprocess:

```cs
var streamOfFile = streamOfRows
    .GroupBy("process per group", i => i.CategoryId, (subStream, firstRow) => subStream
        .ToList("aggregate all rows")
        .Select("create file", rows => FileValueWriter
            .Create($"fileExport{firstRow?.CategoryId}.txt")
            .WriteLine($"here is the list of indexes in the category {firstRow?.CategoryId}")
            .Write(String.Join(", ", rows.Select(row => row.Name).ToList()))));
```

### Many files in CSV or Excel format

Defining a subprocess like shown in the example right above is the only way to go to produce several files by using `Paillave.EtlNet.TextFile` or `Paillave.EtlNet.ExcelFile` extensions:

```cs
var streamOfFile = streamOfRows
    .GroupBy("process per group", i => i.CategoryId, (subStream, firstRow) => subStream
        .Select("create row to save", i => new { i.Index, i.Name })
        .ToTextFileValue(
            "save into csv file", 
            $"fileExport{firstRow?.CategoryId}.csv", 
            FlatFileDefinition.Create(i => new
            {
                Index = i.ToNumberColumn<int>("Idx", "."),
                Name = i.ToColumn("Title")
            })));
```

:::important

Keep in mind that the given first row will be null when called to evaluate the execution plan by the runtime. Therefore, ensure that no null exception is raised when using it.
Of course, during the actual process, it will **never** be null.

:::
