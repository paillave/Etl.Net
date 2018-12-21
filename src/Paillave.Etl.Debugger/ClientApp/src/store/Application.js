import produce from 'immer';
import { convertToDate } from '../tools/dataAccess';
export const switchSelectProcessDialogType = 'SWITCH_SELECT_PROCESS_DIALOG';
export const selectAssemblyType = 'SELECT_ASSEMBLY';
export const receiveProcessListType = 'RECEIVE_PROCESS_LIST';
export const loadProcessType = 'LOAD_PROCESS';
export const receiveProcessDefinitionType = 'RECEIVE_PROCESS_DEFINITION';
export const addTracesType = 'ADD_TRACES';
export const hideTraceDetailsType = 'HIDE_TRACE_DETAILS';
export const showTraceDetailsType = 'SHOW_TRACE_DETAILS';
export const switchProcessParametersDialogType = 'SWITCH_PROCESS_PARAMETERS_DIALOG';
export const executeProcessType = 'EXECUTE_PROCESS';
export const keepParametersType = 'KEEP_PARAMETERS';
export const executionCompletedType = 'EXECUTION_COMPLETED';
export const selectJobNodeType = 'SELECT_JOB_NODE';


const initialState = {
  processSelectionDialog: {
    show: false,
    processes: [],
    assemblyPath: undefined,
    loadingProcesses: false,
  },
  processParametersDialog: {
    show: false,
    parameters: {}
  },
  traceDetails: {
    show: false,
    selectedTrace: undefined,
  },
  loadingProcessDefinition: false,
  traces: {}, //{[nodename:string]:{}}
  process: undefined,
  processDefinition: {
    streamToNodeLinks: [],
    nodes: []
  },
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
  addTraces: (traces) => ({ type: addTracesType, payload: { traces } }),
  hideTraceDetails: () => ({ type: hideTraceDetailsType }),
  showTraceDetails: (trace) => ({ type: showTraceDetailsType, payload: { trace } }),
  receiveProcessDefinition: (processDefinition) => ({ type: receiveProcessDefinitionType, payload: { processDefinition } }),
  showProcessParametersDialog: () => ({ type: switchProcessParametersDialogType, payload: { show: true } }),
  hideProcessParametersDialog: () => ({ type: switchProcessParametersDialogType, payload: { show: false } }),
  executeProcess: () => ({ type: executeProcessType }),
  executionCompleted: () => ({ type: executionCompletedType }),
  keepParameters: (parameters) => ({ type: keepParametersType, payload: { parameters } }),
  selectJobNode: (selectedNode) => ({ type: selectJobNodeType, payload: { selectedNode } }),
};

export const reducer = (state, action) => produce(state || initialState, draft => {
  switch (action.type) {
    case selectJobNodeType:
      draft.selectedNode = action.payload.selectedNode;
      break;
    case keepParametersType:
      draft.processParametersDialog.show = false;
      draft.traces = {};
      draft.processParametersDialog.parameters = action.payload.parameters;
      break;
    case switchSelectProcessDialogType:
      draft.processSelectionDialog.show = action.payload.show;
      break;
    case switchProcessParametersDialogType:
      draft.processParametersDialog.show = action.payload.show;
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
      let parameters = {};
      action.payload.process.parameters.forEach(key => parameters[key] = null);
      draft.processParametersDialog.parameters = parameters;
      draft.process = action.payload.process;
      draft.processSelectionDialog.show = false;
      draft.loadingProcessDefinition = true;
      break;
    case addTracesType:
      action.payload.traces.forEach(trace => {
        convertToDate(trace);
        if (!draft.traces[trace.nodeName])
          draft.traces[trace.nodeName] = [trace];
        else
          draft.traces[trace.nodeName].unshift(trace);
        let counter = draft.traces[trace.nodeName].length;
        if (trace.content.type === "RowProcessStreamTraceContent") {
          draft.processDefinition.streamToNodeLinks.filter(i => i.sourceNodeName === trace.nodeName).forEach(i => i.value = (i.value || 0) + 1);
        }
      });
      break;
    case hideTraceDetailsType:
      draft.traceDetails.show = false;
      break;
    case showTraceDetailsType:
      draft.traceDetails.show = true;
      draft.traceDetails.selectedTrace = action.payload.trace;
      break;
    case receiveProcessDefinitionType:
      draft.loadingProcessDefinition = false;
      draft.processDefinition = action.payload.processDefinition;
      break;
    default:
      break;
  }
});
