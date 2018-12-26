import React, { Component, Fragment } from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import Dropzone from 'react-dropzone';
import CloudUploadIcon from '@material-ui/icons/CloudUpload';

const styles = {
    '@keyframes progress': {
        '0%': {
            backgroundPosition: '0 0',
        },
        '100%': {
            backgroundPosition: '-70px 0',
        },
    },
    dropZone: {
        position: 'relative',
        width: '100%',
        minHeight: '150px',
        backgroundColor: '#F0F0F0',
        border: 'dashed',
        borderColor: '#C8C8C8',
        cursor: 'pointer',
        boxSizing: 'border-box',
    },
    stripes: {
        border: 'solid',
        backgroundImage: 'repeating-linear-gradient(-45deg, #F0F0F0, #F0F0F0 25px, #C8C8C8 25px, #C8C8C8 50px)',
        animation: 'progress 2s linear infinite !important',
        backgroundSize: '150% 100%',
    },
    rejectStripes: {
        border: 'solid',
        backgroundImage: 'repeating-linear-gradient(-45deg, #fc8785, #fc8785 25px, #f4231f 25px, #f4231f 50px)',
        animation: 'progress 2s linear infinite !important',
        backgroundSize: '150% 100%',
    },
    dropzoneTextStyle: {
        textAlign: 'center'
    },
    uploadIconSize: {
        width: 51,
        height: 51,
        color: '#909090'
    },
    dropzoneParagraph: {
        fontSize: 24
    }
}


class DropzoneFileArea extends Component {
    onDrop(files) {
        if (this.props.onChange) {
            this.props.onChange(files[0]);
        }
        if (this.props.onDrop) {
            this.props.onDrop(files[0])
        }
    }
    handleDropRejected(rejectedFiles, evt) {
        if (this.props.onDropRejected) {
            this.props.onDropRejected(rejectedFiles[0], evt);
        }
    }
    render() {
        const { classes } = this.props;
        return (
            <Dropzone
                accept={this.props.acceptedFiles.join(',')}
                onDrop={this.onDrop.bind(this)}
                onDropRejected={this.handleDropRejected.bind(this)}
                className={classes.dropZone}
                acceptClassName={classes.stripes}
                rejectClassName={classes.rejectStripes}
                maxSize={this.props.maxFileSize}>
                <div className={classes.dropzoneTextStyle}>
                    <p className={classes.dropzoneParagraph}>
                        {this.props.children}
                    </p>
                    <CloudUploadIcon className={classes.uploadIconSize} />
                </div>
            </Dropzone>
        )
    }
}

DropzoneFileArea.defaultProps = {
    acceptedFiles: ['image/*', 'video/*', 'application/*'],
    maxFileSize: 3000000,

    onChange: () => { },
    onDrop: () => { },
    onDropRejected: () => { },
}
DropzoneFileArea.propTypes = {
    acceptedFiles: PropTypes.array,
    maxFileSize: PropTypes.number,

    onChange: PropTypes.func,
    onDrop: PropTypes.func,
    onDropRejected: PropTypes.func,
}
export default withStyles(styles)(DropzoneFileArea)