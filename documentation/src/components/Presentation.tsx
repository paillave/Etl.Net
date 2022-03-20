import React from 'react';
// import clsx from 'clsx';
import styles from './Presentation.module.css';

const PresentationImage = require('../../static/img/presentation-etlnet.svg').default;

export default function Presentation() {
  return (
    <section className={styles.features}>
      <PresentationImage className={styles.featureSvg} />
      {/* <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div> */}
      <div className={styles.paragraphDescription}>
        <h1>Data processing</h1>
        <p>ETL.NET is a framework for .NET to <b>implement with no effort fast and easy to maintain data processes</b>. All the tooling for <b>normalization</b>, <b>upsert</b>, <b>lookup</b> or <b>join</b> dramatically reduces the effort for any import and transformation purpose. Everything to handle <b>tracing</b>, <b>error tracking</b> is done automatically for the developer.</p>
        <div style={{ display: "flex", justifyContent: "space-between" }}>
          <p>Read or write <b>any file type</b> and any data source
            <ul>
              <li>Native SQL server</li>
              <li>Entity Framework</li>
              <li>CSV</li>
              <li>Excel</li>
              <li>Bloomberg response files</li>
              <li>Searchable PDF</li>
              <li>XML</li>
              <li>Anything .NET can read or write whatsoever</li>
            </ul>
          </p>
          <p>Read or write files on <b>any source</b>
            <ul>
              <li>File system</li>
              <li>FTP</li>
              <li>SFTP</li>
              <li>FTPS</li>
              <li>Dropbox</li>
              <li>eMail and MailBox</li>
              <li>zip archives</li>
              <li>Anything .NET can access whatsoever</li>
            </ul>
          </p>
        </div>
      </div>
    </section>
  );
}
