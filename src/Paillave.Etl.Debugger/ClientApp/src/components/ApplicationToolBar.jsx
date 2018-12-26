import React from "react";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
// import IconButton from "@material-ui/core/IconButton";
import Button from "@material-ui/core/Button";
import FolderOpenIcon from "@material-ui/icons/FolderOpen";
import PlayArrowIcon from "@material-ui/icons/PlayArrow";
import IconButton from '@material-ui/core/IconButton';
import Tooltip from '@material-ui/core/Tooltip';

const styles = theme => ({
  button: {
    margin: theme.spacing.unit,
  },
  leftIcon: {
    marginRight: theme.spacing.unit,
  },
  grow: {
    flexGrow: 1,
  },
  menuButton: {
    marginLeft: 12,
    marginRight: 20
  },
});

class ApplicationToolBar extends React.Component {
  render() {
    const { classes, theme, showSelectProcessDialog, showProcessParametersDialog, process } = this.props;

    return (
      <Toolbar>
        <Typography variant="h6" color="inherit" noWrap>
          Etl.Net debugger
        </Typography>
        <div className={classes.grow} />
        <Tooltip title="Open an assembly and select the process to execute">
          <Button color="inherit" onClick={showSelectProcessDialog} className={classNames(classes.menuButton, classes.button)}>
            <FolderOpenIcon className={classes.leftIcon} />
            Select Process...
        </Button>
        </Tooltip>
        {process &&
          <Tooltip title="Execute the selected process by providing its parameter values">
            <IconButton
              onClick={showProcessParametersDialog}
              color="inherit">
              <PlayArrowIcon />
            </IconButton>
          </Tooltip>}
      </Toolbar>
    );
  }
}

// ApplicationToolBar.propTypes = {
//   classes: PropTypes.object.isRequired,
//   theme: PropTypes.object.isRequired,
//   onSwitchDrawer: PropTypes.func.isRequired,
// };

export default withStyles(styles, { withTheme: true })(ApplicationToolBar);
