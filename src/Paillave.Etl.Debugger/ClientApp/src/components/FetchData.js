import React from 'react';
import { Link } from 'react-router-dom';

const ForecastsTable = (props) => (
  <table className='table'>
    <thead>
      <tr>
        <th>Date</th>
        <th>Temp. (C)</th>
        <th>Temp. (F)</th>
        <th>Summary</th>
      </tr>
    </thead>
    <tbody>
      {props.forecasts.map(forecast =>
        <tr key={forecast.dateFormatted}>
          <td>{forecast.dateFormatted}</td>
          <td>{forecast.temperatureC}</td>
          <td>{forecast.temperatureF}</td>
          <td>{forecast.summary}</td>
        </tr>
      )}
    </tbody>
  </table>
);

const Pagination = (props) => (
  <p className='clearfix text-center'>
    <Link className='btn btn-default pull-left' to={`/fetchdata/${(props.startDateIndex || 0) - 5}`}>Previous</Link>
    <Link className='btn btn-default pull-right' to={`/fetchdata/${(props.startDateIndex || 0) + 5}`}>Next</Link>
    {props.isLoading ? <span>Loading...</span> : []}
  </p>
);

export default (props) => (
  <div>
    <h1>Weather forecast</h1>
    <p>This component demonstrates fetching data from the server and working with URL parameters.</p>
    <ForecastsTable {...props} />
    <Pagination {...props} />
  </div>
)
// export default class extends Component {
//   render(props) {
//     return (
//       <div>
//         <h1>Weather forecast</h1>
//         <p>This component demonstrates fetching data from the server and working with URL parameters.</p>
//         <ForecastsTable {...props} />
//         <Pagination {...props} />
//       </div>
//     );
//   }
// }
