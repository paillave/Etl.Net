import React from "react";
import {withStyles} from '@material-ui/core/styles';
import {isImage} from './helpers/helpers.js';
import Grid from '@material-ui/core/Grid';
import DeleteIcon from '@material-ui/icons/Delete'; 
import AttachFileIcon from '@material-ui/icons/AttachFile';
import Fab from '@material-ui/core/Fab';

const styles = {
    removeBtn: {
        transition: '.5s ease',
        position: 'absolute',
        opacity: 0,
        top: -5,
        right: -5,
        width: 40,
        height: 40
    },
    smallPreviewImg: {
        height: 100,
        width: 'initial',
        maxWidth: '100%',
        marginTop: 5,
        marginRight: 10,
        color: 'rgba(0, 0, 0, 0.87)',
        transition: 'all 450ms cubic-bezier(0.23, 1, 0.32, 1) 0ms',
        boxSizing: 'border-box',
        boxShadow: 'rgba(0, 0, 0, 0.12) 0 1px 6px, rgba(0, 0, 0, 0.12) 0 1px 4px',
        borderRadius: 2,
        zIndex: 5,
        opacity: 1
    },
    imageContainer: {
        position: 'relative',
        zIndex: 10,
        textAlign: 'center',
        '&:hover $smallPreviewImg': {
            opacity: 0.3
        },
        '&:hover $removeBtn': {
            opacity: 1
        }
    }
}

function PreviewList(props){
    const {fileObjects, handleRemove, classes} = props
    return(
        <Grid container spacing={8}>
            {
                fileObjects.map((fileObject, i) => {
                    const img = (isImage(fileObject.file) ? 
                    <img className={classes.smallPreviewImg} role="presentation" src={fileObject.data}/>
                        : 
                        <AttachFileIcon className={classes.smallPreviewImg}/>
                    );
                    return (
                        <Grid item xs={4} key={i} className={classes.imageContainer}>
                            {img}
                            
                            <Fab onClick={handleRemove(i)}
                                aria-label="Delete" 
                                className={classes.removeBtn}>
                                <DeleteIcon />
                            </Fab>
                        </Grid>
                    );
                })
            }
        </Grid>
    )
}

export default withStyles(styles)(PreviewList);