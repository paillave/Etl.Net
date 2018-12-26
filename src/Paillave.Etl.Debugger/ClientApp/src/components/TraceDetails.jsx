import React from "react";
import ReactJson from 'react-json-view'
import { withStyles } from "@material-ui/core/styles";

const styles = theme => ({
  container: {
    margin: theme.spacing.unit,
  },
});

class TraceDetails extends React.Component {
  render() {
    const { classes, theme,selectedTrace } = this.props;
    return <div className={classes.container} ><ReactJson src={ selectedTrace } /></div>;
  }
}

export default withStyles(styles, { withTheme: true })(TraceDetails);
