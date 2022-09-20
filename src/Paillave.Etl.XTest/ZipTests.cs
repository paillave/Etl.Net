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
    public class ZipTests
    {
        [Fact]
        public static async Task ReadFromZipAsync()
        {
            var processRunner = Core.StreamProcessRunner.Create<string>(DefineProcessRead);
            var res = await processRunner.ExecuteAsync("InputFiles");
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }
        //[Fact]
        public static async Task WriteToTextZipAsync()
        {
            throw new NotImplementedException();
        }
        private static void DefineProcessWrite(Core.ISingleStream<string> contextStream)
        {
            throw new NotImplementedException();
        }

        private static void DefineProcessRead(Core.ISingleStream<string> contextStream)
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
