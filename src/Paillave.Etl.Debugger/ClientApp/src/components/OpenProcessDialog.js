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
import SelectAssembly from "../forms/SelectAssembly";

const styles = theme => ({
  textField: {
    marginLeft: theme.spacing.unit,
    marginRight: theme.spacing.unit,
    width: 200,
  }
});
class OpenProcessDialog extends React.Component {
  submitAssembly(values) {
    this.props.selectAssembly(values.assemblyPath);
  }
  render() {
    const { classes, theme, processSelectionDialog: { show }, processSelectionDialog: { assemblyPath } } = this.props;
    return (<Dialog
      scroll="paper"
      open={show}
      onClose={this.props.hideSelectProcessDialog}
      aria-labelledby="form-dialog-title"
    >
      <DialogTitle id="form-dialog-title">Select Process</DialogTitle>
      <DialogContent>
        <DialogContentText>
        </DialogContentText>
        <SelectAssembly onSubmit={this.submitAssembly.bind(this)} />
      </DialogContent>
      <DialogActions>
        <Button onClick={this.props.hideSelectProcessDialog} color="primary">
          Cancel
        </Button>
        <Button onClick={this.props.loadProcess} color="primary">
          OK
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

export default withStyles(styles, { withTheme: true })(OpenProcessDialog);
