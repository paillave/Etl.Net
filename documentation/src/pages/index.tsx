import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './index.module.css';
import HomepageFeatures from '../components/HomepageFeatures';
import HomepageExamples from '../components/HomepageExamples';
import QuickStart from '../components/QuickStart';
// import Highlight, { defaultProps } from "prism-react-renderer";
// require(`prismjs/components/prism-csharp`); // eslint-disable-line
// import theme from "prism-react-renderer/themes/dracula";

const Bot = require('../../static/img/dotnet-bot_kayaking.svg').default;
const EtlNet = require('../../static/img/full-black-logo.svg').default
const streamImage = require('../../static/img/SmallStreams.jpg').url;
function HomepageHeader() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <Bot className={clsx(styles.kayakBot)} />
        <div className={clsx(styles.mainLogoBackground)}>
          <EtlNet />
        </div>
        {/* <h1 className="hero__title">{siteConfig.title}</h1> */}
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            to="/docs/intro">
            ⏱️ Introduction
          </Link>
          <Link
            className="button button--secondary button--lg"
            to="/docs/quickstart/principle">
            🏁 Get Started
          </Link>
          <Link
            className="button button--secondary button--lg"
            to="/docs/tutorials/backbone">
            ⚡ Tutorial
          </Link>
        </div>
      </div>
    </header>
  );
}

export default function Home() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`Home`}
      description="Fully featured ETL int .NET5 for .NET5 working with the same principle than SSIS"
      image={streamImage}
      keywords={["ETL", ".NET", "SSIS", "Import", "Export"]}>
      <HomepageHeader />
      <main>
        <QuickStart />
        <HomepageFeatures />
        <HomepageExamples />
      </main>
    </Layout>
  );
}
