import React from 'react';
// import { Link } from 'react-router-dom';

const ForecastsTable = (props) => (
  <table className='table'>
    <thead>
      <tr>
        <th>DateTime</th>
        <th>Node Name</th>
        <th>Node Type Name</th>
        <th>Level</th>
        <th>Trace type</th>
        <th>Message</th>
      </tr>
    </thead>
    <tbody>
      {props.traces.map((trace, idx) =>
        <tr key={idx}>
          <td>{trace.dateTime}</td>
          <td>{trace.nodeName}</td>
          <td>{trace.nodeTypeName}</td>
          <td>{trace.content.level}</td>
          <td>{trace.content.type}</td>
          <td>{trace.content.message}</td>
        </tr>
      )}
    </tbody>
  </table>
);

export default (props) => (
  <div>
    <h1>Traces</h1>
    <p>Traces from Etl.</p>
    <button onClick={props.startEtlTrace.bind(null)}>Start</button>
    <ForecastsTable {...props} />
  </div>
)
