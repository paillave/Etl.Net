using Paillave.Etl.Core;
using Paillave.Etl.ExcelFile;
using Paillave.Etl.ExcelFile.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.TextFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.XTests
{
    public class ExcelTests
    {
        [Fact]
        public static async Task ReadFromExcelAsync()
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcessRead);
            var res = await processRunner.ExecuteAsync("InputFiles");
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }
        [Fact]

        public static async Task WriteToExcelAsync()
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcessWrite);
            var res = await processRunner.ExecuteAsync("InputFiles");
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }

        private static void DefineProcessWrite(ISingleStream<string> contextStream)
        {

            contextStream.CrossApplyFolderFiles("get data", "*.csv")
                .CrossApplyTextFile("Parse data", FlatFileDefinition.Create(i => new
                {
                    a = i.ToColumn("a"),
                    b = i.ToColumn("b")
                }).IsColumnSeparated(','))
                .ToExcelFile("to excel", "InputFiles/output.xlsx", ExcelFileDefinition.Create(i => new
                {
                    a = i.ToColumn("b"),
                    b = i.ToColumn("a"),
                }).HasColumnHeader("A1:B1")
                .WithDataset("A2:B2"))
                .WriteToFile("write", i => i.Name);
            // TODO: Define the ETL process here
        }

        private static void DefineProcessRead(ISingleStream<string> contextStream)
        {

            contextStream.CrossApplyFolderFiles("get data", "input.xlsx")
                .CrossApplyExcelSheets("Parse data")
                .CrossApplyExcelRows("get all rows", o => o.UseMap(m => new
                {
                    a = m.ToColumn("a"),
                    b = m.ToColumn("b"),

                }).HasColumnHeader("A1:B1").WithDataset("A2:B2"))
                .Do("show data", i => Console.WriteLine($"a = {i.a}, b = {i.b}"));
        }
    }
}
