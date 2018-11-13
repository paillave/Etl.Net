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
            var task = StreamProcessRunner.CreateAndExecuteAsync(
                new ImportFilesConfig { InputFilesRootFolderPath = @"C:\Users\sroyer\Downloads\RBC" },
                // new ImportFilesConfig { InputFilesRootFolderPath = @"C:\Users\paill\Documents\GitHub\Etl.Net\src\Samples\TestFiles\RBC" },
                ImportFiles.DefineProcess);
            try
            {
                task.Wait();
            }
            catch (Paillave.Etl.Core.JobExecutionException ex)
            {
            }
            catch (Exception ex)
            {
                var jex = ex.InnerException as Paillave.Etl.Core.JobExecutionException;
                var te = jex.TraceEvent;
            }
        }
    }
}
