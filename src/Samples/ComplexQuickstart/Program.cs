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
using Paillave.Etl.Debugger.Extensions;

namespace ComplexQuickstart
{
    class Program
    {
        static void Main(string[] args)
        {
            var testFilesDirectory = @"C:\Users\sroyer\Source\Repos\Etl.Net\src\Samples\TestFiles";
            StreamProcessDebugger.OpenInDebugger(ComplexQuickstartJob.DefineProcess, new MyConfig
            {
                InputFolderPath = Path.Combine(testFilesDirectory, @"."),
                InputFilesSearchPattern = "testin.*.csv",
                TypeFilePath = Path.Combine(testFilesDirectory, @"ref - Copy.csv"),
                DestinationFilePath = Path.Combine(testFilesDirectory, @"outfile.csv"),
                CategoryDestinationFilePath = Path.Combine(testFilesDirectory, @"categoryStats.csv")
            }).Wait();

            Console.WriteLine("Done");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
