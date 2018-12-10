import React from 'react';
import NavMenu from './NavMenu';
import 'office-ui-fabric-react/dist/css/fabric.css';

export default props => (
  <div class="ms-Grid" dir="ltr">
    <div class="ms-Grid-row">
      <div class="ms-Grid-col ms-sm6 ms-md4 ms-lg2"><NavMenu /></div>
      <div class="ms-Grid-col ms-sm6 ms-md8 ms-lg10">        {props.children}
      </div>
    </div>
  </div>
);
