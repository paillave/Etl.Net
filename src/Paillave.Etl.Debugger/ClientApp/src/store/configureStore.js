import { applyMiddleware, combineReducers, createStore } from 'redux';
import { combineEpics } from 'redux-observable';
import { composeWithDevTools } from 'redux-devtools-extension';
import { connectRouter, routerMiddleware } from 'connected-react-router'
import * as Counter from './Counter';
import * as EtlProcessTraces from './EtlProcessTraces';
import * as EtlProcessTracesEpic from '../epics/EtlProcessTraces';
import { createEpicMiddleware } from 'redux-observable';
import logger from 'redux-logger'

export default function configureStore(history, initialState) {
  const reducers = {
    counter: Counter.reducer,
    etlTraces: EtlProcessTraces.reducer
  };

  const createRootReducer = (history) => combineReducers({
    router: connectRouter(history),
    ...reducers
  })

  const rootEpic = combineEpics(EtlProcessTracesEpic.receiveEtlTracesEpic);

  const epicMiddleware = createEpicMiddleware();
  const middleware = [
    routerMiddleware(history),
    epicMiddleware,
    logger
  ];

  const store = createStore(
    createRootReducer(history),
    initialState,
    composeWithDevTools(applyMiddleware(...middleware))
  );

  epicMiddleware.run(rootEpic);

  return store;
}
