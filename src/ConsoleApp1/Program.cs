using Paillave.Etl.Core.StreamNodes;
using System;
using System.Reactive.Linq;
using System.IO;
using System.Globalization;
using System.Reactive.Subjects;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core.MapperFactories;
using ConsoleApp1.StreamTypes;
using Paillave.Etl.Core.System;

namespace ConsoleApp1
{
    public class MyClass
    {
        public string FilePath { get; set; }
        public string TypeFilePath { get; set; }
    }
    class Program
    {
        // https://www.nuget.org/packages/EPPlus
        static void Main(string[] args)
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-GB");
            ci.DateTimeFormat.FullDateTimePattern = "yyyy-dd-MM HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-dd-MM";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-dd-MM";

            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.NumberFormat.NumberDecimalSeparator = ",";
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
            ci.NumberFormat.PercentDecimalSeparator = ",";

            var ctx = new ExecutionContext<MyClass>("import file");
            //ctx.TraceStream.Observable.Where(i => i.Content.Level <= System.Diagnostics.TraceLevel.Info).Subscribe(Console.WriteLine);

            #region Main file
            var splittedLineS = ctx.StartupStream
                .Select("Open file", i => (Stream)File.OpenRead(i.FilePath))
                .CrossApplyDataStream("Read file")
                .Select("split lines", Mappers.CsvLineSplitter('\t'));

            var lineParserS = splittedLineS
                .Top("take first header line only", 1)
                .Select("create line processor", Mappers.ColumnNameStringParserMappers<Class1>()
                    .WithGlobalCultureInfo(ci)
                    .MapColumnToProperty("#", i => i.Id)
                    .MapColumnToProperty("DateTime", i => i.Col1)
                    .MapColumnToProperty("Value", i => i.Col2)
                    .MapColumnToProperty("Rank", i => i.Col3)
                    .MapColumnToProperty("Comment", i => i.Col4)
                    .MapColumnToProperty("TypeId", i => i.TypeId)
                    .LineParser);
            var dataLineS = splittedLineS.Skip("take everything after the first line", 1);
            var parsedLineS = dataLineS.CombineLatest("parse every line", lineParserS, (dataLine, lineParser) => lineParser(dataLine))
                .EnsureSorted("Ensure input file is sorted", i => SortCriteria.Create(i, e => e.TypeId));
            #endregion

            #region Type file
            var splittedTypeLineS = ctx.StartupStream
                .Select("Open type file", i => (Stream)File.OpenRead(i.TypeFilePath))
                .CrossApplyDataStream("Read type file")
                .Select("split type lines", Mappers.CsvLineSplitter('\t'));

            var typeLineParserS = splittedTypeLineS
                .Top("take first header type line only", 1)
                .Select("create type line processor", Mappers.ColumnNameStringParserMappers<Class2>()
                    .WithGlobalCultureInfo(ci)
                    .MapColumnToProperty("#", i => i.Id)
                    .MapColumnToProperty("Label", i => i.Name)
                    .LineParser);
            var dataTypeLineS = splittedTypeLineS.Skip("take everything after the first type line", 1);

            var parsedTypeLineS = dataTypeLineS.CombineLatest("parse every type line", typeLineParserS, (dataLine, lineParser) => lineParser(dataLine))
                .EnsureKeyed("Ensure type file is keyed", i => SortCriteria.Create(i, e => e.Id));
            #endregion

            parsedLineS.LeftJoin("join types to file", parsedTypeLineS, (l, r) => new { l.Id, r.Name })
                //.Where("check any issue", i => i.Name == null)
                .Select("output after join", i => $"{i.Id}->{i.Name}").Observable.Subscribe(Console.WriteLine);

            ctx.Configure(new MyClass
            {
                FilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\test - Copy.txt",
                TypeFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\ref - Copy.txt"
            });

            ctx.ExecuteAsync().Wait();

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
