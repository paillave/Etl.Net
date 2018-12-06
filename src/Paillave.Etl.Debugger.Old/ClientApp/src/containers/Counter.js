import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Counter';
import CounterComponent from '../components/Counter';


export default connect(
  state => state.counter,
  dispatch => bindActionCreators(actionCreators, dispatch)
)(CounterComponent);
