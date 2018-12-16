import React from "react";
import createSankey from "../tools/createSankey";

class Sankey extends React.Component {
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
            },
            this.props.data
        );
    }

    componentDidUpdate() {
        this._chart.updateData(this.props.data);
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
        return <div ref={this._setRef.bind(this)} className="full-screen" />;
    }
}
export default Sankey;
