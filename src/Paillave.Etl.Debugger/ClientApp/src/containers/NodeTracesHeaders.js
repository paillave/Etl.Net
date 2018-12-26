import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import NodeTracesHeaders from '../components/NodeTracesHeaders';

export default connect(
    state => state.app,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(NodeTracesHeaders);
