import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import ApplicationComponent from '../components/Application';

export default connect(
    state => state.app,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(ApplicationComponent);
