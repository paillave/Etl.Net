import React from 'react';
import PropTypes from 'prop-types';
import classNames from 'classnames';
import { withStyles } from '@material-ui/core/styles';
import TableCell from '@material-ui/core/TableCell';
import TableSortLabel from '@material-ui/core/TableSortLabel';
import Paper from '@material-ui/core/Paper';
import { AutoSizer, Column, SortDirection, Table } from 'react-virtualized';
import { formatDate } from '../tools/dataAccess';
import DoneOutlinedIcon from '@material-ui/icons/DoneOutlined';
import DoneAllOutlinedIcon from '@material-ui/icons/DoneAllOutlined';

const styles = theme => ({
    table: {
        fontFamily: theme.typography.fontFamily,
    },
    flexContainer: {
        display: 'flex',
        alignItems: 'center',
        boxSizing: 'border-box',
    },
    tableRow: {
        cursor: 'pointer',
        // height: 30
    },
    tableRowHover: {
        '&:hover': {
            backgroundColor: theme.palette.grey[200],
        },
    },
    tableCell: {
        flex: 1,
        // height: 30
        // marginTop:5,
        // marginBottom:5,
    },
    noClick: {
        cursor: 'initial',
    },
    rowIcon: {
        marginLeft: 20,
        marginRight: 20,
    }
});

class MuiVirtualizedTable extends React.PureComponent {
    getRowClassName = ({ index }) => {
        const { classes, rowClassName, onRowClick } = this.props;

        return classNames(classes.tableRow, classes.flexContainer, rowClassName, {
            [classes.tableRowHover]: index !== -1 && onRowClick != null,
        });
    };

    cellRenderer = ({ cellData, columnIndex = null }) => {
        const { columns, classes, rowHeight, onRowClick } = this.props;
        return (
            <TableCell
                component="div"
                className={classNames(classes.tableCell, classes.flexContainer, {
                    [classes.noClick]: onRowClick == null,
                })}
                variant="body"
                style={{ height: rowHeight }}
                align={(columnIndex != null && columns[columnIndex].align) || 'left'}
            >
                {cellData}
            </TableCell>
        );
    };

    headerRenderer = ({ label, columnIndex, dataKey, sortBy, sortDirection }) => {
        const { headerHeight, columns, classes, sort } = this.props;
        const direction = {
            [SortDirection.ASC]: 'asc',
            [SortDirection.DESC]: 'desc',
        };

        const inner =
            !columns[columnIndex].disableSort && sort != null ? (
                <TableSortLabel active={dataKey === sortBy} direction={direction[sortDirection]}>
                    {label}
                </TableSortLabel>
            ) : (
                    label
                );

        return (
            <TableCell
                component="div"
                className={classNames(classes.tableCell, classes.flexContainer, classes.noClick)}
                variant="head"
                style={{ height: headerHeight }}
                align={columns[columnIndex].align || "left"}
            >
                {inner}
            </TableCell>
        );
    };

    render() {
        const { classes, columns, ...tableProps } = this.props;
        return (
            <AutoSizer>
                {({ height, width }) => (
                    <Table
                        className={classes.table}
                        height={height}
                        width={width}
                        {...tableProps}
                        rowClassName={this.getRowClassName}
                    >
                        {columns.map(({ cellContentRenderer = null, className, dataKey, ...other }, index) => {
                            let renderer;
                            if (cellContentRenderer != null) {
                                renderer = cellRendererProps =>
                                    this.cellRenderer({
                                        cellData: cellContentRenderer(cellRendererProps),
                                        columnIndex: index,
                                    });
                            } else {
                                renderer = this.cellRenderer;
                            }

                            return (
                                <Column
                                    key={dataKey}
                                    headerRenderer={headerProps =>
                                        this.headerRenderer({
                                            ...headerProps,
                                            columnIndex: index,
                                        })
                                    }
                                    className={classNames(classes.flexContainer, className)}
                                    cellRenderer={renderer}
                                    dataKey={dataKey}
                                    {...other}
                                />
                            );
                        })}
                    </Table>
                )}
            </AutoSizer>
        );
    }
}

MuiVirtualizedTable.propTypes = {
    classes: PropTypes.object.isRequired,
    columns: PropTypes.arrayOf(
        PropTypes.shape({
            cellContentRenderer: PropTypes.func,
            dataKey: PropTypes.string.isRequired,
            width: PropTypes.number.isRequired,
        }),
    ).isRequired,
    headerHeight: PropTypes.number,
    onRowClick: PropTypes.func,
    rowClassName: PropTypes.string,
    rowHeight: PropTypes.oneOfType([PropTypes.number, PropTypes.func]),
    sort: PropTypes.func,
};

MuiVirtualizedTable.defaultProps = {
    headerHeight: 40,
    rowHeight: 35,
};

const WrappedVirtualizedTable = withStyles(styles)(MuiVirtualizedTable);

class ReactVirtualizedTable extends React.PureComponent {
    getRowCount() {
        if (!this.props.selectedNode || !this.props.traces[this.props.selectedNode.nodeName]) return 0;
        return this.props.traces[this.props.selectedNode.nodeName].length;
    }
    getRowData({ index }) {
        if (!this.props.selectedNode || !this.props.traces[this.props.selectedNode.nodeName]) return {};
        return this.props.traces[this.props.selectedNode.nodeName][index];
    }
    handleRowClick(event) {
        this.props.showTraceDetails(event.rowData);
    }
    render() {
        const { classes } = this.props;
        return <Paper style={{ height: 600, width: '100%' }}>
            <WrappedVirtualizedTable
                rowCount={this.getRowCount.bind(this)()}
                rowGetter={this.getRowData.bind(this)}
                onRowClick={this.handleRowClick.bind(this)}
                columns={[
                    {
                        width: 50,
                        label: "",
                        dataKey: 'traceIcon',
                        cellRenderer: ({ rowData }) => {
                            switch (rowData.content.type) {
                                case "RowProcessStreamTraceContent": return (<DoneOutlinedIcon className={classes.rowIcon} />);
                                case "CounterSummaryStreamTraceContent": return (<DoneAllOutlinedIcon className={classes.rowIcon} />);
                            }
                        }
                    },
                    {
                        width: 90,
                        label: 'Row #',
                        dataKey: 'position',
                        cellDataGetter: ({ rowData }) => rowData.content.position,
                        align: 'right',
                    },
                    {
                        width: 200,
                        flexGrow: 1.0,
                        label: 'Date Time',
                        cellDataGetter: ({ rowData }) => formatDate(rowData.dateTime),
                        dataKey: 'dateTime',
                    },
                ]}
            />
        </Paper>;
    }
}

export default withStyles(styles)(ReactVirtualizedTable);
