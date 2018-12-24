import React from "react";
import createSankey from "../tools/createSankey";
import { withStyles } from "@material-ui/core/styles";

const styles = theme => ({
    fullScreen: {
        height: 600,
        width: "100%"
    }
});

class Sankey extends React.PureComponent {
    constructor(props) {
        super(props);
        this.state = {};
    }
    componentDidMount() {
        this._chart = createSankey(
            this._rootNode,
            {
                ...this.props.config,
                onNodeClick: this.handleNodeClick.bind(this),
                onLinkClick: this.handleLinkClick.bind(this),
            }, {
                links: this.props.links,
                nodes: this.props.nodes
            }
        );
        // window.addEventListener("resize", this.handleUpdateDimensions);
    }
    // componentWillUnmount() {
    //     window.removeEventListener("resize", this.handleUpdateDimensions);
    // }
    componentDidUpdate(previousProps) {
        if (previousProps.nodes !== this.props.nodes || previousProps.sizeGuid !== this.props.sizeGuid) {
            this._chart = createSankey(
                this._rootNode,
                {
                    ...this.props.config,
                    onNodeClick: this.handleNodeClick.bind(this),
                    onLinkClick: this.handleLinkClick.bind(this),
                }, {
                    links: this.props.links,
                    nodes: this.props.nodes
                }
            );
        }
        else if (previousProps.links !== this.props.links) {
            this._chart.updateLinks(this.props.links);
        }
    }

    componentWillUnmount() {
        this._chart.destroy();
    }

    _setRef(componentNode) {
        this._rootNode = componentNode;
    }

    handleNodeClick(node) {
        if (this.props.onNodeClick)
            this.props.onNodeClick(node);
    }

    handleLinkClick(link) {
        if (this.props.onLinkClick)
            this.props.onLinkClick(link);
    }

    render() {
        const {
            classes,
        } = this.props;

        return <div ref={this._setRef.bind(this)} className={classes.fullScreen} />;
    }
}

export default withStyles(styles, { withTheme: true })(Sankey);
