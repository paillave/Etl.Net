using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl.TextFile;
using Paillave.Etl.SqlServer;
using System.Data.SqlClient;

namespace SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            processRunner.DebugNodeStream += (sender, e) => { };
            using (var cnx = new SqlConnection(@"Server=localhost,1433;Database=SimpleTutorial;user=SimpleTutorial;password=TestEtl.TestEtl;MultipleActiveResultSets=True"))
            {
                cnx.Open();
                var executionOptions = new ExecutionOptions<string>
                {
                    Resolver = new SimpleDependencyResolver().Register(cnx),
                    TraceProcessDefinition = DefineTraceProcess,
                    UseDetailedTraces = true
                };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
                if (res.Failed)
                    Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
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
        private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
        {
            traceStream
                .Where("keep only summary of node and errors", i => i.Content is CounterSummaryStreamTraceContent || i.Content is UnhandledExceptionStreamTraceContent)
                .Select("create log entry", i => new
                {
                    DateTime = DateTime.Now,
                    Type = i.Content switch
                    {
                        CounterSummaryStreamTraceContent => "EndOfNode",
                        UnhandledExceptionStreamTraceContent => "Error",
                        _ => "Unknown"
                    },
                    Message = i.Content switch
                    {
                        CounterSummaryStreamTraceContent counterSummary => $"{i.NodeName}: {counterSummary.Counter}",
                        UnhandledExceptionStreamTraceContent unhandledException => $"{i.NodeName}({i.NodeTypeName}): [{unhandledException.Level.ToString()}] {unhandledException.Message}",
                        _ => "Unknown"
                    }
                })
                .ToTextFileValue("write log file", "log.csv", FlatFileDefinition.Create(i => new
                {
                    DateTime = i.ToDateColumn("datetime", "yyyy-MM-dd hh:mm:ss:ffff"),
                    Type = i.ToColumn("event type"),
                    Message = i.ToColumn("details"),
                }).IsColumnSeparated(','))
                .WriteToFile("save log file", i => i.Name);
        }
    }
}
