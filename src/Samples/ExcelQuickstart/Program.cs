using Paillave.Etl;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.ExcelFile.Core;
using System;
using System.IO;

namespace ExcelQuickstart
{
    public class SimpleConfig
    {
        public string InputDirectory { get; set; }
        public string OutputFile { get; set; }
    }
    public class XlFileTest
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
    public class ExcelQuickstartJob : IStreamProcessDefinition<SimpleConfig>
    {
        public string Name => "Excel quickstart";

        public void DefineProcess(IStream<SimpleConfig> rootStream)
        {
            var outputFile = rootStream.Select("open output file", i => File.OpenWrite(i.OutputFile));
            rootStream
                .CrossApplyFolderFiles("get excel files", i => i.InputDirectory, (f, r) => f.Name, "*.xlsx")
                .Select("link to root stream", rootStream, (i, s) => new { FileName = i, OutputFileName = s.OutputFile })
                .Where("exclude output file", i => !string.Equals(i.FileName, i.OutputFileName, StringComparison.InvariantCultureIgnoreCase))
                .CrossApplyExcelSheets("get excel sheets", i => i.FileName)
                .Where("get only FromDataTable worksheet", i => i.Name == "FromDataTable")
                .CrossApplyExcelRows("get xlsheet content", new ExcelFileDefinition<XlFileTest>()
                    .HasColumnHeader("A1:D1")
                    .WithDataset("A2:D2")
                    .MapColumnToProperty("Name", i => i.Name)
                    .MapColumnToProperty("Size", i => i.Size)
                    .MapColumnToProperty("Modified", i => i.Modified)
                    .MapColumnToProperty("Created", i => i.Created)
                )
                .Select("transform", i => new { TheName = i.Name, i.Modified })
                //.CrossApplyExcelSheets("get excel sheets", (s, f) => new { Sheet = s.Name, File = f })
                .ToExcelFile("write to output file", outputFile)
                .ToAction("write to console", i => Console.WriteLine($"{i.TheName} -> {i.Modified}"));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var testFilesDirectory = @"C:\Users\sroyer\Source\Repos\Etl.Net\src\Samples\TestFiles";
            // var testFilesDirectory = @"C:\Users\paill\Documents\GitHub\Etl.Net\src\Samples\TestFiles";

            new StreamProcessRunner<ExcelQuickstartJob, SimpleConfig>().ExecuteAsync(new SimpleConfig
            {
                InputDirectory = testFilesDirectory,
                OutputFile = @"C:\Users\sroyer\Source\Repos\Etl.Net\src\Samples\testoutput.xlsx"
            }, null).Wait();
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}
