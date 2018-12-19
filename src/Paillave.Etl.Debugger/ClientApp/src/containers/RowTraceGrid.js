import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import RowTraceGrid from '../components/RowTraceGrid';

export default connect(
    state => state.app,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(RowTraceGrid);
