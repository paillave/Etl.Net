import { applyMiddleware, combineReducers, createStore } from 'redux';
import { combineEpics } from 'redux-observable';
import { composeWithDevTools } from 'redux-devtools-extension';
import { connectRouter, routerMiddleware } from 'connected-react-router'
import * as App from './Application';
import { receiveEtlTracesEpic } from '../epics/Application';
import { createEpicMiddleware } from 'redux-observable';
import logger from 'redux-logger'
import { reducer as formReducer } from 'redux-form';

export default function configureStore(history, initialState) {
  const reducers = {
    app: App.reducer,
  };

  const createRootReducer = (history) => combineReducers({
    router: connectRouter(history),
    ...reducers,
    form: formReducer
  })

  const rootEpic = combineEpics(receiveEtlTracesEpic);

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
