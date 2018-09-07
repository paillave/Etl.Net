using Paillave.Etl;
using Paillave.Etl.Core.Streams;
using System;
using System.IO;

namespace ExcelQuickstart
{
    public class SimpleConfig
    {
        public string InputDirectory { get; set; }
    }
    public class ExcelQuickstartJob : IStreamProcessDefinition<SimpleConfig>
    {
        public string Name => "Excel quickstart";

        public void DefineProcess(IStream<SimpleConfig> rootStream)
        {
            rootStream
                .CrossApplyFolderFiles("get excel files", i => i.InputDirectory, "*.xlsx")
                .CrossApplyExcelSheets("get excel sheets", (s, f) => new { Sheet = s.Name, File = f })
                .ToAction("write to console", i => Console.WriteLine($"{i.File} -> {i.Sheet}"));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //var testFilesDirectory = @"C:\Users\sroyer\Source\Repos\Etl.Net\src\Samples\TestFiles";
            var testFilesDirectory = @"C:\Users\paill\source\repos\Etl.Net\src\Samples\TestFiles";

            new StreamProcessRunner<ExcelQuickstartJob, SimpleConfig>().ExecuteAsync(new SimpleConfig
            {
                InputDirectory = testFilesDirectory
            }, null).Wait();
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
