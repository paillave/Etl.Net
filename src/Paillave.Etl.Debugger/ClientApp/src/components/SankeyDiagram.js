import createSankey from "../tools/createSankey";

var configSankey = {
    margin: { top: 10, left: 10, right: 10, bottom: 10 },
    nodes: {
        dynamicSizeFontNode: false,
        draggableX: true,
        draggableY: true
    },
    links: {
        formatValue: function(val) {
            return d3.format("0")(val) + ' row(s)';
        }
    },
    tooltip: {
        infoDiv: true,
        labelSource: 'Input:',
        labelTarget: 'Output:'
    }
}
var values = '<<SANKEY_STATISTICS>>';
var objSankey = createSankey('#chart', configSankey, values);
