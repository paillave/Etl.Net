//------------------------------------------------------------------------------
// <copyright file="FilterQuery.cs" company="Microsoft">
//     
//      Copyright (c) 2002 Microsoft Corporation.  All rights reserved.
//     
//      The use and distribution terms for this software are contained in the file
//      named license.txt, which can be found in the root of this distribution.
//      By using this software in any fashion, you are agreeing to be bound by the
//      terms of this license.
//     
//      You must not remove this notice, or any other, from this software.
//     
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace GotDotNet.XPath {

    using System;
    using System.Xml;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Diagnostics;


    // Filter query
    // PathExpr ::= LocaltionPath
    //              | FilterExpr
    //              | FilterExpr '/' RelativeLocationPath
    //              | FilterExpr '//'RelativeLocationPath
    //
    // FilterExpr ::= PrimaryExpr | FilterExpr Predicate

    // PrimaryExpr    ::=    VariableReference
    //    | '(' Expr ')'
    //    | Literal
    //    | Number
    //    | FunctionCall
    //

    internal class FilterQuery : BaseAxisQuery {
        IQuery predicate;
        int matchCount = 0;
        IQuery axis;

        internal BaseAxisQuery Axis
        {
            get { return (BaseAxisQuery)axis; }
        }

        internal FilterQuery(IQuery axisQuery, IQuery predicate)
        {
           base.QueryInput = ((BaseAxisQuery)axisQuery).QueryInput;
           this.predicate = predicate;
           this.axis = axisQuery;
       }

        //
        // predicate could be result in two results type
        // 1). Number: postion query
        // 2). Boolean
        //
        internal override bool MatchNode(XPathReader reader)
        {
            bool ret = false;

            if (this.axis.MatchNode(reader)) {
                if (reader.NodeType != XmlNodeType.EndElement && reader.ReadMethod != ReadMethods.MoveToElement) {
                    ++matchCount;
                    //
                    // send postion information down to the prdicates
                    //
                    this.predicate.PositionCount = matchCount;
                }

                object obj = this.predicate.GetValue(reader);
                if (obj is System.Boolean && Convert.ToBoolean(obj) == true) {
                    ret = true;
                } else if (obj is System.Double && Convert.ToDouble(obj) == matchCount) {
                    //we need to know how many this axis has been evaluated
                    ret = true;
                } else if (obj != null && !(obj is System.Double || obj is System.Boolean) ){ //object is nodeset
                    ret = true;
                }
            }
            return ret;
        }

        //
        // The filter query value should the selected
        // node value
        // for example:
        //
        // <e><e1 a='1' b='2'><e2 a='3'/><e2 a='1/></e1></e>
        //
        // /e/e1[e2[@a='1'] = 1]
        //
        internal override object GetValue(XPathReader reader)
        {
            throw new XPathReaderException("Can't get value");

        }
    }
}
