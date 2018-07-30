using Paillave.Etl.Core.StreamNodes;
using System;
//using System.Reactive.Linq;
using System.IO;
using System.Globalization;
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
            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.NumberFormat.NumberDecimalSeparator = ",";
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
            ci.NumberFormat.PercentDecimalSeparator = ",";

            var ctx = new ExecutionContext<MyClass>("import file");
            ctx.TraceStream.Where("keep log info", i => i.Content.Level <= System.Diagnostics.TraceLevel.Info).ToAction("logs to console", Console.WriteLine);

            var parsedLineS = ctx.StartupStream
                .Select("get input file path", i => i.FilePath)
                .CrossApplyParsedFile("parse input file", new CrossApplyParsedFileArgs<Class1>(Mappers.ColumnNameStringParserMappers<Class1>()
                    .WithGlobalCultureInfo(ci)
                    .MapColumnToProperty("#", i => i.Id)
                    .MapColumnToProperty("DateTime", i => i.Col1)
                    .MapColumnToProperty("Value", i => i.Col2)
                    .MapColumnToProperty("Rank", i => i.Col3)
                    .MapColumnToProperty("Comment", i => i.Col4)
                    .MapColumnToProperty("TypeId", i => i.TypeId), '\t'))
                .EnsureSorted("Ensure input file is sorted", i => SortCriteria.Create(i, e => e.TypeId));

            var parsedTypeLineS = ctx.StartupStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyParsedFile("parse type input file", new CrossApplyParsedFileArgs<Class2>(Mappers.ColumnNameStringParserMappers<Class2>()
                    .WithGlobalCultureInfo(ci)
                    .MapColumnToProperty("#", i => i.Id)
                    .MapColumnToProperty("Label", i => i.Name), '\t'))
                .EnsureKeyed("Ensure type file is keyed", i => SortCriteria.Create(i, e => e.Id));

            parsedLineS.LeftJoin("join types to file", parsedTypeLineS, (l, r) => new { l.Id, r.Name })
                .Select("output after join", i => $"{i.Id}->{i.Name}")
                .ToAction("write to console", Console.WriteLine);

            ctx.Configure(new MyClass
            {
                FilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\test - Copy - Copy.txt",
                TypeFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\ref - Copy.txt"
            });

            ctx.ExecuteAsync().Wait();

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
