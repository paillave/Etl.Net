import React from "react";
import PropTypes from "prop-types";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import AppBar from "@material-ui/core/AppBar";
import CssBaseline from "@material-ui/core/CssBaseline";
import ApplicationToolBar from "../containers/ApplicationToolBar";
import OpenProcessDialog from "../containers/OpenProcessDialog";
import ProcessParametersDialog from "../containers/ProcessParametersDialog";

const drawerWidth = 400;

const styles = theme => ({
  root: {
  },
  drawerPaper: {
    width: drawerWidth
  },
  drawerHeader: {
    ...theme.mixins.toolbar,
  },
  content: {
    padding: theme.spacing.unit * 3,
  },
});

class PersistentDrawerRight extends React.Component {
  render() {
    const { classes, theme } = this.props;
    let { traceDetails: { show } } = this.props;

    return (
      <div className={classes.root}>
        <CssBaseline />
        <OpenProcessDialog />
        <ProcessParametersDialog />
        <AppBar position="fixed">
          <ApplicationToolBar />
        </AppBar>
        <main className={classes.content}>
          <div className={classes.drawerHeader} />
          {this.props.children}
        </main>
      </div>
    );
  }
}

PersistentDrawerRight.propTypes = {
  classes: PropTypes.object.isRequired,
  theme: PropTypes.object.isRequired
};

export default withStyles(styles, { withTheme: true })(PersistentDrawerRight);
