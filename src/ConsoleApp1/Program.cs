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
    class Program
    {
        // https://www.nuget.org/packages/EPPlus
        static void Main(string[] args)
        {
            var ctx = new ExecutionContext<MyConfig>("import file");
            ctx.TraceStream.Where("keep log info", i => i.Content.Level <= System.Diagnostics.TraceLevel.Info).ToAction("logs to console", Console.WriteLine);

            var parsedLineS = ctx.StartupStream
                .CrossApplyFolderFiles("get folder files", i => i.FolderPath, "*.txt")
                .CrossApplyParsedFile("parse input file", new Class1Mapper(), (i, p) => { p.FileName = i; return p; })
                .Sort("sort input file", i => SortCriteria.Create(i, e => e.TypeId));
            //.EnsureSorted("Ensure input file is sorted", i => SortCriteria.Create(i, e => e.TypeId));

            //parsedLineS.ToAction("write to console", i => Console.WriteLine($"{i.FileName} - {i.Id}"));

            var parsedTypeLineS = ctx.StartupStream
                .Select("get input file type path", i => i.TypeFilePath)
                .CrossApplyParsedFile("parse type input file", new Class2Mapper())
                .EnsureKeyed("Ensure type file is keyed", i => SortCriteria.Create(i, e => e.Id));

            parsedLineS.LeftJoin("join types to file", parsedTypeLineS, (l, r) => new { l.Id, r.Name, l.FileName })
                .Select("output after join", i => $"{i.FileName}:{i.Id}->{i.Name}")
                .ToAction("write to console", Console.WriteLine);

            ctx.Configure(new MyConfig
            {
                FolderPath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\tmp",
                TypeFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\ref - Copy.txt"
            });

            ctx.ExecuteAsync().Wait();

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
