import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Application';
import LayoutComponent from '../components/Layout';

export default connect(
    state => state.app,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(LayoutComponent);
