import React from "react";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
// import IconButton from "@material-ui/core/IconButton";
import Button from "@material-ui/core/Button";
import FolderOpenIcon from "@material-ui/icons/FolderOpen";

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
    const { classes, theme } = this.props;

    return (
      <Toolbar>
        <Typography variant="h6" color="inherit" noWrap>
          Etl.Net debugger
        </Typography>
        <div className={classes.grow} />
        <Button color="inherit" onClick={this.props.showSelectProcessDialog} className={classNames(classes.menuButton, classes.button)}>
          <FolderOpenIcon className={classes.leftIcon} />
          Select Process...
        </Button>
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
