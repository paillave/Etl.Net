# Etl.Net

[![Build status](https://ci.appveyor.com/api/projects/status/sqjh6f6cwadxfoou/branch/master?svg=true)](https://ci.appveyor.com/project/paillave/etl-net)
[![NuGet](https://img.shields.io/nuget/v/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net)
[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net)


Implementation of an Etl for .net working with a similar principle than SSIS, but only from .net code

## Developement status

**:construction: This library is still under development.**

The first alpha release is expected once it starts to be a decent candidate to replace SSIS for common use cases.

## Implemented and to be done ETL operations

| Name | Type | Done |
| ----- | ----- | ----- |
| Select | Tranformation | :heavy_check_mark: |
| Where | Tranformation | :heavy_check_mark: |
| Sort | Tranformation | :heavy_check_mark: |
| Left Join | Tranformation | :heavy_check_mark: |
| Lookup | Tranformation | :heavy_check_mark: |
| Union | Tranformation | :heavy_check_mark: |
| Skip | Tranformation | :heavy_check_mark: |
| Top | Tranformation | :heavy_check_mark: |
| Distinct | Tranformation | :heavy_check_mark: |
| Pivot | Tranformation | :heavy_check_mark: |
| Unpivot | Tranformation | :heavy_check_mark: |
| Aggregate | Tranformation | :heavy_check_mark: |
| Cross Apply | Tranformation | :heavy_check_mark: |
| Ensure Sorted | Tranformation | :heavy_check_mark: |
| Ensure Keyed | Tranformation | :heavy_check_mark: |
| Script | Tranformation | :heavy_check_mark: |
| Select keeping sorted | Transformation | :heavy_check_mark: |
| Left join keeping sorted | Transformation | :heavy_check_mark: |
| Lookup keeping sorted | Transformation | :heavy_check_mark: |
| List folder files | Data source | :heavy_check_mark: |
| Read csv file | Data source | :heavy_check_mark: |
| Read excel file | Data source | :construction: |
| Write csv file | Data destination | :heavy_check_mark: |
| Write excel file | Data destinaton | :construction: |
| Read from Entity framework core | Data source | :heavy_check_mark: |
| Write to Entity framework core | Data destination | :heavy_check_mark: |
| Read from Entity framework | Data source | :construction: |
| Write to Entity framework | Data destination | :construction: |
| Entity framework core upsert | Data destination | :construction: |
| Entity framework upsert | Data destination | :construction: |
| SQL Server bulk load | Data destination | :construction: |
| Read from sql server command | Data source | :construction: |
| Write to sql server command | Data destination | :construction: |
| List files from FTP | Data source | :construction: |
| List file from SFTP | Data source | :construction: |
| Read files from FTP | Data source | :construction: |
| Read file from SFTP | Data source | :construction: |
| Write files from FTP | Data destination | :construction: |
| Write file from SFTP | Data destination | :construction: |
| Read data from REST service | Data source | :construction: |
| Write data to REST service | Data destination | :construction: |

*Follow the status in the issue section*

*New requests are very welcome in the issue section*

## Implemented and to be done runtime features

| Name | Done |
| ----- | ----- |
| Trace issued data of each node | :heavy_check_mark: |
| Trace any errors | :heavy_check_mark: |
| Stop the entire process whenever an error is raised | :heavy_check_mark: |
| Trace statistic result of each node at the end of the process | :heavy_check_mark: |
| Trace time that is spent in each node at the end of the process | :construction: |
| Publish a Job as a REST web service in web api core | :construction: |
| Run any ETL operation on traces to permit to filter and save | :heavy_check_mark: |
| Show graphic to represent the process as a directed graph | :heavy_check_mark: |
| Show graphic to represent the process as a sankey graph | :heavy_check_mark: |
| Show graphic to represent process execution statistics as a directed graph | :heavy_check_mark: |
| Show graphic to represent process execution statistics as a sankey graph | :heavy_check_mark: |
| Show realtime process execution statistics as a directed graph | :construction: |
| Show realtime process execution statistics as a sankey graph | :construction: |
| Web portal to host job definitions manage their executions | :construction: |
| Power shell command tool to execute a job | :construction: |
| Visual studio code addon to view the process as a directed graph whenever the job definition class file is saved | :construction: |
| Visual studio code addon to view the process as a sankey graph whenever the job definition class file is saved | :construction: |
| Raise a warning on the risky node when a performance issue or a bad practice is detected given statistics | :construction: |

*New requests are very welcome in the issue section*

## Simple Quickstart :suspect:

```csharp
using Paillave.Etl;
using System.IO;
using Paillave.Etl.Helpers;
using Paillave.Etl.Core.Streams;
using System;

namespace ConsoleApp1
{
    public class SimpleConfig
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
    }

    public class SimpleInputFileRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryCode { get; set; }
    }

    public class SimpleInputFileRowMapper : ColumnNameFlatFileDescriptor<SimpleInputFileRow>
    {
        public SimpleInputFileRowMapper()
        {
            this.MapColumnToProperty("#", i => i.Id);
            this.MapColumnToProperty("Label", i => i.Name);
            this.MapColumnToProperty("Category", i => i.CategoryCode);
            this.IsFieldDelimited('\t');
        }
    }

    public class CategoryStatisticFileRow
    {
        public string CategoryCode { get; set; }
        public int Count { get; set; }
    }

    public class CategoryStatisticFileRowMapper : ColumnNameFlatFileDescriptor<CategoryStatisticFileRow>
    {
        public CategoryStatisticFileRowMapper()
        {
            this.MapColumnToProperty("Category", i => i.CategoryCode);
            this.MapColumnToProperty("Nb", i => i.Count);
        }
    }

    public class SimpleQuickstartJob : IStreamProcessDefinition<SimpleConfig>
    {
        public string Name => "Simple quickstart";

        public void DefineProcess(IStream<SimpleConfig> rootStream)
        {
            var outputFileS = rootStream.Select("open output file", i => new StreamWriter(i.OutputFilePath));
            rootStream
                .CrossApplyTextFile("read input file", new SimpleInputFileRowMapper(), i => i.InputFilePath)
                .ToAction("Write input file to console", i => Console.WriteLine($"{i.Id}-{i.Name}-{i.CategoryCode}"))
                .Pivot("group and count", i => i.CategoryCode, i => new { Count = AggregationOperators.Count() })
                .Select("create output row", i => new CategoryStatisticFileRow { CategoryCode = i.Key, Count = i.Aggregation.Count })
                .Sort("sort output values", i => new { i.CategoryCode })
                .ToTextFile("write to text file", outputFileS, new CategoryStatisticFileRowMapper());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new StreamProcessRunner<SimpleQuickstartJob, SimpleConfig>().ExecuteAsync(new SimpleConfig
            {
                InputFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\simpleinputfile.csv",
                OutputFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\simpleoutputfile.csv"
            }, null).Wait();
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
```

## Complex Quickstart :feelsgood:

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

**:construction: Documentation will be done once all essential features and bugs are solved.**
