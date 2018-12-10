import React from 'react';
import List from '@material-ui/core/List';
import Divider from '@material-ui/core/Divider';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import InputIcon from '@material-ui/icons/Input';
import ListItemText from '@material-ui/core/ListItemText';
import ListSubheader from '@material-ui/core/ListSubheader';

export default props => (
  <React.Fragment>
    <Divider />
    <List>
      <ListSubheader>Class1</ListSubheader>
      <ListItem button>
        <ListItemIcon>
          <InputIcon />
        </ListItemIcon>
        <ListItemText primary="Process1" />
      </ListItem>
      <ListItem button>
        <ListItemIcon>
          <InputIcon />
        </ListItemIcon>
        <ListItemText primary="Process2" />
      </ListItem>
      <ListItem button>
        <ListItemIcon>
          <InputIcon />
        </ListItemIcon>
        <ListItemText primary="Process3" />
      </ListItem>
    </List>
    <Divider />
    <List>
      <ListSubheader>Class2</ListSubheader>
      <ListItem button>
        <ListItemIcon>
          <InputIcon />
        </ListItemIcon>
        <ListItemText primary="Process1" />
      </ListItem>
      <ListItem button>
        <ListItemIcon>
          <InputIcon />
        </ListItemIcon>
        <ListItemText primary="Process2" />
      </ListItem>
      <ListItem button>
        <ListItemIcon>
          <InputIcon />
        </ListItemIcon>
        <ListItemText primary="Process3" />
      </ListItem>
    </List>
  </React.Fragment>

);
