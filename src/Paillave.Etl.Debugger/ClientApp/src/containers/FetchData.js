import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/WeatherForecasts';
import FetchDataComponent from '../components/FetchData';

export default connect(
  state => state.weatherForecasts,
  dispatch => bindActionCreators(actionCreators, dispatch)
)(FetchDataComponent);
