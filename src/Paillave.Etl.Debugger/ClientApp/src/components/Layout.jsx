import React from "react";
import PropTypes from "prop-types";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import Drawer from "@material-ui/core/Drawer";
import AppBar from "@material-ui/core/AppBar";
import CssBaseline from "@material-ui/core/CssBaseline";
import Typography from "@material-ui/core/Typography";
import IconButton from "@material-ui/core/IconButton";
import ChevronLeftIcon from "@material-ui/icons/ChevronLeft";
import ChevronRightIcon from "@material-ui/icons/ChevronRight";
import ApplicationToolBar from "../containers/ApplicationToolBar";
import TraceDetails from "../containers/TraceDetails";
import Divider from "@material-ui/core/Divider";
import OpenProcessDialog from "../containers/OpenProcessDialog";

const drawerWidth = 400;

const styles = theme => ({
  root: {
    display: "flex"
  },
  appBar: {
    transition: theme.transitions.create(["margin", "width"], {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen
    })
  },
  appBarShift: {
    width: `calc(100% - ${drawerWidth}px)`,
    transition: theme.transitions.create(["margin", "width"], {
      easing: theme.transitions.easing.easeOut,
      duration: theme.transitions.duration.enteringScreen
    }),
    marginRight: drawerWidth
  },
  drawer: {
    width: drawerWidth,
    flexShrink: 0
  },
  drawerPaper: {
    width: drawerWidth
  },
  drawerHeader: {
    display: "flex",
    alignItems: "center",
    padding: "0 8px",
    ...theme.mixins.toolbar,
    justifyContent: "flex-start"
  },
  content: {
    flexGrow: 1,
    padding: theme.spacing.unit * 3,
    transition: theme.transitions.create("margin", {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen
    }),
    marginRight: -drawerWidth
  },
  contentShift: {
    transition: theme.transitions.create("margin", {
      easing: theme.transitions.easing.easeOut,
      duration: theme.transitions.duration.enteringScreen
    }),
    marginRight: 0
  }
});

class PersistentDrawerRight extends React.Component {
  render() {
    const { classes, theme } = this.props;
    let { traceDetails:{show} } = this.props;

    return (
      <div className={classes.root}>
        <CssBaseline />
        <OpenProcessDialog/>
        <AppBar position="fixed" className={classNames(classes.appBar, { [classes.appBarShift]: show })}>
          <ApplicationToolBar />
        </AppBar>
        <main className={classNames(classes.content, { [classes.contentShift]: show })}        >
          <div className={classes.drawerHeader} />
            {this.props.children}
        </main>
        <Drawer
          className={classes.drawer}
          variant="persistent"
          anchor="right"
          open={show}
          classes={{ paper: classes.drawerPaper }}        >
          <div className={classes.drawerHeader}>
            <IconButton onClick={this.props.hideTraceDetails}>
              {theme.direction === "rtl" ? (<ChevronLeftIcon />) : (<ChevronRightIcon />)}
            </IconButton>
            <Typography variant="h6">
              Trace details
            </Typography>
          </div>
          <Divider />
          <TraceDetails />
        </Drawer>
      </div>
    );
  }
}

PersistentDrawerRight.propTypes = {
  classes: PropTypes.object.isRequired,
  theme: PropTypes.object.isRequired
};

export default withStyles(styles, { withTheme: true })(PersistentDrawerRight);
