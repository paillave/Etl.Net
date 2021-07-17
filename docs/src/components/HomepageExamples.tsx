import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';
import Highlight, { defaultProps } from "prism-react-renderer";
require(`prismjs/components/prism-csharp`); // eslint-disable-line
import theme from "prism-react-renderer/themes/dracula";

const FeatureList = [
  {
    title: 'Unzip it, read it, save it, report it',
    sourceCode: `private static void DefineProcess(ISingleStream<string> contextStream)
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
          Id = i.ToNumberColumn<int>("new or existing Id")
      }).IsColumnSeparated(','))
    .WriteToFile("save log file", i => i.Name);
}`,
    description: (
      <>
        Read all zip files from a folder, unzip csv files that are inside, parse them, exclude duplicates, upsert them, and report new or pre existing id corresponding to the email.
      </>
    ),
  }
];

function Example({ sourceCode, title, description }) {
  return (<div className={clsx('col col--10 col--offset-1')}>
    <div className='card margin--md'>
      <div className="card__header">
        <h3>{title}</h3>
      </div>
      <div className="card__body">
        <p>{description}</p>
        <WithLineNumbers sourceCode={sourceCode} />
      </div>
    </div>
  </div>
  );
}

export default function HomepageExamples() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Example key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}

const WithLineNumbers = ({ sourceCode }: { sourceCode: string }) => (
  <Highlight
    {...defaultProps}
    theme={theme}
    code={sourceCode}
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
