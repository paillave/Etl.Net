// import { LOCATION_CHANGE } from 'connected-react-router';
// https://codesandbox.io/s/5xk1k05zqx

import { ofType } from 'redux-observable';
// import 'jquery';
import { from } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { Subject, merge } from 'rxjs/index';
import { actionCreators, startEtlTraceType } from '../store/EtlProcessTraces'
// https://github.com/aspnet/SignalR/
import * as signalR from '@aspnet/signalr'

export const receiveEtlTracesEpic = action$ => {
    //let connection = new signalR.HubConnectionBuilder().withUrl("/EtlProcessDebug").build();

    const ws$ = new Subject();
    // connection.on("PushTrace", i => {
    //     ws$.next(i);
    // });
    // connection.start().then(i => console.info("CONNECTED!!!")).catch(i => console.error("NOT CONNECTED!!!"));
    return merge(
        // action$.pipe(ofType(startEtlTraceType), map(action => action))

        action$.pipe(
            ofType(startEtlTraceType),
            switchMap(action => from(connection.invoke("Start")).pipe(map(() => ({ type: "etlStarted", }))))),
        ws$.pipe(map(i => actionCreators.addEtlTrace(i)))
    );
}

    // let ws$ = new WebSocketSubject({ url: "http://localhost:5000/EtlProcessDebug" });
//     return ws$.pipe(map(actionCreators.addEtlTrace));
// };
