using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.System;
using System;
using System.Reactive.Linq;
using System.IO;
using Paillave.Etl.Core.Helpers;
using System.Globalization;

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

            ILineSplitter lineSplitter = new SeparatorLineSplitter("\t");

            using (var ctx = new StreamTraceExecutionContext())
            {
                ctx.ProcessTraceStream.Observable.Where(i => i.ProcessTrace.Level <= System.Diagnostics.TraceLevel.Info).Subscribe(Console.WriteLine);
                var src = new DataStreamSourceNode(ctx, "text file source") { InputDataStream = File.OpenRead(@"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\test.txt") }
                .OutputStream.Map("split lines", lineSplitter.Split);
                var lineProcessor = src.Take("take first header line only", 1).Map("create line processor", h => new LineColumnNameParserConfiguration<Class1>(h)
                    .WithGlobalCultureInfo(ci)
                    .MapColumnToProperty("DateTime", i => i.Col1)
                    .MapColumnToProperty("Value", i => i.Col2)
                    .MapColumnToProperty("Rank", i => i.Col3)
                    .MapColumnToProperty("Comment", i => i.Col4)
                    .GetLineProcessor());
                var dataLines = src.Skip("take everything after the first line", 1);
                var parsedLines = dataLines.CombineWithLatest("parse every line", lineProcessor, (l, lp) => lp.Parse(l));
                //tmp1.Merge("merge streams", tmp2).Observable.Subscribe(Console.WriteLine);
                //src.OutputStream.Observable.Subscribe(Console.WriteLine);
                //lineProcessor.Observable.Subscribe(Console.WriteLine);
                //parsedLines.Observable.Subscribe(i => Console.WriteLine($"{i.Col1}-{i.Col2}-{i.Col3}-{i.Col4}"));
                parsedLines.Observable.Subscribe();
                ctx.StartAsync().Wait();
            }
            Console.WriteLine("Done");
            //var tmp = new FixedColumnsLineSplitter(2, 4, 3).Split("1234567890");
            Console.ReadKey();
        }
    }
}
