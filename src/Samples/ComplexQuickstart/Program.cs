using Paillave.Etl;
using Paillave.Etl.Extensions;
using System.IO;
using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.TextFile.Core;
using ComplexQuickstart.Jobs;
using ComplexQuickstart.StreamTypes;
using Paillave.Etl.Core;
using Paillave.Etl.ExecutionPlan;
using Paillave.Etl.ExecutionPlan.Extensions;
// using Paillave.Etl.Debugger.Extensions;

namespace ComplexQuickstart
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = StreamProcessRunner.Create<MyConfig>(ComplexQuickstartJob.DefineProcess);
            Action<IStream<TraceEvent>> traceStreamProcessDefinition = traceStream => traceStream.ThroughAction("logs to console", Console.WriteLine);
            var testFilesDirectory = @"C:\Users\paill\Documents\GitHub\Etl.Net\src\Samples\TestFiles";
            var task = runner.ExecuteAsync(new MyConfig
            {
                InputFolderPath = Path.Combine(testFilesDirectory, @"."),
                InputFilesSearchPattern = "testin.*.csv",
                TypeFilePath = Path.Combine(testFilesDirectory, @"ref - Copy.csv"),
                DestinationFilePath = Path.Combine(testFilesDirectory, @"outfile.csv"),
                CategoryDestinationFilePath = Path.Combine(testFilesDirectory, @"categoryStats.csv")
            }, traceStreamProcessDefinition, true);
            task.Wait();
            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
