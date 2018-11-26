import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './containers/Home';
import Counter from './containers/Counter';
import EtlProcessTraces from './containers/EtlProcessTraces';

export default () => (
  <Layout>
    <Route exact path='/' component={Home} />
    <Route path='/counter' component={Counter} />
    <Route path='/etlTraces/:startDateIndex?' component={EtlProcessTraces} />
  </Layout>
);
