import produce from 'immer';
const addEtlTraceType = 'ADD_ETL_TRACE';
const initialState = { traces: [] };

export const actionCreators = {
  addEtlTrace: trace => ({ type: addEtlTraceType, payload: trace })
};

export const reducer = (state, action) => produce(state || initialState, draft => {
  switch (action.type) {
    case addEtlTraceType:
      draft.traces.push(action.payload);
      break;
  }
});
