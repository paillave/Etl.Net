import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import TraceDetails from '../components/TraceDetails';

export default connect(
    state => state.app.traceDetails,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(TraceDetails);
