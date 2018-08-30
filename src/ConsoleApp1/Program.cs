using Paillave.Etl;
using System;
//using System.Reactive.Linq;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using ConsoleApp1.StreamTypes;
using Paillave.Etl.Core;
using ConsoleApp1.Jobs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //new StreamProcessRunner<TestJob3, MyConfig>().GetDefinitionStructure().OpenVisNetworkStructure();
            var runner = new StreamProcessRunner<TestJob3, MyConfig>();
            StreamProcessDefinition<TraceEvent> traceStreamProcessDefinition = null;// new StreamProcessDefinition<TraceEvent>(traceStream => traceStream.Where("keep log info", i => i.Content.Level <= TraceLevel.Info).ToAction("logs to console", Console.WriteLine));
            // StreamProcessDefinition<TraceEvent> traceStreamProcessDefinition = new StreamProcessDefinition<TraceEvent>(traceStream => traceStream.ToAction("logs to console", Console.WriteLine));
            var testFilesDirectory = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\TestFiles\");
            var task = runner.ExecuteAsync(new MyConfig
            {
                InputFolderPath = Path.Combine(testFilesDirectory, @"."),
                InputFilesSearchPattern = "testin.*.csv",
                TypeFilePath = Path.Combine(testFilesDirectory, @"ref - Copy.csv"),
                DestinationFilePath = Path.Combine(testFilesDirectory, @"outfile.csv"),
                CategoryDestinationFilePath = Path.Combine(testFilesDirectory, @"categoryStats.csv")
            }, traceStreamProcessDefinition);
            task.Wait();

            //task.Result.OpenActualExecutionPlanD3Sankey();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
        static void MainOld(string[] args)
        {
            //new StreamProcessRunner<TestJob3, MyConfig>().GetDefinitionStructure().OpenVisNetworkStructure();
            var runner = new StreamProcessRunner<TestJob5, MyConfig2>();
            StreamProcessDefinition<TraceEvent> traceStreamProcessDefinition = null;// new StreamProcessDefinition<TraceEvent>(traceStream => traceStream.Where("keep log info", i => i.Content.Level <= TraceLevel.Info).ToAction("logs to console", Console.WriteLine));
            var task = runner.ExecuteAsync(new MyConfig2
            {
                ConnectionString = new System.Data.SqlClient.SqlConnectionStringBuilder
                {
                    DataSource = "localhost",
                    InitialCatalog = "TestDB",
                    IntegratedSecurity = true,
                    MultipleActiveResultSets = true
                }.ToString(),
                Filter = "a"
            }, traceStreamProcessDefinition);
            task.Wait();

            //task.Result.OpenActualExecutionPlanD3Sankey();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
