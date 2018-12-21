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

const renderTextField = ({
    label,
    input,
    meta: { touched, invalid, error },
    classes,
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
const renderTextFieldWithStyle = withStyles(styles, { withTheme: true })(renderTextField);

let ProcessParameters = props => {
    const { handleSubmit, classes, parameters } = props

    return <form onSubmit={handleSubmit} className={classes.form} noValidate>
        {Object.keys(parameters).map(key => <Field key={key} name={key} component={renderTextFieldWithStyle} label={key} />)}
    </form>
}

export default reduxForm({ form: 'processParameters' })(withStyles(styles, { withTheme: true })(ProcessParameters));