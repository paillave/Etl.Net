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
import { List, ListItem, ListItemText, Typography } from "@material-ui/core";
import DropzoneFileArea from '../materialuidropzone/DropzoneFileArea';
import isElectron from 'is-electron';

const styles = theme => ({
});
class OpenProcessDialog extends React.Component {
  submitAssembly(values) {
    this.props.selectAssembly(values.assemblyPath);
  }
  handleDroppedFile(file) {
    // console.log(file);
    this.props.selectAssembly(file.path);
  }
  render() {
    const { classes, theme, processSelectionDialog: { show, assemblyPath, processes } } = this.props;
    return (<Dialog
      fullWidth={true}
      maxWidth="md"
      scroll="paper"
      open={show}
      onClose={this.props.hideSelectProcessDialog}
      aria-labelledby="form-dialog-title"
    >
      <DialogTitle id="form-dialog-title">Select Process</DialogTitle>
      <DialogContent>
        <DialogContentText>
        </DialogContentText>
        {(!isElectron()) && <SelectAssembly initialValues={{ assemblyPath }} onSubmit={this.submitAssembly.bind(this)} />}
        {isElectron() && <React.Fragment><DropzoneFileArea
          acceptedFiles={['application/octet-stream', 'application/x-msdownload', 'application/x-msdos-program']}
          onChange={this.handleDroppedFile.bind(this)}>
          <Typography noWrap={true}>
            Drop the assembly here or click to browse
          </Typography>
        </DropzoneFileArea>
          <Typography>
            {assemblyPath}
          </Typography>
        </React.Fragment>}
        <List component="nav">
          {processes.map((i, idx) => <ListItem key={idx} button>
            <ListItemText onClick={this.props.loadProcess.bind(this, i)} primary={`${i.className}.${i.streamTransformationName}`} secondary={i.namespace} />
          </ListItem>)}
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={this.props.hideSelectProcessDialog} color="primary">
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

export default withStyles(styles, { withTheme: true })(OpenProcessDialog);
