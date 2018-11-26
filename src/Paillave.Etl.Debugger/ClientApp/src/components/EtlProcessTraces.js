import React from 'react';
// import { Link } from 'react-router-dom';

const ForecastsTable = (props) => (
  <table className='table'>
    <thead>
      <tr>
        <th>Message</th>
      </tr>
    </thead>
    <tbody>
      {props.traces.map((trace, idx) =>
        <tr key={idx}>
          <td>{trace}</td>
        </tr>
      )}
    </tbody>
  </table>
);

export default (props) => (
  <div>
    <h1>Traces</h1>
    <p>Traces from Etl.</p>
    <ForecastsTable {...props} />
  </div>
)
