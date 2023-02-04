import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

const features: IFeature[] = [
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
        ETL.NET works with a similar principle than SSIS, with ETL processes to be written in .NET like Linq queries.
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
interface IFeature {
  title: string;
  Svg: any;
  description: JSX.Element;
}

function Feature({ Svg, title, description, size }) {
  return (
    <div className={clsx(`col col--${size}`)}>
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
  if (!features?.length) return null;
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {features.map((props, idx) => (
            <Feature key={idx} size={Math.round(12 / features.length)} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
{/* <section className={styles.features}>
  <iframe width="560" height="315" src="https://www.youtube.com/embed/ivts2qvSats" title="YouTube video player" frameBorder={0} allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowFullScreen></iframe>
</section> */}
