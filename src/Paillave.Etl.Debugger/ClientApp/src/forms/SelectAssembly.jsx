import React from 'react'
import { Field, reduxForm } from 'redux-form'
import { withStyles } from "@material-ui/core/styles";
import { Button, TextField } from '@material-ui/core';

const styles = theme => ({
    textField: {
        marginLeft: theme.spacing.unit,
        marginRight: theme.spacing.unit,
        width: 200,
    },
    form: {
        display: 'flex',
        flexDirection: 'column',
        margin: 'auto',
        width: 'fit-content',
    },
    filePath: {
        display: 'flex',
        alignItems: 'baseline'
    }
});

let SelectAssembly = props => {
    const { handleSubmit, classes } = props
    const renderTextField = ({
        label,
        input,
        meta: { touched, invalid, error },
        ...custom
    }) => (
            <TextField
                label={label}

                className={classes.textField}
                error={touched && invalid}
                helperText={touched && error}
                margin="normal"
                {...input}
                {...custom}
            />)

    return <form onSubmit={handleSubmit} className={classes.form} noValidate>
        <div className={classes.filePath}>
            <Field name="assemblyPath" component={renderTextField} label="Path to Assembly" />
            <Button type="submit">Load</Button>
        </div>
    </form>
}

export default reduxForm({ form: 'selectAssembly' })(withStyles(styles, { withTheme: true })(SelectAssembly));