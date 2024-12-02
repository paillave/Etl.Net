import React from 'react';
import clsx from 'clsx';
import styles from './HomepageExamples.module.css';
import Highlight, { defaultProps } from "prism-react-renderer";
import "prismjs"; // eslint-disable-line
require(`prismjs/components/prism-csharp`); // eslint-disable-line
import theme from "prism-react-renderer/themes/dracula";
import { WithLineNumbers } from './WithLineNumbers';
// https://emojipedia.org/

const features: IExample[]  = [
//   {
//     title: 'Unzip it, read it, save it, report it',
//     sourceCode: `
// private static void DefineProcess(ISingleStream<string> contextStream)
// {
//   contextStream
//     .CrossApplyFolderFiles("list all required files", "*.zip", true)
//     .CrossApplyZipFiles("extract files from zip", "*.csv")
//     .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
//     {
//       Email = i.ToColumn("email"),
//       FirstName = i.ToColumn("first name"),
//       LastName = i.ToColumn("last name"),
//       DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
//       Reputation = i.ToNumberColumn<int?>("reputation", ".")
//     }).IsColumnSeparated(','))
//     .Distinct("exclude duplicates based on the Email", i => i.Email)
//     .SqlServerSave("upsert using Email as key and ignore the Id", o => o
//       .ToTable("dbo.Person")
//       .SeekOn(p => p.Email)
//       .DoNotSave(p => p.Id))
//     .Select("define row to report", i => new { i.Email, i.Id })
//     .ToTextFileValue("write summary to file", "report.csv", FlatFileDefinition.Create(i => new
//     {
//       Email = i.ToColumn("Email"),
//       Id = i.ToNumberColumn<int>("new or existing Id", ".")
//     }).IsColumnSeparated(','))
//     .WriteToFile("save log file", i => i.Name);
// }
//     `,
//     description: (
//       <>
//         Read all zip files from a folder, unzip csv files that are inside, parse them, exclude duplicates, upsert them into database, and report new or pre existing id corresponding to the email.
//       </>
//     ),
//   },
  {
    title: 'Run it, debug it, track it, log it',
    sourceCode: `
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl.TextFile;
using Paillave.Etl.SqlServer;
using Microsoft.Data.SqlClient;

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
    private static void DefineProcess(ISingleStream<string> contextStream)
    {
      // TODO: define your ELT process here
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
        .SqlServerSave("save traces", o => o.ToTable("dbo.ExecutionTrace"));
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
        `,
    description: (
      <>
        Execute an ETL process, debug it by tracking debug events using the IDE debugger, catch execution events and log it into database.
      </>
    ),
  },
  {
    title: 'Normalize it',
    sourceCode: `
private static void DefineProcess(ISingleStream<string> contextStream)
{
  var rowStream = contextStream
    .CrossApplyFolderFiles("list all required files", "*.csv", true)
    .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new
    {
      Author = i.ToColumn("author"),
      Email = i.ToColumn("email"),
      TimeSpan = i.ToDateColumn("timestamp", "yyyyMMddHHmmss"),
      Category = i.ToColumn("category"),
      Link = i.ToColumn("link"),
      Post = i.ToColumn("post"),
      Title = i.ToColumn("title"),
    }).IsColumnSeparated(','))
    .SetForCorrelation("set correlation for row");

  var authorStream = rowStream
    .Distinct("remove author duplicates based on emails", i => i.Email)
    .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
    .EfCoreSaveCorrelated("save or update authors", o => o
      .SeekOn(i => i.Email)
      .AlternativelySeekOn(i => i.Name));

  var categoryStream = rowStream
    .Distinct("remove category duplicates", i => i.Category)
    .Select("create category instance", i => new Category { Code = i.Category, Name = i.Category })
    .EfCoreSaveCorrelated("insert categories if doesn't exist, get it otherwise", o => o
      .SeekOn(i => i.Code)
      .DoNotUpdateIfExists());

  var postStream = rowStream
    .CorrelateToSingle("get related category", categoryStream, (l, r) => new { Row = l, Category = r })
    .CorrelateToSingle("get related author", authorStream, (l, r) => new { l.Row, l.Category, Author = r })
    .Select("create post instance", i => string.IsNullOrWhiteSpace(i.Row.Post)
      ? new LinkPost
      {
        AuthorId = i.Author.Id,
        CategoryId = i.Category.Id,
        DateTime = i.Row.TimeSpan,
        Title = i.Row.Title,
        Url = new Uri(i.Row.Link)
      } as Post
      : new TextPost
      {
        AuthorId = i.Author.Id,
        CategoryId = i.Category.Id,
        DateTime = i.Row.TimeSpan,
        Title = i.Row.Title,
        Text = i.Row.Post
      })
    .EfCoreSaveCorrelated("save or update posts", o => o
      .SeekOn(i => new { i.AuthorId, i.DateTime }));
}
            `,
    description: (
      <>
        Dispatch rows from a flat file into several tables to normalize data thanks to the correlation mechanism.
      </>
    ),
  }
];
interface IExample {
  title: string;
  sourceCode: string;
  description: JSX.Element;
}

function Example({ sourceCode, title, description }) {
  return (<div className={clsx('col col--10 col--offset-1')}>
    <div className='card margin--md shadow--tl'>
      <div className="card__header">
        <h3>{title} 🎶</h3>
      </div>
      <div className="card__body">
        <p>{description}</p>
        <WithLineNumbers sourceCode={sourceCode.trim()} />
      </div>
    </div>
  </div>
  );
}

export default function HomepageExamples() {
  if (!features?.length) return null;
  return (
    <section className={styles.features}>
      <div className="container">
        {/* <h1>Examples</h1> */}
        <div className="row">
          {features.map((props, idx) => (
            <Example key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
