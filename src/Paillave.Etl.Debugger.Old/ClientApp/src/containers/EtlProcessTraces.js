import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/EtlProcessTraces';
import EtlProcessTracesComponent from '../components/EtlProcessTraces';

export default connect(
  state => state.etlTraces,
  dispatch => bindActionCreators(actionCreators, dispatch)
)(EtlProcessTracesComponent);
