using Paillave.Etl;
using System;
using System.IO;
using Paillave.Etl.Core;
using Paillave.Etl.TextFile.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.TextFile;

namespace SimpleQuickstart
{
    class Program
    {
        static void Main(string[] args)
        {
            var testFilesDirectory = args[0];

            new StreamProcessRunner<SimpleQuickstartJob, SimpleConfig>().ExecuteAsync(new SimpleConfig
            {
                InputFilePath = Path.Combine(testFilesDirectory, "simpleinputfile.csv"),
                OutputFilePath = Path.Combine(testFilesDirectory, "simpleoutputfile.csv")
            }, null).Wait();
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
