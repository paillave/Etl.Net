import produce from 'immer';
const incrementCountType = 'INCREMENT_COUNT';
const decrementCountType = 'DECREMENT_COUNT';
const initialState = { count: 0 };

export const actionCreators = {
  increment: () => ({ type: incrementCountType }),
  decrement: () => ({ type: decrementCountType })
};

export const reducer = (state, action) => produce(state || initialState, draft => {
  switch (action.type) {
    case incrementCountType:
      draft.count++;
      break;
    case decrementCountType:
      draft.count--;
      break;
    default:
      break;
  }
})
