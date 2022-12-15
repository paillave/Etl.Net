import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

const VideoList = [{ url: "https://www.youtube.com/embed/ivts2qvSats", title: "Data processing with ETL.NET" }];

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          <div className={clsx('col col--3')} />
          {VideoList.map((props, idx) => <div key={idx} className={clsx('col col--6')}>
            <section className={styles.features}>
              <iframe width="560" height="315" src={props.url} title={props.title} frameBorder={0} allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowFullScreen></iframe>
            </section>
          </div>)}
          <div className={clsx('col col--3')} />
        </div>
      </div>
    </section>
  );
}
