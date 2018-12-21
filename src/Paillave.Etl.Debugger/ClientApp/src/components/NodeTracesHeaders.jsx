import React from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import Typography from '@material-ui/core/Typography';
import RestorePageIcon from '@material-ui/icons/RestorePageOutlined';
import SettingsApplicationsIcon from '@material-ui/icons/SettingsApplicationsOutlined';

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
        marginRight:20,
        marginLeft:10,
    },
    menuTitlePrimary: {
        ...theme.typography.h6
    },
    menuTitleSecondary: {
        ...theme.typography.subHeading
    }
});

function NodeTracesHeaders(props) {
    const { classes, process, selectedNode } = props;

    return (
        <div className={classes.root}>
            <AppBar position="static" color="default">
                <Toolbar>
                    <RestorePageIcon/>
                    <div className={classes.menuTitle}>
                        <Typography variant="h6" color="inherit" className={classes.grow}>{process.className}</Typography>
                        <Typography variant="body1" color="inherit" className={classes.grow}>{process.namespace}</Typography>
                    </div>
                    <SettingsApplicationsIcon/>
                    <div className={classes.menuTitle}>
                        <Typography variant="h6" color="inherit" className={classes.grow}>{process.streamTransformationName}</Typography>
                        <Typography variant="body1" color="inherit" className={classes.grow}>{selectedNode.name}</Typography>
                    </div>
                    {/* <Typography variant="h6" color="inherit" className={classes.grow}>
                        {process.namespace}/{process.className}/{process.streamTransformationName}/{selectedNode.name}
                    </Typography> */}
                </Toolbar>
            </AppBar>
        </div>
    );
}

NodeTracesHeaders.propTypes = {
    classes: PropTypes.object.isRequired,
};

export default withStyles(styles)(NodeTracesHeaders);