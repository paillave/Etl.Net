using Paillave.Etl;
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
using Paillave.Etl.Core.TraceContents;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<TraceEvent> traceEvents = new List<TraceEvent>();
            var ctx = new TestJob1();
            ctx.TraceStream.Where("keep log info", i => i.Content.Level <= System.Diagnostics.TraceLevel.Info).ToAction("logs to console", Console.WriteLine);
            //Type counterSummaryStreamTraceContentType = typeof(CounterSummaryStreamTraceContent);
            ctx.TraceStream.Where("keep stream results", i => i.Content is CounterSummaryStreamTraceContent).ToAction("", traceEvents.Add);
            ctx.ExecuteAsync(new MyConfig
            {
                InputFolderPath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\",
                InputFilesSearchPattern = "testin.*.txt",
                TypeFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\ref - Copy.txt",
                DestinationFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\outfile.csv"
            }).Wait();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
