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

namespace ComplexQuickstart
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = StreamProcessRunner.Create<MyConfig>(ComplexQuickstartJob.DefineProcess);
            runner.GetDefinitionStructure().OpenEstimatedExecutionPlanVisNetwork();
            Action<IStream<TraceEvent>> traceStreamProcessDefinition = traceStream => traceStream.ThroughAction("logs to console", Console.WriteLine);
            var testFilesDirectory = @"C:\Users\sroyer\Source\Repos\Etl.Net\src\Samples\TestFiles";
            // var testFilesDirectory = @"C:\Users\paill\source\repos\Etl.Net\src\Samples\TestFiles";
            var task = runner.ExecuteAsync(new MyConfig
            {
                InputFolderPath = Path.Combine(testFilesDirectory, @"."),
                InputFilesSearchPattern = "testin.*.csv",
                TypeFilePath = Path.Combine(testFilesDirectory, @"ref - Copy.csv"),
                DestinationFilePath = Path.Combine(testFilesDirectory, @"outfile.csv"),
                CategoryDestinationFilePath = Path.Combine(testFilesDirectory, @"categoryStats.csv")
            }, traceStreamProcessDefinition);
            task.Result.OpenActualExecutionPlanD3Sankey();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
