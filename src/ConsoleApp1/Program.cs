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
using Paillave.RxPush.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new TestJob1();
            ctx.TraceStream.Where("keep log info", i => i.Content.Level <= System.Diagnostics.TraceLevel.Info).ToAction("logs to console", Console.WriteLine);
            //Type counterSummaryStreamTraceContentType = typeof(CounterSummaryStreamTraceContent);
            //var sankeyStatisticsTask = ctx.GetHtmlD3SankeyStatisticsAsync();
            var sankeyStatisticsTask = ctx.GetHtmlPlotlySankeyStatisticsAsync();
            ctx.ExecuteAsync(new MyConfig
            {
                InputFolderPath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\",
                InputFilesSearchPattern = "testin.*.txt",
                TypeFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\ref - Copy.txt",
                DestinationFilePath = @"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\outfile.csv"
            }).Wait();

            File.WriteAllText("sankeyStats.html", sankeyStatisticsTask.Result);


            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@"sankeyStats.html")
            {
                UseShellExecute = true
            };
            p.Start();

            //Process.Start("sankeyStats.html");
            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
