import React from 'react';

export default (props) => (
  <div>
    <h1>Counter</h1>
    <p>This is a simple example of a React component.</p>
    <p>Current count: <strong>{props.count}</strong></p>
    <button onClick={props.increment}>Increment</button>
  </div>
);
