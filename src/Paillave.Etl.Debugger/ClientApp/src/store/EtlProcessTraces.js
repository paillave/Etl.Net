import produce from 'immer';
export const addEtlTraceType = 'ADD_ETL_TRACE';
export const startEtlTraceType = 'START_ETL_TRACE';
const initialState = { traces: [] };

export const actionCreators = {
  addEtlTrace: trace => ({ type: addEtlTraceType, payload: trace }),
  startEtlTrace: () => ({ type: startEtlTraceType }),
};

export const reducer = (state, action) => produce(state || initialState, draft => {
  switch (action.type) {
    case addEtlTraceType:
      draft.traces.push(action.payload);
      break;
  }
});
