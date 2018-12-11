import React from "react";
import PropTypes from "prop-types";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import Button from "@material-ui/core/Button";
// import FolderOpenIcon from "@material-ui/icons/FolderOpen";
import TextField from '@material-ui/core/TextField';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import { DropzoneArea } from 'material-ui-dropzone';

const styles = theme => ({
});

class OpenProcessDialog extends React.Component {
  handleDropFile(files) {
    this.props.selectAssembly(files.name);
  }
  render() {
    const { classes, theme, processSelectionDialog: { show } } = this.props;

    return (<Dialog
      open={show}
      onClose={this.props.hideSelectProcessDialog}
      aria-labelledby="form-dialog-title"
    >
      <DialogTitle id="form-dialog-title">Select Process</DialogTitle>
      <DialogContent>
        <DialogContentText>
          <DropzoneArea
            acceptedFiles={["application/x-msdownload", "application/octet-stream", "application/x-msdos-program"]}
            filesLimit={1}
            showPreviewsInDropzone={false}
            showPreviews={false}
            showAlerts={false}
            onDrop={this.handleDropFile.bind(this)}
          />
        </DialogContentText>
        <TextField
          autoFocus
          margin="dense"
          id="name"
          label="Email Address"
          type="email"
          fullWidth
        />
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
