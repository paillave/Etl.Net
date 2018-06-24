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
                var tmp1 = src.OutputStream.Take("take first header line only", 1);
                var tmp2 = src.OutputStream.Skip("take everything after the first line", 1);
                tmp1.Merge("merge streams", tmp2).Observable.Subscribe(Console.WriteLine);
                src.OutputStream.Observable.Subscribe(Console.WriteLine);
                tmp1.Observable.Subscribe(Console.WriteLine);
                tmp2.Observable.Subscribe(Console.WriteLine);
                ctx.StartAsync().Wait();
            }
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
