import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './index.module.css';
import HomepageFeatures from '../components/HomepageFeatures';
import HomepageExamples from '../components/HomepageExamples';
// import Highlight, { defaultProps } from "prism-react-renderer";
// require(`prismjs/components/prism-csharp`); // eslint-disable-line
// import theme from "prism-react-renderer/themes/dracula";

const Bot = require('../../static/img/dotnet-bot_kayaking.svg').default;
const EtlNet = require('../../static/img/full-black-logo.svg').default
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
            🏁 Get Started
          </Link>
          <Link
            className="button button--secondary button--lg"
            to="/docs/simpleTutorial/intro">
            ⏱️ Quick Presentation
          </Link>
          <Link
            className="button button--secondary button--lg"
            to="/docs/simpleTutorial/intro">
            ⚡ First Tutorial
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
      description="Description will go into a meta tag in <head />">
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        <HomepageExamples />
      </main>
    </Layout>
  );
}
