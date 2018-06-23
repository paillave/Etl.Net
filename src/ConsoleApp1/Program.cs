using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.System;
using System;
using System.Reactive.Linq;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new StreamTraceExecutionContext())
            {
                ctx.ProcessTraceStream.Observable.Where(i => i.ProcessTrace.Level <= System.Diagnostics.TraceLevel.Info).Subscribe(Console.WriteLine);
                var src = new DataStreamSourceNode(ctx, "text file source") { InputDataStream = File.OpenRead(@"C:\Users\paill\source\repos\Etl.Net\src\TestFiles\test.txt") };
                src.OutputStream.Observable.Subscribe();
                ctx.StartAsync().Wait();
            }
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
