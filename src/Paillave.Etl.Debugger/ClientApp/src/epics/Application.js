// import { LOCATION_CHANGE } from 'connected-react-router';
// https://codesandbox.io/s/5xk1k05zqx

import { ofType } from 'redux-observable';
// import 'jquery';
import { map } from 'rxjs/operators';
import { Subject, merge } from 'rxjs/index';
import { actionCreators, selectAssemblyType, loadProcessType } from '../store/Application'
// https://github.com/aspnet/SignalR/
import * as signalR from '@aspnet/signalr'
import { fetchData } from '../tools/dataAccess';

export const receiveEtlTracesEpic = action$ => {
    let connection = new signalR.HubConnectionBuilder().withUrl("/application").build();

    const trace$ = new Subject();
    connection.on("PushTrace", i => {
        trace$.next(i);
    });
    connection.start().then(i => console.info("CONNECTED!!!")).catch(i => console.error("NOT CONNECTED!!!"));
    return merge(
        // action$.pipe(ofType(startEtlTraceType), map(action => action))

        action$.pipe(
            ofType(selectAssemblyType),
            map(i => ({ queryParams: i.payload })),
            fetchData("Application/GetAssemblyProcesses"),
            map(i => actionCreators.receiveProcessList(i))
        ),
        trace$.pipe(map(i => actionCreators.addTrace(i))),
        action$.pipe(
            ofType(loadProcessType),
            map(i => ({ queryParams: i.payload.process })),
            fetchData("Application/GetEstimatedExecutionPlan"),
            map(i => actionCreators.receiveProcessDefinition(i))
        )
    );
}

    // let ws$ = new WebSocketSubject({ url: "http://localhost:5000/EtlProcessDebug" });
//     return ws$.pipe(map(actionCreators.addEtlTrace));
// };

