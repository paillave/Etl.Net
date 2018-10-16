using Paillave.Etl;
using System.IO;
using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.TextFile.Core;
using SubProcessQuickStart.Jobs;
using SubProcessQuickStart.StreamTypes;
using Paillave.Etl.Core;
using Paillave.Etl.ExecutionPlan;

namespace SubProcessQuickStart
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new StreamProcessRunner<SubProcessQuickstartJob, MyConfig>();
            runner.GetDefinitionStructure().OpenEstimatedExecutionPlanVisNetwork();
            TraceStreamProcessDefinition traceStreamProcessDefinition = new TraceStreamProcessDefinition(traceStream => traceStream.ThroughAction("logs to console", Console.WriteLine));
            var testFilesDirectory = @"C:\Users\sroyer\Source\Repos\Etl.Net\src\Samples\TestFiles";
            // var testFilesDirectory = @"C:\Users\paill\source\repos\Etl.Net\src\Samples\TestFiles";
            var task = runner.ExecuteAsync(new MyConfig
            {
                InputFolderPath = Path.Combine(testFilesDirectory, @"."),
                InputFilesSearchPattern = "testin.*.csv",
                TypeFilePath = Path.Combine(testFilesDirectory, @"ref - Copy.csv"),
                DestinationFilePath = Path.Combine(testFilesDirectory, @"outfile.csv"),
                CategoryDestinationFolder = Path.Combine(testFilesDirectory, @"categoryStats")
            }, traceStreamProcessDefinition);
            task.Result.OpenActualExecutionPlanD3Sankey();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
