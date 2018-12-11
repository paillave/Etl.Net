// import { LOCATION_CHANGE } from 'connected-react-router';
// https://codesandbox.io/s/5xk1k05zqx

import { ofType } from 'redux-observable';
// import 'jquery';
import { from } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { Subject, merge } from 'rxjs/index';
import { actionCreators } from '../store/Application'
// https://github.com/aspnet/SignalR/
import * as signalR from '@aspnet/signalr'

export const receiveEtlTracesEpic = action$ => {
    let connection = new signalR.HubConnectionBuilder().withUrl("/application").build();

    const ws$ = new Subject();
    // connection.on("PushTrace", i => {
    //     ws$.next(i);
    // });
    connection.start().then(i => console.info("CONNECTED!!!")).catch(i => console.error("NOT CONNECTED!!!"));
    return merge(
        // action$.pipe(ofType(startEtlTraceType), map(action => action))

        // action$.pipe(
        //     ofType(startEtlTraceType),
        //     switchMap(action => from(connection.invoke("Start")).pipe(map(() => ({ type: "etlStarted", }))))),
        ws$.pipe(map(i => actionCreators.addTrace(i)))
    );
}

    // let ws$ = new WebSocketSubject({ url: "http://localhost:5000/EtlProcessDebug" });
//     return ws$.pipe(map(actionCreators.addEtlTrace));
// };

