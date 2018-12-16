import React from 'react';
import { Route } from 'react-router';
import Layout from './containers/Layout';
import Appli from './containers/Application';

export default () => (
  <Layout>
    <Route exact path='/' component={Appli} />
  </Layout>
);
    // <Route path='/counter' component={Counter} />
    // <Route path='/etlTraces/:startDateIndex?' component={EtlProcessTraces} />
