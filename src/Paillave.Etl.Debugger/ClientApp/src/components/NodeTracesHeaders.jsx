import React from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import Typography from '@material-ui/core/Typography';
import RestorePageIcon from '@material-ui/icons/RestorePageOutlined';
import SettingsApplicationsIcon from '@material-ui/icons/SettingsApplicationsOutlined';
import SettingsIcon from '@material-ui/icons/SettingsOutlined';
import LinearProgress from '@material-ui/core/LinearProgress';

const styles = theme => ({
    root: {
        flexGrow: 1,
        marginBottom: 20
    },
    grow: {
        flexGrow: 1,
    },
    menuButton: {
        marginLeft: -12,
        marginRight: 20,
    },
    menuTitle: {
        display: "flex",
        flexDirection: "column",
        marginRight: 20,
        marginLeft: 10,
    },
    menuTitlePrimary: {
        ...theme.typography.h6
    },
    menuTitleSecondary: {
        ...theme.typography.subHeading
    }
});

// getRowCount() {
//     if (!this.props.selectedNode || !this.props.traces[this.props.selectedNode.nodeName]) return 0;
//     return this.props.traces[this.props.selectedNode.nodeName].length;
// }

class NodeTracesHeaders extends React.PureComponent {

    render() {
        const { classes, process, selectedNode, executingProcess } = this.props;
        return (
            <div className={classes.root}>
                <AppBar position="static" color="default">
                    {process && <Toolbar>
                        <RestorePageIcon />
                        <div className={classes.menuTitle}>
                            <Typography variant="h6" color="inherit" className={classes.grow}>{process.className}</Typography>
                            <Typography variant="body1" color="inherit" className={classes.grow}>{process.namespace}</Typography>
                        </div>
                        <SettingsApplicationsIcon />
                        <div className={classes.menuTitle}>
                            <Typography variant="h6" color="inherit" className={classes.grow}>{process.streamTransformationName}</Typography>
                            {selectedNode && <Typography variant="body1" color="inherit" className={classes.grow}>{selectedNode.nodeName}</Typography>}
                        </div>
                        {selectedNode && <React.Fragment>
                            <SettingsIcon />
                            <div className={classes.menuTitle}>
                                <Typography variant="h6" color="inherit" className={classes.grow}>{selectedNode.typeName}</Typography>
                                {selectedNode.rowCount && <Typography variant="body1" color="inherit" className={classes.grow}>{selectedNode.rowCount} row(s)</Typography>}
                            </div>
                        </React.Fragment>}
                    </Toolbar>}
                    {executingProcess && <LinearProgress />}
                </AppBar>
            </div>
        );
    }
}

NodeTracesHeaders.propTypes = {
    classes: PropTypes.object.isRequired,
};

export default withStyles(styles)(NodeTracesHeaders);