import * as d3 from "d3";
import * as d3Sankey from "d3-sankey";
import d3Tip from "d3-tip";
import "./sankey.css";

function createSankey(containerNode, configSankey, dataSankey) {
    var _safeValueToLink = v => Math.max(v, Number.MIN_VALUE);

    var _dataSankey = {
        nodes: [],
        links: []
    };

    let nodesKeyToIdDictionary = {};
    let nodesIdxToDataDictionary = {};
    let linksIdToDataDictionary = {};

    _dataSankey.nodes = dataSankey.nodes.map((d, idx) => {
        nodesKeyToIdDictionary[configSankey.getNodeKey(d)] = idx;
        nodesIdxToDataDictionary[idx] = d;
        return {
            name: (configSankey.getNodeName || configSankey.getNodeKey)(d),
            id: nodesKeyToIdDictionary[configSankey.getNodeKey(d)]
        };
    });
    _dataSankey.links = dataSankey.links.map(l => {
        let sourceId = nodesKeyToIdDictionary[configSankey.getLinkSourceKey(l)];
        let targetId = nodesKeyToIdDictionary[configSankey.getLinkTargetKey(l)];
        let id = `${sourceId} -> ${targetId}`;
        linksIdToDataDictionary[id] = l;
        return {
            source: sourceId,
            target: targetId,
            id,
            value: _safeValueToLink(configSankey.getLinkValue(l))
        };
    });

    //removing old svg and tips
    d3.select(".d3-tip-nodes").remove();
    d3.select(".d3-tip").remove();

    var container = d3.select(containerNode);
    container.select("svg").remove();

    function _getDimensions(containerParam, margin) {
        var bbox = containerParam.node().getBoundingClientRect();
        return {
            width: bbox.width - margin.left - margin.right,
            height: bbox.height - margin.top - margin.bottom
        };
    }
    var dimensions = _getDimensions(container, configSankey.margin);

    var svg_base = container
        .append("svg")
        .attr("width", dimensions.width + configSankey.margin.left + configSankey.margin.right)
        .attr("height", dimensions.height + configSankey.margin.top + configSankey.margin.bottom)
        .classed("sk-svg", true);
    var svg = svg_base
        .append("g")
        .attr("transform", `translate(${configSankey.margin.left}, ${configSankey.margin.top})`);

    var sankey = d3Sankey
        .sankey()
        .nodeWidth(15)
        .nodePadding(10)
        .extent([[0, 0], [dimensions.width, dimensions.height]]);

    var path = d3Sankey.sankeyLinkHorizontal();

    //options defaults for drag nodes
    var _nodes_draggableX = false;
    var _nodes_draggableY = true;

    if (configSankey.nodes.draggableX)
        _nodes_draggableX = configSankey.nodes.draggableX;
    if (configSankey.nodes.draggableY)
        _nodes_draggableY = configSankey.nodes.draggableY;

    //Tooltip function:
    //D3 sankey diagram with view options (Austin Czarnecki’s Block cc6371af0b726e61b9ab)
    //https://bl.ocks.org/austinczarnecki/cc6371af0b726e61b9ab
    var linkTooltipOffset = 65,
        nodeTooltipOffset = 130;
    var tipLinks = d3Tip()
        .attr("class", "d3-tip d3-tip-links")
        .offset([-10, 0]);
    var tipNodes = d3Tip()
        .attr("class", "d3-tip d3-tip-nodes")
        .offset([-10, 0]);

    function _formatValueTooltip(val) {
        if (configSankey.links.formatValue)
            return configSankey.links.formatValue(val);
        else return val + " " + configSankey.links.unit;
    }
    function createNodeHtmlTip(d) {
        var title, candidate;
        if (_dataSankey.links.indexOf(d.source.name) > -1) {
            candidate = d.source.name;
            title = d.target.name;
        } else {
            candidate = d.target.name;
            title = d.source.name;
        }
        var html = `<div class="table-wrapper">
            <center><h1>${title}</h1></center>
            <table>
                <tr>
                    <td></td><td></td><tr><td></td><td></td></tr><tr><td><h2>${configSankey.tooltip.labelTarget}</h2></td><td></td>
                </tr>
                <tr>
                    <td class="col-left">${candidate}</td><td align="right">${_formatValueTooltip(d.value)}</td>
                </tr>
            </table>
        </div>`;
        return html;
    }
    tipLinks.html(createNodeHtmlTip);
    var topContentSVG = d3
        .select(".sk-svg")
        .node()
        .getBoundingClientRect().top;
    tipLinks.move = function (event) {
        tipLinks.boxInfo = d3
            .select(".d3-tip-links")
            .node()
            .getBoundingClientRect();
        tipLinks
            .style("top", function () {
                if (d3.event.pageY - topContentSVG - tipLinks.boxInfo.height - 20 >= 0)
                    return d3.event.pageY - tipLinks.boxInfo.height - 20 + "px";
                else return d3.event.pageY + 20 + "px";
            })
            .style("left", function () {
                var left = Math.max(d3.event.pageX - linkTooltipOffset, 10);
                left = Math.min(
                    left,
                    window.innerWidth - d3
                        .select(".d3-tip")
                        .node()
                        .getBoundingClientRect().width - 20
                );
                return left + "px";
            });
    };

    function createLinkHtmlTip(d) {
        var nodeName = d.name;
        var linksFrom = d.targetLinks; //invert for reference
        var linksTo = d.sourceLinks;
        var html;
        var i;

        html = `<div class="table-wrapper"><center><h1>${nodeName}</h1></center><table>`;

        if ((linksFrom.length > 0) & (linksTo.length > 0)) {
            html += `<tr><td><h2>${configSankey.tooltip.labelSource}</h2></td><td></td></tr>`;
        }
        for (i = 0; i < linksFrom.length; ++i) {
            html += `<tr><td class="col-left">${linksFrom[i].source.name}</td><td align="right">${_formatValueTooltip(linksFrom[i].value)}</td></tr>`;
        }
        if ((linksFrom.length > 0) & (linksTo.length > 0)) {
            html += `<tr><td></td><td></td><tr><td></td><td></td></tr><tr><td><h2>${configSankey.tooltip.labelTarget}</h2></td><td></td></tr>`;
        }
        for (i = 0; i < linksTo.length; ++i) {
            html += `<tr><td class="col-left">${linksTo[i].target.name}</td><td align="right">${_formatValueTooltip(linksTo[i].value)}</td></tr>`;
        }
        html += "</table></div>";
        return html;
    }
    tipNodes.html(createLinkHtmlTip);

    tipNodes.move = function (event) {
        tipNodes.boxInfo = d3
            .select(".d3-tip-nodes")
            .node()
            .getBoundingClientRect();
        tipNodes
            .style("top", function () {
                if (d3.event.pageY - topContentSVG - tipNodes.boxInfo.height - 20 >= 0)
                    return d3.event.pageY - tipNodes.boxInfo.height - 20 + "px";
                else return d3.event.pageY + 20 + "px";
            })
            .style("left", function () {
                var left = Math.max(d3.event.pageX - nodeTooltipOffset, 10);
                left = Math.min(
                    left,
                    window.innerWidth -
                    d3
                        .select(".d3-tip")
                        .node()
                        .getBoundingClientRect().width - 20
                );
                return left + "px";
            });
    };

    svg.call(tipLinks);
    svg.call(tipNodes);
    var _stopTooltips = function () {
        if (tipLinks) tipLinks.hide();
        if (tipNodes) tipNodes.hide();
    };

    //Load data
    sankey(_dataSankey);

    var link = svg
        .append("g")
        .selectAll(".sk-link")
        .data(_dataSankey.links, l => l.id)
        .enter()
        .append("path")
        .on("click", onLinkClick)
        .classed("sk-link", true)
        .attr("d", path)
        .style("stroke-width", l => Math.max(1, l.width) + "px")
        .sort((a, b) => b.width - a.width);
    if (configSankey.tooltip.infoDiv)
        link
            .on("mousemove", tipLinks.move)
            .on("mouseover", tipLinks.show)
            .on("mouseout", tipLinks.hide);
    else
        link.append("title").text(d => `${d.source.name}  -> ${d.target.name}\n${_formatValueTooltip(d.value)}`);

    // the function for moving the nodes
    function _dragmove(d) {
        _stopTooltips();
        if (_nodes_draggableX && d3.event.x < dimensions.width) {
            d.x0 = Math.max(0, Math.min(dimensions.width - sankey.nodeWidth(), d.x0 + d3.event.dx));
            d.x1 = d.x0 + sankey.nodeWidth();
        }
        if (_nodes_draggableY && d3.event.y < dimensions.height) {
            var heightNode = d.y1 - d.y0;
            d.y0 = Math.max(0, Math.min(dimensions.height - (d.y1 - d.y0), d.y0 + d3.event.dy));
            d.y1 = d.y0 + heightNode;
        }
        d3.select(this).attr("transform", `translate(${d.x0},${d.y0})`);
        sankey.update(_dataSankey);
        link.attr("d", path);
    }

    var node = svg
        .append("g")
        .selectAll(".sk-node")
        .data(_dataSankey.nodes, d => d.name)
        .enter()
        .append("g")
        .classed("sk-node", true)
        // .attr("class", "sk-node")
        .attr("transform", d => `translate(${d.x0},${d.y0})`);
    if (configSankey.tooltip.infoDiv)
        node
            .on("mousemove", tipNodes.move)
            .on("mouseover", tipNodes.show)
            .on("mouseout", tipNodes.hide);
    else
        node.append("title").text(d => `${d.name}\n${_formatValueTooltip(d.value)}`);
    //Drag nodes
    if (_nodes_draggableX || _nodes_draggableY)
        node.call(
            d3
                .drag()
                .subject(d => d)
                .on("start", function (d) {
                    onNodeClick(d);
                    d3.event.sourceEvent.stopPropagation();
                    this.parentNode.appendChild(this);
                })
                .on("drag", _dragmove)
        );
    else
        node
            .on("click", onNodeClick)

    function onNodeClick(d) {
        if (configSankey.onNodeClick)
            configSankey.onNodeClick(nodesIdxToDataDictionary[d.id]);
    }

    function onLinkClick(d) {
        if (configSankey.onLinkClick)
            configSankey.onLinkClick(linksIdToDataDictionary[d.id]);
    }

    node
        .append("rect")
        .attr("height", d => d.y1 - d.y0)
        .attr("width", sankey.nodeWidth());

    node
        .append("text")
        .attr("x", -6)
        .attr("y", d => (d.y1 - d.y0) / 2)
        .attr("dy", ".35em")
        .attr("text-anchor", "end")
        .attr("transform", null)
        .text(d => d.name)
        .filter(d => d.x0 < dimensions.width / 2)
        .attr("x", 6 + sankey.nodeWidth())
        .attr("text-anchor", "start");

    //https://bl.ocks.org/syntagmatic/77c7f7e8802e8824eed473dd065c450b
    var _updateLinksValues = function (dataUpdated) {
        _stopTooltips();
        sankey(dataUpdated);
        sankey.update(dataUpdated);
        function addTransition(selection) {
            if (configSankey.transitionDuration)
                return selection.transition()
                    .duration(configSankey.transitionDuration);
            else return selection;
        }
        addTransition(
            svg
                .selectAll(".sk-link")
                .data(dataUpdated.links, d => d.id)
                .sort((a, b) => b.width - a.width))
            .attr("d", path)
            .style("stroke-width", l => Math.max(1, l.width) + "px");

        addTransition(
            svg
                .selectAll(".sk-node")
                .data(dataUpdated.nodes, d => d.name))
            .attr("transform", d => `translate(${d.x0},${d.y0})`);

        addTransition(svg.selectAll(".sk-node rect"))
            .attr("height", d => d.y1 - d.y0);

        addTransition(svg.selectAll(".sk-node text"))
            .attr("y", d => (d.y1 - d.y0) / 2);
    };

    //Update value of links, for call the function '_updateLinksValues' transition values (old to new)
    //This function only update values from links
    
    function updateLinks(links) {
        links.forEach(link => {
            let sourceId = nodesKeyToIdDictionary[configSankey.getLinkSourceKey(link)];
            let targetId = nodesKeyToIdDictionary[configSankey.getLinkTargetKey(link)];
            let idLinkUpdate = `${sourceId} -> ${targetId}`;
            var linkToUpdate = _dataSankey.links.filter(l => l.id === idLinkUpdate)[0];
            if (linkToUpdate) linkToUpdate.value = _safeValueToLink(configSankey.getLinkValue(link));
        });
        _updateLinksValues(_dataSankey);
    };

    function destroy() {
        d3.select(".d3-tip-nodes").remove();
        d3.select(".d3-tip").remove();

        var container = d3.select(containerNode);
        container.select("svg").remove();
    };

    return { updateLinks, destroy };
}
export default createSankey;