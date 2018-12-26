import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import OpenProcessDialog from '../components/OpenProcessDialog';

export default connect(
    state => state.app,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(OpenProcessDialog);
