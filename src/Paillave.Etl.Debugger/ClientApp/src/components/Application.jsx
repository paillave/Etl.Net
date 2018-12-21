import React from "react";
import PropTypes from "prop-types";
import classNames from "classnames";
import { withStyles } from "@material-ui/core/styles";
import Sankey from './Sankey';
import RowTraceGrid from '../containers/RowTraceGrid';
import NodeTracesHeaders from "../containers/NodeTracesHeaders";

const styles = theme => ({
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
      processSelectionDialog: {
        show
      },
      processDefinition: {
        streamToNodeLinks: links,
        nodes
      }
    } = this.props;

    var config = {
      transitionDuration: 200,
      getNodeKey: e => e.name,
      getNodeName: e => e.name,
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
      <Sankey config={config} nodes={nodes} links={links} className={"full-screen"} onNodeClick={this.handleNodeClick.bind(this)} onLinkClick={this.handleLinkClick.bind(this)} />
      <NodeTracesHeaders/>
      <RowTraceGrid />
    </React.Fragment>);
  }
}

// ApplicationToolBar.propTypes = {
//   classes: PropTypes.object.isRequired,
//   theme: PropTypes.object.isRequired,
//   onSwitchDrawer: PropTypes.func.isRequired,
// };

export default withStyles(styles, { withTheme: true })(Application);
