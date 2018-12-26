import React from "react";
import PropTypes from "prop-types";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import Sankey from './Sankey';
import RowTraceGrid from '../containers/RowTraceGrid';
import NodeTracesHeaders from "../containers/NodeTracesHeaders";
import Drawer from "@material-ui/core/Drawer";
import Typography from "@material-ui/core/Typography";
import IconButton from "@material-ui/core/IconButton";
import ChevronLeftIcon from "@material-ui/icons/ChevronLeft";
import ChevronRightIcon from "@material-ui/icons/ChevronRight";
import TraceDetails from "../containers/TraceDetails";
import Divider from "@material-ui/core/Divider";

const drawerWidth = 600;

const styles = theme => ({
  root: {
    display: "flex",
    overflow: "hidden"
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
    width: drawerWidth - theme.spacing.unit * 3,
    position: "unset",
    marginLeft: theme.spacing.unit * 3
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
    // padding: theme.spacing.unit * 3,
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


class Application extends React.Component {

  handleNodeClick(node) {
    this.props.selectJobNode(node);
    //console.log(node);
  }

  handleLinkClick(link) {
    console.log(link);
  }

  render() {
    const {
      classes,
      theme,
      processDefinition: {
        streamToNodeLinks: links,
        nodes
      },
      traceDetails: { show: showDrawer }
    } = this.props;

    var config = {
      transitionDuration: 200,
      getNodeKey: e => e.nodeName,
      getNodeName: e => `${e.nodeName}:${e.typeName}`,
      getLinkSourceKey: e => e.sourceNodeName,
      getLinkTargetKey: e => e.targetNodeName,
      getLinkValue: e => {
        if (typeof e.value === "undefined") return 1;
        else return e.value;
      },
      margin: { top: 10, left: 10, right: 10, bottom: 10 },
      nodes: {
        draggableX: true,
        draggableY: true
      },
      links: {
        unit: "row(s)"
      },
      tooltip: {
        infoDiv: true,
        labelSource: "Input:",
        labelTarget: "Output:"
      }
    };

    return (<React.Fragment>
      <Sankey config={config} nodes={nodes} links={links} onNodeClick={this.handleNodeClick.bind(this)} onLinkClick={this.handleLinkClick.bind(this)} sizeGuid={this.props.sizeGuid} />
      <NodeTracesHeaders />
      <div className={classes.root}>
        <div className={classNames(classes.content, { [classes.contentShift]: showDrawer })}>

          <RowTraceGrid />
        </div>
        <Drawer
          className={classes.drawer}
          variant="persistent"
          anchor="right"
          open={showDrawer}
          classes={{ paper: classes.drawerPaper }}>
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
    </React.Fragment>);
  }
}

// ApplicationToolBar.propTypes = {
//   classes: PropTypes.object.isRequired,
//   theme: PropTypes.object.isRequired,
//   onSwitchDrawer: PropTypes.func.isRequired,
// };

export default withStyles(styles, { withTheme: true })(Application);
