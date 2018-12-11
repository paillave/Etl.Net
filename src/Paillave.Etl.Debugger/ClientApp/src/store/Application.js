import produce from 'immer';
export const switchSelectAssemblyDialogType = 'SWITCH_SELECT_ASSEMBLY_DIALOG';
export const hideSelectAssemblyDialogType = 'HIDE_SELECT_ASSEMBLY_DIALOG';
export const selectAssemblyType = 'SELECT_ASSEMBLY';
export const receiveProcessesListType = 'PROCESSES_LIST';
export const selectProcessType = 'SELECT_PROCESS';
export const addTraceType = 'ADD_TRACE';
export const hideTraceDetailsType = 'HIDE_TRACE_DETAILS';

const initialState = {
  processSelectionDialog: {
    show: false,
    processes: [],
    assemblyPath: undefined,
    loadingProcesses: false,
  },
  traceDetails: {
    show: true,
    selectedTrace: { blabla: "coucou" },
  },
  loadingProcessDefinition: false,
  traces: [],
  tracesToShow: [],
  assemblyPath: undefined,
  process: undefined,
  processDefinition: undefined,
  selectedNode: undefined
};

export const actionCreators = {
  // addEtlTrace: trace => ({ type: addEtlTraceType, payload: trace }),
  // startEtlTrace: () => ({ type: startEtlTraceType }),
  showSelectAssemblyDialog: () => ({ type: switchSelectAssemblyDialogType, payload: { show: true } }),
  hideSelectAssemblyDialog: () => ({ type: switchSelectAssemblyDialogType, payload: { show: false } }),
  selectAssembly: (assemblyPath) => ({ type: selectAssemblyType, payload: { assemblyPath } }),
  receiveProcessesList: (processes) => ({ type: receiveProcessesListType, payload: { processes } }),
  selectProcess: (process) => ({ type: selectProcessType, payload: { process } }),
  addTrace: (trace) => ({ type: addTraceType, payload: { trace } }),
  hideTraceDetails: () => ({ type: hideTraceDetailsType })
};

export const reducer = (state, action) => produce(state || initialState, draft => {
  switch (action.type) {
    case switchSelectAssemblyDialogType:
      draft.processSelectionDialog.show = action.payload.show;
      break;
    case selectAssemblyType:
      draft.processSelectionDialog.assemblyPath = action.payload.assemblyPath;
      draft.processSelectionDialog.loadingProcesses = true;
      break;
    case receiveProcessesListType:
      draft.processSelectionDialog.processes = action.payload.processes;
      draft.processSelectionDialog.loadingProcesses = false;
      break;
    case selectProcessType:
      draft.process = action.payload.process;
      draft.assemblyPath = draft.processSelectionDialog.payload.assemblyPath;
      draft.loadingProcessDefinition = true;
      break;
    case addTraceType:
      draft.traces.push(action.payload.trace);
      break;
    case hideTraceDetailsType:
      draft.traceDetails.show = false;
      break;
    default:
      break;
  }
});
