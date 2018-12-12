import React from 'react'
import { Field, reduxForm } from 'redux-form'

let SelectAssembly = props => {
    const { handleSubmit } = props
    return <form onSubmit={handleSubmit}>
        <div>
            <label htmlFor="assemblyPath">Path to Assembly</label>
            <Field name="assemblyPath" component="input" type="text" />
        </div>
        <button type="submit">Submit</button>
    </form>
}

export default reduxForm({ form: 'selectAssembly' })(SelectAssembly);