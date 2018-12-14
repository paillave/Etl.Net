// https://github.com/FabricioRHS/skd3/blob/master/package.json
import d3 from "d3-sankey";
import "d3-tip";
import "./sankey.css";

export default function (containerId, configSankey, dataSankey) {

    // to prevent NaN value, related https://github.com/d3/d3-sankey/issues/39
    var _safeValueToLink = function (v) { return Math.max(v, Number.MIN_VALUE); }

    var _dataSankey = {
        nodes: [],
        links: []
    };

    //load data
    dataSankey.nodes.map(function (d) {
        _dataSankey.nodes.push({
            name: d.name,
            color: d.color,
            id: d.id
        });
    });
    dataSankey.links.map(function (l) {
        _dataSankey.links.push({
            source: l.source,
            target: l.target,
            id: l.id,
            value: _safeValueToLink(l.value)
        });
    });

    //var _dataSankey = Object.assign({}, dataSankey);

    var _updateLinksId = function (linkData) {
        for (var i = 0; i < linkData.length; i++)
            if (linkData[i].id == undefined)
                linkData[i].id = linkData[i].source + " -> " + linkData[i].target;
    };
    //update links id
    _updateLinksId(_dataSankey.links);

    //removing old svg and tips
    d3.select('.d3-tip-nodes').remove();
    d3.select('.d3-tip').remove();
    d3.select(containerId + ' svg').remove()

    var container = d3.select(containerId);

    function _getDimensions(container, margin) {
        var bbox = container.node().getBoundingClientRect();
        return {
            width: bbox.width - margin.left - margin.right,
            height: bbox.height - margin.top - margin.bottom
        };
    }
    var dimensions = _getDimensions(container, configSankey.margin);

    var svg_base = container.append("svg")
        .attr('width', dimensions.width + configSankey.margin.left + configSankey.margin.right)
        .attr('height', dimensions.height + configSankey.margin.top + configSankey.margin.bottom)
        .attr("class", "sk-svg");
    var svg = svg_base.append("g")
        .attr('transform', "translate(" + configSankey.margin.left + "," + configSankey.margin.top + ")");

    var sankey = d3.sankey()
        .nodeWidth(15)
        .nodePadding(10)
        .extent([
            [0, 0],
            [dimensions.width, dimensions.height]
        ]);

    var path = d3.sankeyLinkHorizontal();

    //Fonts
    var _getFontSize = function (d) {
        return configSankey.nodes.fontSize;
    }; //For default
    var _dynamicFontSize;
    var _updateRangeFontData = function (d) { }; //For default
    if (configSankey.nodes.dynamicSizeFontNode.enabled) {
        _dynamicFontSize = d3.scaleLinear().range(
            [configSankey.nodes.dynamicSizeFontNode.minSize,
            configSankey.nodes.dynamicSizeFontNode.maxSize
            ]);
        _updateRangeFontData = function (nodeData) {
            _dynamicFontSize.domain(d3.extent(nodeData, function (d) {
                return d.value
            }));
        };
        _getFontSize = function (d) {
            return Math.floor(_dynamicFontSize(d.value));
        };
    }

    //options defaults for drag nodes
    var _nodes_draggableX = false;
    var _nodes_draggableY = true;

    if (configSankey.nodes.draggableX != undefined) _nodes_draggableX = configSankey.nodes.draggableX;
    if (configSankey.nodes.draggableY != undefined) _nodes_draggableY = configSankey.nodes.draggableY;

    //Colors
    //set color in nodes, case not exists
    for (var i = 0; i < _dataSankey.nodes.length; i++)
        if (_dataSankey.nodes[i].color == undefined)
            _dataSankey.nodes[i].color = configSankey.nodes.colors(_dataSankey.nodes[i].name.replace(/ .*/, ""));

    //Tooltip function:
    //D3 sankey diagram with view options (Austin Czarnecki’s Block cc6371af0b726e61b9ab)
    //https://bl.ocks.org/austinczarnecki/cc6371af0b726e61b9ab
    var linkTooltipOffset = 65,
        nodeTooltipOffset = 130;
    var tipLinks = d3.tip().attr("class", "d3-tip").offset([-10, 0]);
    var tipNodes = d3.tip().attr("class", "d3-tip d3-tip-nodes").offset([-10, 0]);

    function _formatValueTooltip(val) {
        if (configSankey.links.formatValue)
            return configSankey.links.formatValue(val);
        else
            return val + ' ' + configSankey.links.unit;
    }

    tipLinks.html(function (d) {
        var title, candidate;
        if (_dataSankey.links.indexOf(d.source.name) > -1) {
            candidate = d.source.name;
            title = d.target.name;
        } else {
            candidate = d.target.name;
            title = d.source.name;
        }
        var html = '<div class="table-wrapper">' +
            '<center><h1>' + title + '</h1></center>' +
            '<table>' +
            '<tr>' +
            '<td class="col-left">' + candidate + '</td>' +
            '<td align="right">' + _formatValueTooltip(d.value) + '</td>' +
            '</tr>' +
            '</table>' +
            '</div>';
        return html;
    });
    var topContentSVG = d3.select('.sk-svg').node().getBoundingClientRect().top;
    tipLinks.move = function (event) {
        tipLinks
            .style("top", function () {
                if (d3.event.pageY - topContentSVG - linkTooltipOffset >= 0)
                    return (d3.event.pageY - linkTooltipOffset) + "px";
                else
                    return d3.event.pageY + 20 + "px";
            })
            .style("left", function () {
                var left = (Math.max(d3.event.pageX - linkTooltipOffset, 10));
                left = Math.min(left, window.innerWidth - d3.select('.d3-tip').node().getBoundingClientRect().width - 20)
                return left + "px";
            })
    };

    tipNodes.html(function (d) {
        var nodeName = d.name;
        var linksFrom = d.targetLinks; //invert for reference
        var linksTo = d.sourceLinks;
        var html;

        html = '<div class="table-wrapper">' +
            '<center><h1>' + nodeName + '</h1></center>' +
            '<table>';
        if (linksFrom.length > 0 & linksTo.length > 0) {
            html += '<tr><td><h2>' + configSankey.tooltip.labelSource + '</h2></td><td></td></tr>'
        }
        for (i = 0; i < linksFrom.length; ++i) {
            html += '<tr>' +
                '<td class="col-left">' + linksFrom[i].source.name + '</td>' +
                '<td align="right">' + _formatValueTooltip(linksFrom[i].value) + '</td>' +
                '</tr>';
        }
        if (linksFrom.length > 0 & linksTo.length > 0) {
            html += '<tr><td></td><td></td><tr><td></td><td></td> </tr><tr><td><h2>' + configSankey.tooltip.labelTarget + '</h2></td><td></td></tr>'
        }
        for (i = 0; i < linksTo.length; ++i) {
            html += '<tr>' +
                '<td class="col-left">' + linksTo[i].target.name + '</td>' +
                '<td align="right">' + _formatValueTooltip(linksTo[i].value) + '</td>' +
                '</tr>';
        }
        html += '</table></div>';
        return html;
    });
    tipNodes.move = function (event) {
        tipNodes.boxInfo = d3.select('.d3-tip-nodes').node().getBoundingClientRect();
        tipNodes
            .style("top",
                function () {
                    if ((d3.event.pageY - topContentSVG - tipNodes.boxInfo.height - 20) >= 0)
                        return (d3.event.pageY - tipNodes.boxInfo.height - 20) + "px";
                    else
                        return d3.event.pageY + 20 + "px";
                }
            )
            .style("left", function () {
                var left = (Math.max(d3.event.pageX - nodeTooltipOffset, 10));
                left = Math.min(left, window.innerWidth - d3.select('.d3-tip').node().getBoundingClientRect().width - 20)
                return left + "px";
            })
    };

    svg.call(tipLinks);
    svg.call(tipNodes);
    var _stopTooltips = function () {
        if (tipLinks) tipLinks.hide();
        if (tipNodes) tipNodes.hide();
    };

    //Load data
    sankey(_dataSankey);

    //update range font data
    _updateRangeFontData(_dataSankey.nodes);


    var link = svg.append("g").selectAll(".sk-link")
        .data(_dataSankey.links, function (l) {
            return l.id;
        })
        .enter().append("path")
        .attr("class", "sk-link")
        .attr("d", path)
        .style("stroke", function (l) {
            return l.source.color;
        })
        .style("stroke-width", function (l) {
            return Math.max(1, l.width) + "px";
        })
        .sort(function (a, b) {
            return b.width - a.width;
        });
    if (configSankey.tooltip.infoDiv)
        link.on('mousemove', tipLinks.move).on('mouseover', tipLinks.show).on('mouseout', tipLinks.hide);
    else
        link.append("title").text(function (d) {
            return d.source.name + " -> " + d.target.name + "\n" + _formatValueTooltip(d.value);
        });

    // the function for moving the nodes
    function _dragmove(d) {
        _stopTooltips();
        if (_nodes_draggableX && (d3.event.x < dimensions.width)) {
            d.x0 = Math.max(0, Math.min(dimensions.width - sankey.nodeWidth(), d.x0 + d3.event.dx));
            d.x1 = d.x0 + sankey.nodeWidth();
        }
        if (_nodes_draggableY && (d3.event.y < dimensions.height)) {
            var heightNode = d.y1 - d.y0;
            d.y0 = Math.max(0, Math.min(dimensions.height - (d.y1 - d.y0), d.y0 + d3.event.dy));
            d.y1 = d.y0 + heightNode;
        }
        d3.select(this).attr("transform", "translate(" + d.x0 + "," + d.y0 + ")");
        sankey.update(_dataSankey);
        link.attr("d", path);
    }

    var node = svg.append("g").selectAll(".sk-node")
        .data(_dataSankey.nodes, function (d) {
            return d.name;
        })
        .enter().append("g")
        .attr("class", "sk-node")
        .attr("transform", function (d) {
            return "translate(" + d.x0 + "," + d.y0 + ")";
        })
    if (configSankey.tooltip.infoDiv)
        node.on('mousemove', tipNodes.move).on('mouseover', tipNodes.show).on('mouseout', tipNodes.hide);
    else
        node.append("title").text(function (d) {
            return d.name + "\n" + _formatValueTooltip(d.value);
        });
    //Drag nodes	
    if (_nodes_draggableX || _nodes_draggableY)
        node.call(d3.drag().subject(function (d) {
            return d;
        }).on("start", function (d) {
            d3.event.sourceEvent.stopPropagation();
            this.parentNode.appendChild(this);
        }).on("drag", _dragmove));

    node.append("rect")
        .attr("height", function (d) {
            return (d.y1 - d.y0);
        })
        .attr("width", sankey.nodeWidth())
        .style("fill", function (d) {
            return d.color;
        })
        .style("stroke", function (d) {
            return d3.rgb(d.color).darker(1.8);
        });

    node.append("text")
        .attr("x", -6)
        .attr("y", function (d) {
            return (d.y1 - d.y0) / 2;
        })
        .attr("dy", ".35em")
        .attr("text-anchor", "end")
        .attr("transform", null)
        .style("fill", function (d) {
            return d3.rgb(d.color).darker(2.4);
        })
        .text(function (d) {
            return d.name;
        })
        .style("font-size", function (d) {
            return _getFontSize(d) + "px";
        })
        .filter(function (d) {
            return d.x0 < dimensions.width / 2;
        })
        .attr("x", 6 + sankey.nodeWidth())
        .attr("text-anchor", "start");

    //https://bl.ocks.org/syntagmatic/77c7f7e8802e8824eed473dd065c450b
    var _updateLinksValues = function (dataUpdated) {
        _stopTooltips();
        sankey(dataUpdated);
        sankey.update(dataUpdated);

        //update range font data
        _updateRangeFontData(dataUpdated.nodes);

        svg.selectAll(".sk-link")
            .data(dataUpdated.links, function (d) {
                return d.id;
            })
            .sort(function (a, b) {
                return b.width - a.width;
            })
            .transition()
            .duration(1300)
            .attr("d", path)
            .style("stroke-width", function (l) {
                return Math.max(1, l.width) + "px";
            });

        svg.selectAll(".sk-node")
            .data(dataUpdated.nodes, function (d) {
                return d.name;
            })
            .transition()
            .duration(1300)
            .attr("transform", function (d) {
                return "translate(" + d.x0 + "," + d.y0 + ")";
            });

        svg.selectAll(".sk-node rect")
            .transition()
            .duration(1300)
            .attr("height", function (d) {
                return (d.y1 - d.y0);
            });

        svg.selectAll(".sk-node text")
            .transition()
            .duration(1300)
            .attr("y", function (d) {
                return (d.y1 - d.y0) / 2;
            })
            .style("font-size", function (d) {
                return _getFontSize(d) + "px";
            });
    };

    //Update value of links, for call the function '_updateLinksValues' transition values (old to new)
    //This function only update values from links
    this.updateData = function (dataUpdated) {
        for (var i = 0; i < dataUpdated.links.length; i++) {
            var idLinkUpdate = dataUpdated.links[i].id || dataUpdated.links[i].source + " -> " + dataUpdated.links[i].target;
            var linkToUpdate = _dataSankey.links.filter(function (l) {
                return l.id == idLinkUpdate
            })[0];
            if (linkToUpdate) linkToUpdate.value = _safeValueToLink(dataUpdated.links[i].value);
        }
        _updateLinksValues(_dataSankey);
    };

    return this;
};
