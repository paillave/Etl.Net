import produce from 'immer';
export const switchSelectProcessDialogType = 'SWITCH_SELECT_PROCESS_DIALOG';
export const selectAssemblyType = 'SELECT_ASSEMBLY';
export const receiveProcessListType = 'RECEIVE_PROCESS_LIST';
export const loadProcessType = 'LOAD_PROCESS';
export const addTraceType = 'ADD_TRACE';
export const hideTraceDetailsType = 'HIDE_TRACE_DETAILS';
export const showTraceDetailsType = 'SHOW_TRACE_DETAILS';

const initialState = {
  processSelectionDialog: {
    show: false,
    processes: [],
    assemblyPath: undefined,
    loadingProcesses: false,
  },
  traceDetails: {
    show: false,
    selectedTrace: undefined,
  },
  loadingProcessDefinition: false,
  traces: [],
  tracesToShow: [],
  process: undefined,
  processDefinition: undefined,
  selectedNode: undefined
};

export const actionCreators = {
  // addEtlTrace: trace => ({ type: addEtlTraceType, payload: trace }),
  // startEtlTrace: () => ({ type: startEtlTraceType }),
  showSelectProcessDialog: () => ({ type: switchSelectProcessDialogType, payload: { show: true } }),
  hideSelectProcessDialog: () => ({ type: switchSelectProcessDialogType, payload: { show: false } }),
  selectAssembly: (assemblyPath) => ({ type: selectAssemblyType, payload: { assemblyPath } }),
  receiveProcessList: (processes) => ({ type: receiveProcessListType, payload: { processes } }),
  loadProcess: (process) => ({ type: loadProcessType, payload: { process } }),
  addTrace: (trace) => ({ type: addTraceType, payload: { trace } }),
  hideTraceDetails: () => ({ type: hideTraceDetailsType }),
  showTraceDetails: (trace) => ({ type: showTraceDetailsType, payload: { trace } })
};

export const reducer = (state, action) => produce(state || initialState, draft => {
  switch (action.type) {
    case switchSelectProcessDialogType:
      draft.processSelectionDialog.show = action.payload.show;
      break;
    case selectAssemblyType:
      draft.processSelectionDialog.assemblyPath = action.payload.assemblyPath;
      draft.processSelectionDialog.loadingProcesses = true;
      break;
    case receiveProcessListType:
      draft.processSelectionDialog.processes = action.payload.processes;
      draft.processSelectionDialog.loadingProcesses = false;
      break;
    case loadProcessType:
      draft.process = action.payload.process;
      draft.processSelectionDialog.show = false;
      draft.loadingProcessDefinition = true;
      break;
    case addTraceType:
      draft.traces.push(action.payload.trace);
      break;
    case hideTraceDetailsType:
      draft.traceDetails.show = false;
      break;
    case showTraceDetailsType:
      draft.traceDetails.show = true;
      draft.traceDetails.selectedTrace = action.payload.trace;
      break;
    default:
      break;
  }
});
