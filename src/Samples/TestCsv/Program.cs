using System;
using Paillave.Etl.Core;
using Paillave.Etl;
using TestCsv.StreamTypes;
using TestCsv.Jobs;

namespace TestCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamProcessRunner.CreateAndExecuteAsync(
                new ImportFilesConfig { InputFilesRootFolderPath = @"C:\Users\paill\source\repos\PMS\src\FundProcess.Pms.ImportsTests\TestFiles\RBC" },
                ImportFiles.DefineProcess).Wait();
        }
    }
}
