import { applyMiddleware, combineReducers, createStore } from 'redux';
import { combineEpics } from 'redux-observable';
import { composeWithDevTools } from 'redux-devtools-extension';
import { connectRouter, routerMiddleware } from 'connected-react-router'
import * as Counter from './Counter';
import * as WeatherForecasts from './WeatherForecasts';
import { createEpicMiddleware } from 'redux-observable';
import logger from 'redux-logger'

export default function configureStore(history, initialState) {
  const reducers = {
    counter: Counter.reducer,
    weatherForecasts: WeatherForecasts.reducer
  };
  const createRootReducer = (history) => combineReducers({
    router: connectRouter(history),
    ...reducers
  })


  const rootEpic = combineEpics();
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
