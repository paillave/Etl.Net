# Etl.Net

[![Build status](https://ci.appveyor.com/api/projects/status/sqjh6f6cwadxfoou/branch/master?svg=true)](https://ci.appveyor.com/project/paillave/etl-net)
[![NuGet](https://img.shields.io/nuget/v/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net)
[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net)


Implementation of an Etl for .net working with a similar principle than SSIS, but only from .net code

## Developement status

:construction: **This library is still under development.**

The first alpha release is expected once it starts to be a decent candidate to replace SSIS for common use cases.

## Implemented and to be done ETL operations

| Name | Type | Done |
| ----- | ----- | ----- |
| Select | Tranformation | [x] |
| Where | Tranformation | [x] |
| Sort | Tranformation | [x] |
| Left Join | Tranformation | [x] |
| Lookup | Tranformation | [x] |
| Union | Tranformation | [x] |
| Skip | Tranformation | [x] |
| Top | Tranformation | [x] |
| Distinct | Tranformation | [x] |
| Pivot | Tranformation | [x] |
| Unpivot | Tranformation | [x] |
| Aggregate | Tranformation | [x] |
| Cross Apply | Tranformation | [x] |
| Ensure Sorted | Tranformation | [x] |
| Ensure Keyed | Tranformation | [x] |
| Script | Tranformation | [x] |
| Select keeping sorted | Transformation | [x] |
| Left join keeping sorted | Transformation | [x] |
| Lookup keeping sorted | Transformation | [x] |
| List folder files | Data source | [x] |
| Read csv file | Data source | [x] |
| :construction: Read excel file | Data source | [ ] |
| Write csv file | Data destination | [x] |
| :construction: Write excel file | Data destinaton | [ ] |
| Read from Entity framework core | Data source | [x] |
| Write to Entity framework core | Data destination | [x] |
| :construction: Read from Entity framework | Data source | [ ] |
| :construction: Write to Entity framework | Data destination | [ ] |
| :construction: Entity framework core upsert | Data destination | [ ] |
| :construction: Entity framework upsert | Data destination | [ ] |
| :construction: SQL Server bulk load | Data destination | [ ] |
| :construction: Read from sql server command | Data source | [ ] |
| :construction: Write to sql server command | Data destination | [ ] |
| :construction: List files from FTP | Data source | [ ] |
| :construction: List file from SFTP | Data source | [ ] |
| :construction: Read files from FTP | Data source | [ ] |
| :construction: Read file from SFTP | Data source | [ ] |
| :construction: Write files from FTP | Data destination | [ ] |
| :construction: Write file from SFTP | Data destination | [ ] |
| :construction: Read data from REST service | Data source | [ ] |
| :construction: Write data to REST service | Data destination | [ ] |

*Follow the status in the issue section*

*New requests are very welcome in the issue section*

## Implemented and to be done runtime features

| Name | Done |
| ----- | ----- |
| Trace issued data of each node | [x] |
| Trace any errors | [x] |
| Stop the entire process whenever an error is raised | [x] |
| Trace statistic result of each node at the end of the process | [x] |
| :construction: Trace time that is spent in each node at the end of the process | [ ] |
| :construction: Publish a Job as a REST web service in web api core | [ ] |
| Run any ETL operation on traces to permit to filter and save | [x] |
| Show graphic to represent the process as a directed graph | [x] |
| Show graphic to represent the process as a sankey graph | [x] |
| Show graphic to represent process execution statistics as a directed graph | [x] |
| Show graphic to represent process execution statistics as a sankey graph | [x] |
| :construction: Show realtime process execution statistics as a directed graph | [ ] |
| :construction: Show realtime process execution statistics as a sankey graph | [ ] |
| :construction: Web portal to host job definitions manage their executions | [ ] |
| :construction: Power shell command tool to execute a job | [ ] |
| :construction: Visual studio code addon to view the process as a directed graph whenever the job definition class file is saved | [ ] |
| :construction: Visual studio code addon to view the process as a sankey graph whenever the job definition class file is saved | [ ] |

*New requests are very welcome in the issue section*

## Quickstart

### Create configuration type

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleApp1.StreamTypes
{
    public class MyConfig
    {
        public string InputFolderPath { get; set; }
        public string InputFilesSearchPattern { get; set; }
        public string TypeFilePath { get; set; }
        public string DestinationFilePath { get; internal set; }
        public string CategoryDestinationFilePath { get; internal set; }
    }
}
```

### Create input and output stream structures
```csharp
using Paillave.Etl.Helpers;
using System;
using System.Globalization;

namespace ConsoleApp1.StreamTypes
{
    public class InputFileRow
    {
        public int Id { get; set; }
        public DateTime Col1 { get; set; }
        public decimal Col2 { get; set; }
        public int Col3 { get; set; }
        public string Col4 { get; set; }
        public int TypeId { get; set; }
        public string FileName { get; set; }
    }

    public class InputFileRowMapper : ColumnNameFlatFileDescriptor<InputFileRow>
    {
        public InputFileRowMapper()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-GB");
            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.NumberFormat.NumberDecimalSeparator = ",";
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
            ci.NumberFormat.PercentDecimalSeparator = ",";

            this.WithCultureInfo(ci);
            this.MapColumnToProperty("#", i => i.Id);
            this.MapColumnToProperty("DateTime", i => i.Col1);
            this.MapColumnToProperty("Value", i => i.Col2);
            this.MapColumnToProperty("Rank", i => i.Col3);
            this.MapColumnToProperty("Comment", i => i.Col4);
            this.MapColumnToProperty("TypeId", i => i.TypeId);
            this.IsFieldDelimited('\t');
        }
    }
}
```
```csharp
using Paillave.Etl.Helpers;
using System.Globalization;

namespace ConsoleApp1.StreamTypes
{
    public class TypeFileRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string FileName { get; set; }
    }

    public class TypeFileRowMapper : ColumnNameFlatFileDescriptor<TypeFileRow>
    {
        public TypeFileRowMapper()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-GB");
            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.NumberFormat.NumberDecimalSeparator = ",";
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
            ci.NumberFormat.PercentDecimalSeparator = ",";

            this.WithCultureInfo(ci);
            this.MapColumnToProperty("#", i => i.Id);
            this.MapColumnToProperty("Label", i => i.Name);
            this.MapColumnToProperty("Category", i => i.Category);
            this.IsFieldDelimited('\t');
        }
    }
}
```
```csharp
using Paillave.Etl.Helpers;

namespace ConsoleApp1.StreamTypes
{
    public class OutputFileRow
    {
        public string FileName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OutputFileRowMapper : ColumnNameFlatFileDescriptor<OutputFileRow>
    {
        public OutputFileRowMapper()
        {
            this.MapColumnToProperty("Id", i => i.Id);
            this.MapColumnToProperty("Name", i => i.Name);
            this.MapColumnToProperty("FileName", i => i.FileName);
            this.IsFieldDelimited(',');
        }
    }
}
```
```csharp
using Paillave.Etl.Helpers;

namespace ConsoleApp1.StreamTypes
{
    public class OutputCategoryRow
    {
        public string Category { get; set; }
        public int TotalAmount { get; set; }
        public int AmountOfEntries { get; set; }
    }
    public class OutputCategoryRowMapper : ColumnNameFlatFileDescriptor<OutputCategoryRow>
    {
        public OutputCategoryRowMapper()
        {
            this.MapColumnToProperty("Category", i => i.Category);
            this.MapColumnToProperty("Nb", i => i.AmountOfEntries);
            this.MapColumnToProperty("Tot", i => i.TotalAmount);
            this.IsFieldDelimited(',');
        }
    }
}
```
### Define the ETL job
```csharp
using ConsoleApp1.StreamTypes;
using System.IO;
using Paillave.Etl;
using Paillave.Etl.Core.Streams;
using System;

namespace ConsoleApp1.Jobs
{
    public class QuickStartJob : IStreamProcessDefinition<MyConfig>
    {
        public string Name => "import file";

        public void DefineProcess(IStream<MyConfig> rootStream)
        {
            var outputFileResourceS = rootStream.Select("open output file", i => new StreamWriter(i.DestinationFilePath));
            var outputCategoryResourceS = rootStream.Select("open output category file", i => new StreamWriter(i.CategoryDestinationFilePath));

            var parsedLineS = rootStream
                .CrossApplyFolderFiles("get folder files", i => i.InputFolderPath, i => i.InputFilesSearchPattern)
                .CrossApplyTextFile("parse input file", new InputFileRowMapper(), (i, p) => { p.FileName = i; return p; });

            var parsedTypeLineS = rootStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyTextFile("parse type input file", new TypeFileRowMapper());

            var joinedLineS = parsedLineS
                .Lookup("join types to file", parsedTypeLineS, i => i.TypeId, i => i.Id, (l, r) => new { l.Id, r.Name, l.FileName, r.Category });

            var categoryStatistics = joinedLineS
                .Pivot("create statistic for categories", i => i.Category, i => new { Count = AggregationOperators.Count(), Total = AggregationOperators.Sum(i.Id) })
                .Select("create output category data", i => new OutputCategoryRow { Category = i.Key, AmountOfEntries = i.Aggregation.Count, TotalAmount = i.Aggregation.Total })
                .ToTextFile("write category statistics to file", outputCategoryResourceS, new OutputCategoryRowMapper());

            joinedLineS.Select("create output data", i => new OutputFileRow { Id = i.Id, Name = i.Name, FileName = i.FileName })
                .ToTextFile("write to output file", outputFileResourceS, new OutputFileRowMapper())
                .ToAction("write to console", i => Console.WriteLine($"{i.FileName}:{i.Id}-{i.Name}"));
        }
    }
}
```
### Execute an ETL job definition
```csharp
using Paillave.Etl;
using System;
using System.IO;
using ConsoleApp1.StreamTypes;
using Paillave.Etl.Core;
using ConsoleApp1.Jobs;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new StreamProcessRunner<QuickStartJob, MyConfig>();
            runner.GetDefinitionStructure().OpenEstimatedExecutionPlanVisNetwork();
            StreamProcessDefinition<TraceEvent> traceStreamProcessDefinition = new StreamProcessDefinition<TraceEvent>(traceStream => traceStream.ToAction("logs to console", Console.WriteLine));
            var testFilesDirectory = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles";
            var task = runner.ExecuteAsync(new MyConfig
            {
                InputFolderPath = Path.Combine(testFilesDirectory, @"."),
                InputFilesSearchPattern = "testin.*.csv",
                TypeFilePath = Path.Combine(testFilesDirectory, @"ref - Copy.csv"),
                DestinationFilePath = Path.Combine(testFilesDirectory, @"outfile.csv"),
                CategoryDestinationFilePath = Path.Combine(testFilesDirectory, @"categoryStats.csv")
            }, traceStreamProcessDefinition);
            task.Result.OpenActualExecutionPlanD3Sankey();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
```

## Documentation

:construction: **Documentation will be done once all essential features and bugs are solved.**
