// import { LOCATION_CHANGE } from 'connected-react-router';
// https://codesandbox.io/s/5xk1k05zqx

// import { ofType } from 'redux-observable';
// import 'jquery';
import { map } from 'rxjs/operators';
import { Subject } from 'rxjs/index';
import { actionCreators } from '../store/EtlProcessTraces'
// https://github.com/aspnet/SignalR/
import * as signalR from '@aspnet/signalr'

export const receiveEtlTracesEpic = action$ => {
    let connection = new signalR.HubConnectionBuilder()
    .withUrl("/EtlProcessDebug")
    .build();

    const ws$ = new Subject();
    connection.on("PushTrace", ws$.next);
    connection.start()
    .then(()=>console.log('Now connected'));
    
    // let ws$ = new WebSocketSubject({ url: "http://localhost:5000/EtlProcessDebug" });
    return ws$.pipe(map(actionCreators.addEtlTrace));
};