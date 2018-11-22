import produce from 'immer';
const requestWeatherForecastsType = 'REQUEST_WEATHER_FORECASTS';
const receiveWeatherForecastsType = 'RECEIVE_WEATHER_FORECASTS';
const initialState = { forecasts: [], isLoading: false };

export const actionCreators = {
  requestWeatherForecasts: startDateIndex => async (dispatch, getState) => {
    if (startDateIndex === getState().weatherForecasts.startDateIndex) {
      // Don't issue a duplicate request (we already have or are loading the requested data)
      return;
    }

    dispatch({ type: requestWeatherForecastsType, startDateIndex });

    const url = `api/SampleData/WeatherForecasts?startDateIndex=${startDateIndex}`;
    const response = await fetch(url);
    const forecasts = await response.json();

    dispatch({ type: receiveWeatherForecastsType, startDateIndex, forecasts });
  }
};

export const reducer = (state, action) => produce(state || initialState, draft => {
  switch (action.type) {
    case requestWeatherForecastsType:
      draft.startDateIndex = action.startDateIndex;
      draft.isLoading = true;
      break;
    case receiveWeatherForecastsType:
      draft.startDateIndex = action.startDateIndex;
      draft.forecasts = action.forecasts;
      draft.isLoading = false;
      break;
  }
});
