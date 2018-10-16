# Etl.Net [![Join the chat at https://gitter.im/Etl-Net/Lobby](https://badges.gitter.im/Etl-Net/Lobby.svg)](https://gitter.im/Etl-Net/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

|develop|master|
|-|-|
| ![GitHub last commit](https://img.shields.io/github/last-commit/paillave/etl.net/develop.svg) | ![GitHub last commit](https://img.shields.io/github/last-commit/paillave/etl.net/master.svg) |
| [![Build status](https://ci.appveyor.com/api/projects/status/n0ok6xrd7d5s176b/branch/develop?svg=true)](https://ci.appveyor.com/project/paillave/etl-net-vyqub) | [![Build status](https://ci.appveyor.com/api/projects/status/sqjh6f6cwadxfoou/branch/master?svg=true)](https://ci.appveyor.com/project/paillave/etl-net) |

Implementation of a multi platform reactive ETL for .net standard 2.0 working with a similar principle than SSIS, but that is used in the same way than Linq.
The reactive approach for the implementation of this engine ensures parallelized multi streams, high performances and low memory foot print even with million rows to process.

| Package | nuget version | nuget downloads |
|-|-|-|
| Etl.Net | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.svg)](https://www.nuget.org/packages/Etl.Net) |
| Etl.Net.EntityFrameworkCore | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Etl.Net.EntityFrameworkCore) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Etl.Net.EntityFrameworkCore) |
| Etl.Net.TextFile | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.TextFile.svg)](https://www.nuget.org/packages/Etl.Net.TextFile) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.TextFile.svg)](https://www.nuget.org/packages/Etl.Net.TextFile) |
| Etl.Net.Ftp | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.Ftp.svg)](https://www.nuget.org/packages/Etl.Net.Ftp) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.Ftp.svg)](https://www.nuget.org/packages/Etl.Net.Ftp) |
| Etl.Net.ExcelFile | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.ExcelFile.svg)](https://www.nuget.org/packages/Etl.Net.ExcelFile) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.ExcelFile.svg)](https://www.nuget.org/packages/Etl.Net.ExcelFile) |
| Etl.Net.SqlServer | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.SqlServer.svg)](https://www.nuget.org/packages/Etl.Net.SqlServer) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.SqlServer.svg)](https://www.nuget.org/packages/Etl.Net.SqlServer) |
| Etl.Net.ExecutionPlan | [![NuGet](https://img.shields.io/nuget/v/Etl.Net.ExecutionPlan.svg)](https://www.nuget.org/packages/Etl.Net.ExecutionPlan) |[![NuGet](https://img.shields.io/nuget/dt/Etl.Net.ExecutionPlan.svg)](https://www.nuget.org/packages/Etl.Net.ExecutionPlan) |

## Developement status :construction:

> [!WARNING]
> This library is still under development, don't use it on production environment yet as its api structure is subject for changes.
> The documentation is being written with a large set of unit tests

The first alpha release is expected once it starts to be a decent candidate to replace SSIS for common use cases.

## ETL features

| Name | Type | Done | Issue |
| ----- | ----- | ----- | ----- |
| Select | Transformation | :heavy_check_mark: | #26 |
| Where | Transformation | :heavy_check_mark: |  |
| Sort | Transformation | :heavy_check_mark: | #30 |
| Left Join | Transformation | :heavy_check_mark: |  |
| Lookup | Transformation | :heavy_check_mark: | #23 |
| Union | Transformation | :heavy_check_mark: | #34 |
| Union All | Transformation | :heavy_check_mark: | #94 |
| Skip | Transformation | :heavy_check_mark: |  |
| Top | Transformation | :heavy_check_mark: |  |
| Distinct | Transformation | :heavy_check_mark: | #15 |
| Pivot | Transformation | :heavy_check_mark: | #41 |
| Unpivot | Transformation | :heavy_check_mark: | #42 |
| Aggregate | Transformation | :heavy_check_mark: | #19 |
| Cross Apply | Transformation | :heavy_check_mark: | #25 |
| Ensure Sorted | Transformation | :heavy_check_mark: |  |
| Ensure Keyed | Transformation | :heavy_check_mark: |  |
| Script | Transformation | :heavy_check_mark: |  |
| Run subprocess | Transformation | :heavy_check_mark: | #70 |
| List folder files | Data source | :heavy_check_mark: |  |
| Read csv file | Data source | :heavy_check_mark: | #31 |
| Keep section | Transformation | :heavy_check_mark: | #58 |
| Ignore section | Transformation | :heavy_check_mark: | #59 |
| Read very large xml file | Data source | :construction: | #65 |
| Read very large json file | Data source | :construction: | #79 |
| Read excel file | Data source | :heavy_check_mark: | #18 |
| Write csv file | Data destination | :heavy_check_mark: | #28 |
| Write excel file | Data destinaton | :heavy_check_mark: | #17 |
| Read from Entity framework core | Data source | :heavy_check_mark: | #13 |
| Write to Entity framework core | Data destination | :heavy_check_mark: | #14 |
| Read from Entity framework | Data source | :construction: | #50 |
| Write to Entity framework | Data destination | :construction: | #51 |
| Read from MongoDb | Data source | :construction: |  |
| Write to MongoDb | Data destination | :construction: |  |
| MongoDb upsert | Data destination | :construction: |  |
| Entity framework core upsert | Data destination | :heavy_check_mark: | #49 |
| Entity framework upsert | Data destination | :construction: | #53 |
| SQL Server bulk load | Data destination | :heavy_check_mark: | #20 |
| Read from sql server command | Data source | :heavy_check_mark: | #55 |
| Write to sql server command | Data destination | :heavy_check_mark: | #54 |
| List files from FTP | Data source | :heavy_check_mark: | #11 |
| List file from SFTP | Data source | :construction: | #10 |
| Read file from FTP | Data source | :heavy_check_mark: | #11 |
| Read file from SFTP | Data source | :construction: | #10 |
| Write file to FTP | Data destination | :heavy_check_mark: | #6 |
| Write file to SFTP | Data destination | :construction: | #5 |
| Read data from REST service | Data source | :construction: | #9 |
| Write data to REST service | Data destination | :construction: | #8 |

> [!NOTE]
> Follow the status in the issue section
> 
> New requests are very welcome in the issue section

## Runtime features

| Name | Done | Issue |
| ----- | ----- | ----- |
| Trace issued data by each node | :heavy_check_mark: |  |
| Trace any error | :heavy_check_mark: |  |
| Stop the entire process whenever an error is raised | :heavy_check_mark: |  |
| Trace statistic result of each node at the end of the process | :heavy_check_mark: |  |
| Trace time that is spent in each node at the end of the process | :construction: |  |
| Publish a Job as a REST web service in web api core | :construction: |  |
| Execute a job using reference to native .net core configuration | :construction: |  |
| Execute any ETL process on traces to filter and save them | :heavy_check_mark: |  |
| Show graphic to represent the process | :heavy_check_mark: |  |
| Show graphic to represent process execution statistics | :heavy_check_mark: | #27 |
| Show realtime process execution statistics | :construction: | #36 |
| Web portal to host job definitions and manage their executions | :construction: | #80 |
| Power shell command tool to execute a job | :construction: | #83 |
| Visual studio code addon to view the process whenever the job definition class file is saved | :construction: | #82 |
| Raise a warning on the risky node when a performance issue or a bad practice is detected given statistics | :construction: | #81 |
| Interprets a T-SQL-like language script to build a job definition on the fly and run it | :construction: | #44 |

> [!NOTE]
> New requests are very welcome in the [issue section](https://github.com/paillave/Etl.Net/issues)

## Simple Quickstart :suspect:

```csharp
using Paillave.Etl;
using System;
using System.IO;
using Paillave.Etl.Core;
using Paillave.Etl.TextFile.Core;
using Paillave.Etl.Core.Streams;

namespace SimpleQuickstart
{
    public class SimpleConfig
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
    }

    public class SimpleQuickstartJob
    {
        public static void DefineProcess(IStream<SimpleConfig> rootStream)
        {
            var outputFileS = rootStream.Select("open output file", i => new StreamWriter(i.OutputFilePath));
            rootStream
                .CrossApplyTextFile("read input file",
                    FileDefinition.Create(
                        i =>
                        {
                            Id = i.ToColumn<int>("#"),
                            Name = i.ToColumn<string>("Label"),
                            CategoryCode = i.ToColumn<string>("Category")
                        }).IsColumnSeparated('\t'),
                    i => i.InputFilePath)
                .ToAction("Write input file to console", i => Console.WriteLine($"{i.Id}-{i.Name}-{i.CategoryCode}"))
                .Pivot("group and count", i => i.CategoryCode, i => new { Count = AggregationOperators.Count() })
                .Select("create output row", i => new CategoryStatisticFileRow { CategoryCode = i.Key, Count = i.Aggregation.Count })
                .Sort("sort output values", i => new { i.CategoryCode })
                .ToTextFile("write to text file", outputFileS, FileDefinition.Create(i =>
                {
                    CategoryCode = i.ToColumn<string>("Category"),
                    Count = i.ToColumn<int>("Total")
                }));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var testFilesDirectory = @"XXXXXXXXXXXX\Etl.Net\src\TestFiles";

            StreamProcessRunner.Create<SimpleConfig>(SimpleQuickstartJob.DefineProcess).ExecuteAsync(new SimpleConfig
            {
                InputFilePath = Path.Combine(testFilesDirectory, "simpleinputfile.csv"),
                OutputFilePath = Path.Combine(testFilesDirectory, "simpleoutputfile.csv")
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

namespace ComplexQuickstart.StreamTypes
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
using System;
using System.Globalization;
using Paillave.Etl.TextFile.Core;

namespace ComplexQuickstart.StreamTypes
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

    public class InputFileRowMapper : FileDefinition<InputFileRow>
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
            this.IsColumnSeparated('\t');
        }
    }
}
```

```csharp
using System.Globalization;
using Paillave.Etl.TextFile.Core;

namespace ComplexQuickstart.StreamTypes
{
    public class TypeFileRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string FileName { get; set; }
    }

    public class TypeFileRowMapper : FileDefinition<TypeFileRow>
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
            this.IsColumnSeparated('\t');
        }
    }
}
```

```csharp

using Paillave.Etl.TextFile.Core;

namespace ComplexQuickstart.StreamTypes
{
    public class OutputFileRow
    {
        public string FileName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OutputFileRowMapper : FileDefinition<OutputFileRow>
    {
        public OutputFileRowMapper()
        {
            this.MapColumnToProperty("Id", i => i.Id);
            this.MapColumnToProperty("Name", i => i.Name);
            this.MapColumnToProperty("FileName", i => i.FileName);
            this.IsColumnSeparated(',');
        }
    }
}
```

```csharp

using Paillave.Etl.TextFile.Core;

namespace ComplexQuickstart.StreamTypes
{
    public class OutputCategoryRow
    {
        public string Category { get; set; }
        public int TotalAmount { get; set; }
        public int AmountOfEntries { get; set; }
    }
    public class OutputCategoryRowMapper : FileDefinition<OutputCategoryRow>
    {
        public OutputCategoryRowMapper()
        {
            this.MapColumnToProperty("Category", i => i.Category);
            this.MapColumnToProperty("Nb", i => i.AmountOfEntries);
            this.MapColumnToProperty("Tot", i => i.TotalAmount);
            this.IsColumnSeparated(',');
        }
    }
}
```

### Define the ETL job

```csharp
using ComplexQuickstart.StreamTypes;
using System.IO;
using Paillave.Etl;
using Paillave.Etl.Core.Streams;
using System;

namespace ComplexQuickstart.Jobs
{
    public class ComplexQuickstartJob
    {
        public static void DefineProcess(IStream<MyConfig> rootStream)
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

### Execute the ETL job

```csharp
using Paillave.Etl;
using System.IO;
using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.TextFile.Core;
using ComplexQuickstart.Jobs;
using ComplexQuickstart.StreamTypes;
using Paillave.Etl.Core;

namespace ComplexQuickstart
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = StreamProcessRunner.Create<MyConfig>(ComplexQuickstartJob.DefineProcess);
            runner.GetDefinitionStructure().OpenEstimatedExecutionPlanVisNetwork();
            StreamProcessDefinition<TraceEvent> traceStreamProcessDefinition = new StreamProcessDefinition<TraceEvent>(traceStream => traceStream.ToAction("logs to console", Console.WriteLine));
            var testFilesDirectory = @"XXXXXXXXXXXXXXXX\Etl.Net\src\TestFiles";
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

This program first shows the estimated execution plan:

![Estimated execution plan](./docs/EstimatedExecutionPlan.PNG "Estimated execution plan")

Then it shows the actual execution with statistics when hovering streams, and input and outputs when hovering nodes:
![Actual execution plan](./docs/ActualExecutionPlan.PNG "Actual execution plan")

## Documentation :construction:

> [!IMPORTANT]
> Documentation will be done once all essential features and bugs are solved.
> 
[gitbook documentation](https://paillave.gitbook.io/etl-net/)
