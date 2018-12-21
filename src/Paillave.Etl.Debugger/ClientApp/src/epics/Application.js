// import { LOCATION_CHANGE } from 'connected-react-router';
// https://codesandbox.io/s/5xk1k05zqx

import { combineEpics } from 'redux-observable';
import { ofType } from 'redux-observable';
// import 'jquery';
import { map, withLatestFrom, filter, bufferTime, flatMap } from 'rxjs/operators';
import { Subject, of, merge } from 'rxjs/index';
import { actionCreators, selectAssemblyType, loadProcessType, executeProcessType, keepParametersType } from '../store/Application'
// https://github.com/aspnet/SignalR/
import * as signalR from '@aspnet/signalr'
import { fetchData } from '../tools/dataAccess';

const receiveEtlTracesEpic = (action$, state$) => {
    let connection = new signalR.HubConnectionBuilder().withUrl("/application").build();

    const trace$ = new Subject();
    connection.on("PushTrace", i => {
        trace$.next(i);
    });
    connection.start().then(i => console.info("CONNECTED!!!")).catch(i => console.error("NOT CONNECTED!!!"));

    return trace$.pipe(
        bufferTime(200),
        filter(i => i.length > 0),
        map(i => actionCreators.addTraces(i)));
}

const getAssemblyProcesses = (action$, state$) => action$.pipe(
    ofType(selectAssemblyType),
    map(i => ({ queryParams: i.payload })),
    fetchData("Application/GetAssemblyProcesses"),
    map(i => actionCreators.receiveProcessList(i))
);


const executeProcess = (action$, state$) => {
    return action$.pipe(
        ofType(executeProcessType),
        withLatestFrom(state$),
        flatMap(([, state]) => {
            let processDefinitionAction$ = of({
                queryParams: state.app.process,
            }).pipe(
                fetchData("Application/GetEstimatedExecutionPlan"),
                map(i => actionCreators.receiveProcessDefinition(i))
            );
            return merge(processDefinitionAction$,
                processDefinitionAction$.pipe(
                    map(() => ({
                        queryParams: state.app.process,
                        data: state.form.processParameters.values
                    })),
                    fetchData({ path: "Application/ExecuteProcess", method: "POST" }),
                    map(i => actionCreators.executionCompleted()))
            );
        })
    );
}

const keepParameters = (action$, state$) => action$.pipe(
    ofType(executeProcessType),
    withLatestFrom(state$),
    map(([, state]) => actionCreators.keepParameters(state.form.processParameters.values))
);

const getEstimatedExecutionPlan = (action$, state$) => action$.pipe(
    ofType(loadProcessType),
    map(i => ({ queryParams: i.payload.process })),
    fetchData("Application/GetEstimatedExecutionPlan"),
    map(i => actionCreators.receiveProcessDefinition(i))
);

export default combineEpics(
    receiveEtlTracesEpic,
    getAssemblyProcesses,
    getEstimatedExecutionPlan,
    keepParameters,
    executeProcess,
);
