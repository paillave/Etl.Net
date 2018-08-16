using Paillave.Etl.StreamNodes;
using System;
//using System.Reactive.Linq;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.MapperFactories;
using ConsoleApp1.StreamTypes;
using Paillave.Etl.Core;
using ConsoleApp1.Jobs;

namespace ConsoleApp1
{
    class Program
    {
        // https://www.nuget.org/packages/EPPlus
        static void Main(string[] args)
        {
            var ctx = new TestJob1();
            ctx.TraceStream.Where("keep log info", i => i.Content.Level <= System.Diagnostics.TraceLevel.Info).ToAction("logs to console", Console.WriteLine);

            ctx.ExecuteAsync(new MyConfig
            {
                InputFolderPath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\",
                InputFilesSearchPattern = "testin.*.txt",
                TypeFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\ref - Copy.txt",
                DestinationFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\outfile.txt"
            }).Wait();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
