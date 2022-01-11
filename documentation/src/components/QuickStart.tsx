import React from 'react';
// import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';
import Highlight, { defaultProps } from "prism-react-renderer";
import "prismjs"; // eslint-disable-line
require(`prismjs/components/prism-csharp`); // eslint-disable-line
import theme from "prism-react-renderer/themes/dracula";
// https://emojipedia.org/


export default function QuickStart() {
  return <section className={styles.features}>
    <div className="container">
      <div className="row">
        <div className="col col--10 col--offset-1">
          <div className='card margin--md shadow--tl'>
            <div className="card__header">
              <h3>Very Quick Start ⚡</h3>
            </div>
            <div className="card__body">
              <p>Create the project</p>
              <WithLineNumbers sourceCode={`dotnet new console -o MyFirstEtl
cd MyFirstEtl
dotnet add package Paillave.EtlNet.Core
dotnet add package Paillave.EtlNet.FileSystem
dotnet add package Paillave.EtlNet.Zip
dotnet add package Paillave.EtlNet.TextFile
dotnet add package Paillave.EtlNet.SqlServer`} />
              <p>Create and call the ETL</p>
              <WithLineNumbers sourceCode={`using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl.TextFile;
using Paillave.Etl.SqlServer;
using System.Data.SqlClient;
using System.Linq;

namespace SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            using (var cnx = new SqlConnection(args[1]))
            {
                cnx.Open();
                var executionOptions = new ExecutionOptions<string> { Resolver = new SimpleDependencyResolver().Register(cnx) };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
                if (res.Failed)
                    Console.Write($"{res.ErrorTraceEvent.NodeName}({res.ErrorTraceEvent.NodeTypeName}):{res.ErrorTraceEvent.Content.Message}");
            }
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
                .SqlServerSave("upsert using Email as key and ignore the Id", o => o
                    .ToTable("dbo.Person")
                    .SeekOn(p => p.Email)
                    .DoNotSave(p => p.Id))
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
    }
}`} />
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
}

const WithLineNumbers = ({ sourceCode }: { sourceCode: string }) => (
  <Highlight
    {...defaultProps}
    theme={theme}
    code={sourceCode}
    // @ts-ignore
    language="csharp"
  >
    {({ className, style, tokens, getLineProps, getTokenProps }) => (
      <pre className={className + " language-csharp frontpage"} style={style}>
        {tokens.map((line, i) => (
          <div key={i} {...getLineProps({ line, key: i })}>
            <span>
              {line.map((token, key) => (
                <span key={key} {...getTokenProps({ token, key })} />
              ))}
            </span>
          </div>
        ))}
      </pre>
    )}
  </Highlight>
); // eslint-disable-line
