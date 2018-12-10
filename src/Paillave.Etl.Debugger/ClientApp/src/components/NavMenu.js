import React from 'react';
// import { Link } from 'react-router-dom';
// import { Glyphicon, Nav, Navbar, NavItem } from 'react-bootstrap';
// import { LinkContainer } from 'react-router-bootstrap';
// import './NavMenu.css';
import { Nav } from 'office-ui-fabric-react/lib/Nav';

export default props => (
  <Nav
    groups={[
      {
        links: [
          {
            name: 'Home',
            url: 'http://example.com',
            links: [
              {
                name: 'Activity',
                url: 'http://msn.com',
                key: 'key1'
              },
              {
                name: 'MSN',
                url: 'http://msn.com',
                key: 'key2'
              }
            ],
            isExpanded: true
          },
          { name: 'Documents', url: 'http://example.com', key: 'key3', isExpanded: true },
          { name: 'Pages', url: 'http://msn.com', key: 'key4' },
          { name: 'Notebook', url: 'http://msn.com', key: 'key5' },
          { name: 'Long Name Test for ellipse', url: 'http://msn.com', key: 'key6' },
          {
            name: 'News',
            url: 'http://cnn.com',
            icon: 'News',
            key: 'key8'
          }
        ]
      }
    ]}
    // onLinkClick={this.onLinkClick}
    expandedStateText={'expanded'}
    collapsedStateText={'collapsed'}
    selectedKey={'key3'}
    expandButtonAriaLabel={'Expand or collapse'}
  />
);
