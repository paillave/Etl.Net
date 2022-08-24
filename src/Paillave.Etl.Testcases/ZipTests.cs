using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.TextFile;
using Paillave.Etl.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Testcases
{
    internal class ZipTests
    {
        public static async Task ReadFromZipAsync()
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcessRead);
            var res = await processRunner.ExecuteAsync("InputFiles");
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }
        public static async Task WriteToTextZipAsync()
        {
            throw new NotImplementedException();
        }
        private static void DefineProcessWrite(ISingleStream<string> contextStream)
        {
            throw new NotImplementedException();
        }

        private static void DefineProcessRead(ISingleStream<string> contextStream)
        {

            contextStream.CrossApplyFolderFiles("get data", "*.zip")
                .CrossApplyZipFiles("extract files from zip", "*.csv")
                .CrossApplyTextFile("Parse data", FlatFileDefinition.Create(i => new
                {
                    a = i.ToColumn("a"),
                    b = i.ToColumn("b")
                }).IsColumnSeparated(','))
                .Do("show data", i => Console.WriteLine($"a = {i.a}, b = {i.b}"));
        }
    }
}
