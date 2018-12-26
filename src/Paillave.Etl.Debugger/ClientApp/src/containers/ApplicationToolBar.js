import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import ApplicationToolBar from '../components/ApplicationToolBar';

export default connect(
    state => state.app,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(ApplicationToolBar);
