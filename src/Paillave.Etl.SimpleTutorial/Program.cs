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
            processRunner.DebugNodeStream += (sender, e) => { /* PLACE A CONDITIONAL BREAKPOINT HERE FOR DEBUG */ };
            using (var cnx = new SqlConnection(args[1]))
            {
                cnx.Open();
                var executionOptions = new ExecutionOptions<string>
                {
                    Resolver = new SimpleDependencyResolver().Register(cnx),
                    TraceProcessDefinition = DefineTraceProcess,
                    // UseDetailedTraces = true // activate only if per row traces are meant to be caught
                };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
                if (res.Failed)
                    Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
            }
        }
        private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
        {
            traceStream
                .Where("keep only summary of node and errors", i => i.Content is CounterSummaryStreamTraceContent || i.Content is UnhandledExceptionStreamTraceContent)
                .Select("create log entry", i => new ExecutionLog
                {
                    DateTime = i.DateTime,
                    ExecutionId = i.ExecutionId,
                    EventType = i.Content switch
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
              .SqlServerSave("save traces", "dbo.ExecutionTrace");
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
              .Distinct("exclude duplicates based on the Email", i => i.Email)
              .SqlServerSave("upsert using Email as key and ignore the Id", "dbo.Person", p => p.Email, p => p.Id)
              .Select("define row to report", i => new { i.Email, i.Id })
              .ToTextFileValue("write summary to file", "report.csv", FlatFileDefinition.Create(i => new
              {
                  Email = i.ToColumn("Email"),
                  Id = i.ToNumberColumn<int>("new or existing Id", ".")
              }).IsColumnSeparated(','))
              .WriteToFile("save log file", i => i.Name);
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
        private class ExecutionLog
        {
            public DateTime DateTime { get; set; }
            public Guid ExecutionId { get; set; }
            public string EventType { get; set; }
            public string Message { get; set; }
        }
    }
}



// IF OBJECT_ID('[dbo].[ExecutionTrace]', 'U') IS NOT NULL
// DROP TABLE [dbo].[ExecutionTrace]
// GO
// CREATE TABLE [dbo].[ExecutionTrace]
// (
//     [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- Primary Key column
//     [ExecutionId] UNIQUEIDENTIFIER NOT NULL,
//     [DateTime] DATETIME2 NOT NULL,
//     [EventType] NVARCHAR(255) NOT NULL,
//     [Message] NVARCHAR(MAX) NOT NULL,
// );
// GO
