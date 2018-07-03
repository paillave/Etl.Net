using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.SystemOld;
using System;
using System.Reactive.Linq;
using System.IO;
using System.Globalization;
using System.Reactive.Subjects;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core.MapperFactories;
using ConsoleApp1.StreamTypes;

namespace ConsoleApp1
{
    class Program
    {
        // https://www.nuget.org/packages/EPPlus
        static void Main(string[] args)
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-GB");
            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            ci.NumberFormat.NumberDecimalSeparator = ",";
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
            ci.NumberFormat.PercentDecimalSeparator = ",";

            using (var ctx = new StreamTraceExecutionContext())
            {
                ctx.ProcessTraceStream.Observable.Where(i => i.ProcessTrace.Level <= System.Diagnostics.TraceLevel.Info).Subscribe(Console.WriteLine);

                var splittedLineS = new DataStreamSourceNode(ctx, "text file source") { InputDataStream = File.OpenRead(@"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\test - Copy.txt") }
                    .Output
                    .Map("split lines", Mappers.CsvLineSplitter('\t'));
                var lineParserS = splittedLineS
                    .Take("take first header line only", 1)
                    .Map("create line processor", Mappers.ColumnNameStringParserMappers<Class1>()
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



                var splittedTypeLineS = new DataStreamSourceNode(ctx, "type file source") { InputDataStream = File.OpenRead(@"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\ref - Copy.txt") }
                    .Output
                    .Map("split type file lines", Mappers.CsvLineSplitter('\t'));
                var typeLineParserS = splittedTypeLineS
                    .Take("take first header type line only", 1)
                    .Map("create type line processor", Mappers.ColumnNameStringParserMappers<Class2>()
                        .WithGlobalCultureInfo(ci)
                        .MapColumnToProperty("#", i => i.Id)
                        .MapColumnToProperty("Label", i => i.Name)
                        .LineParser);
                var dataTypeLineS = splittedTypeLineS.Skip("take everything after the first type line", 1);
                var parsedTypeLineS = dataTypeLineS.CombineLatest("parse every type line", typeLineParserS, (dataLine, lineParser) => lineParser(dataLine))
                    .EnsureKeyed("Ensure type file is keyed", i => SortCriteria.Create(i, e => e.Id));


                parsedLineS.LeftJoin("join types to file", parsedTypeLineS, (l, r) => new { l.Id, r.Name })
                    .Map("output after join", i => $"\t\t\t{i.Id}->{i.Name}").Observable.Subscribe(Console.WriteLine);


                parsedTypeLineS.Observable.Subscribe();
                ctx.StartAsync().Wait();
            }
            Console.WriteLine("Done");
            //var tmp = new FixedColumnsLineSplitter(2, 4, 3).Split("1234567890");
            Console.ReadKey();
        }
    }
}
