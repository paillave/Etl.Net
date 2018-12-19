import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import ProcessParametersDialog from '../components/ProcessParametersDialog';

export default connect(
    state => state.app,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(ProcessParametersDialog);
