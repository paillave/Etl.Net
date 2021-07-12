import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

const FeatureList = [
  {
    title: 'Powered by & for .NET',
    Svg: require('../../static/img/dotnet-logo.svg').default,
    description: (
      <>
        ETL.NET is fully written in .NET for a multi platform usage and for a straight forward integration in any application. Extend it takes 5mn... literally.
      </>
    ),
  },
  {
    title: 'Easy to implement',
    Svg: require('../../static/img/dotnet-bot_microservices.svg').default,
    description: (
      <>
        ETL.NET works with a similar principle than SSIS with ETL processes to be written in .NET like Linq queries.
      </>
    ),
  },
  {
    title: 'Easy to run',
    Svg: require('../../static/img/dotnet-bot_surfing.svg').default,
    description: (
      <>
        A simple and straight forward ELT.NET runtime for .NET executes ETL processes with no installation required.
      </>
    ),
  },
];

function Feature({Svg, title, description}) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} alt={title} />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
