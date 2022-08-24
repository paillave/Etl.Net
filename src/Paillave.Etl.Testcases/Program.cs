

using Paillave.Etl.Core;
using Paillave.Etl.ExcelFile;
using Paillave.Etl.ExcelFile.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.TextFile;

namespace Paillave.Etl.Testcases
{
    class Program
    {


        static async Task Main(string[] args)
        {

            await ExcelTests.ReadFromExcelAsync();
        }
    }
}