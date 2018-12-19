import React from "react";
import PropTypes from "prop-types";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import Button from "@material-ui/core/Button";
// import FolderOpenIcon from "@material-ui/icons/FolderOpen";
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import ProcessParameters from "../forms/ProcessParameters";

const styles = theme => ({
});
class ProcessParametersDialog extends React.Component {
  render() {
    const { classes, theme, processParametersDialog: { show, parameters }, executeProcess, hideProcessParametersDialog } = this.props;
    return (<Dialog
      fullWidth={true}
      maxWidth="sm"
      scroll="paper"
      open={show}
      onClose={hideProcessParametersDialog}
      aria-labelledby="form-dialog-title"
    >
      <DialogTitle id="form-dialog-title">Process parameters</DialogTitle>
      <DialogContent>
        <DialogContentText>
        </DialogContentText>
        <ProcessParameters initialValues={parameters} parameters={parameters} />
      </DialogContent>
      <DialogActions>
        <Button onClick={executeProcess} color="primary">
          Execute
        </Button>
        <Button onClick={hideProcessParametersDialog} color="primary">
          Cancel
        </Button>
      </DialogActions>
    </Dialog>);
  }
}

// ApplicationToolBar.propTypes = {
//   classes: PropTypes.object.isRequired,
//   theme: PropTypes.object.isRequired,
//   onSwitchDrawer: PropTypes.func.isRequired,
// };

export default withStyles(styles, { withTheme: true })(ProcessParametersDialog);
