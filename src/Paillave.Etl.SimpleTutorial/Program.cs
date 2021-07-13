using System;
using System.Threading.Tasks;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl;
using Paillave.Etl.TextFile;
using Paillave.Etl.TextFile.Core;
using Paillave.Etl.SqlServer.Extensions;
using System.Data.SqlClient;
using Paillave.Etl.Core;
using Paillave.Etl.ExecutionToolkit;

namespace Paillave.Etl.SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            using (var cnx = new SqlConnection(@"Server=localhost,1433;Database=SimpleTutorial;user=SimpleTutorial;password=TestEtl.TestEtl;MultipleActiveResultSets=True"))
            {
                cnx.Open();
                var executionOptions = new ExecutionOptions<string>
                {
                    Resolver = new SimpleDependencyResolver().Register(cnx)
                };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
                res.OpenActualExecutionPlan();
            }
        }
        private class Person
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public int? Reputation { get; set; }
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApplyFolderFiles("list all required files", "*.zip", true)
                .CrossApplyZipFiles("extract files from zip", "*.csv")
                .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
                    {
                        Email = i.ToColumn("email"),
                        FirstName = i.ToColumn("first name"),
                        LastName = i.ToColumn("last name"),
                        DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
                        Reputation = i.ToNumberColumn<int?>("reputation", ".")
                    }).IsColumnSeparated(','))
                .Distinct("exclude duplicates", i => i.Email)
                .SqlServerSave("save in DB", "dbo.Person", p => p.Email, p => p.Id)
                .Do("display ids on console", i => Console.WriteLine(i.Id));
        }
    }
}
