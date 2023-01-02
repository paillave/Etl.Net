using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.TextFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.XTests
{
    public class TextFileTests
    {
        [Fact]
        public static async Task ReadFromTextFile()
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcessRead);
            var res = await processRunner.ExecuteAsync("InputFiles");
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }
        [Fact]
        public static async Task WriteToTextFile()
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
                .ToTextFileValue("to excel", "InputFiles/output.csv", FlatFileDefinition.Create(i => new
                {
                    a = i.ToColumn("b"),
                    b = i.ToColumn("a"),
                }).IsColumnSeparated(','))
                .WriteToFile("write to file", i => i.Name);
        }

        private static void DefineProcessRead(ISingleStream<string> contextStream)
        {

            contextStream.CrossApplyFolderFiles("get data", "*.csv")
                .CrossApplyTextFile("Parse data", FlatFileDefinition.Create(i => new
                {
                    a = i.ToColumn("a"),
                    b = i.ToColumn("b")
                }).IsColumnSeparated(','))
                .Do("show data", i => Console.WriteLine($"a = {i.a}, b = {i.b}"));
        }
    }
}
