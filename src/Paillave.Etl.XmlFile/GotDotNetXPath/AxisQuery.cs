//------------------------------------------------------------------------------
// <copyright file="AxisQuery.cs" company="Microsoft">
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
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Xml;

    internal sealed class NullQuery : BaseAxisQuery {

        internal override bool MatchNode(XPathReader reader) {
            return false;
        }
    }

    internal sealed class AttributeQuery : BaseAxisQuery {

        internal AttributeQuery(IQuery qyParent, String name, String prefix, XPathNodeType type)
                               : base(qyParent, name, prefix, type) {
        }

        //
        // we need to walk the attribute for
        // query like e/@a and position the reader to that
        // attribute node.

        // example: e/attribute::node()
        //          e/attribute::text() //no return
        //          e/@a
        //
        // There are two situations to match the attributes
        // 1). user has moved the reader to an attribute in the current element context
        // 2). user still in the element context, since it's an attribute query, we
        //     need to move to the attribute ourself.

        internal override bool MatchNode(XPathReader reader) {
            bool ret = true;

            if (base.NodeType != XPathNodeType.All) {

                if (!MatchType(base.NodeType, reader.NodeType)) {
                    ret = false;
                } else if (base.Name != string.Empty && (base.Name != reader.Name 
                    || base.Prefix != reader.Prefix)) {
                    ret = false;
                }
            }

            return ret;
        }

        // The reader will be the current element node
        // We need to restore the
        internal override object GetValue(XPathReader reader) {

            XmlReader baseReader = reader.BaseReader;

            object ret = null;

            if (baseReader.MoveToAttribute(base.Name)) {
                ret = reader.Value;
            }

            //Move back to the parent
            baseReader.MoveToElement();
            return ret;
        }
    }


    //
    // handles the child axis in the following situation:
    //  foo:bar
    //  child::*
    //  child::node() //element, text, comment, PI, no attribute
    //  child::Text()
    //  child::ProcessingInstruction()
    //  child::comment();
    internal class ChildQuery : BaseAxisQuery {

        internal ChildQuery(IQuery qyInput, String name, String prefix, XPathNodeType type)
                              : base (qyInput, name, prefix, type) {
        }

        // try to match the node
        internal override bool MatchNode(XPathReader reader) {

            bool ret = true;

            if ( base.NodeType != XPathNodeType.All) {

                if (!MatchType(base.NodeType, reader.NodeType)) {
                    ret = false;
                } 
                else if(base.Name != string.Empty && (base.Name != reader.LocalName 
                    || (base.Prefix.Length != 0 && reader.MapPrefixWithNamespace(base.Prefix) == false))) 
                {
                    //currently the AstNode build initial the name as String.Empty
                    ret = false;
                }
            }

            return ret;
        }


        //
        // children query return value when it's in the
        // predicates
        // for example: <e><e1>1</e1><e1>2<e1/></e>
        // e[e1 = 1]
        // Can't move the reader forward.
        internal override object GetValue(XPathReader reader)
        {
            throw new XPathReaderException("Can't get the child value");
        }
    }

    internal class XPathSelfQuery : BaseAxisQuery {

        internal XPathSelfQuery() {
        }
        internal XPathSelfQuery(IQuery queryInput, string name, string prefix, XPathNodeType type)
                                : base(queryInput, name,  prefix, type) {
        }

        internal override bool MatchNode(XPathReader reader) {
            bool ret = true;

            if (base.NodeType != XPathNodeType.All) {

                if (!MatchType(base.NodeType, reader.NodeType)) {
                    ret = false;
                } else if(base.Name != null && (base.Name != reader.Name || base.Prefix != reader.Prefix)) {
                    ret = false;
                }
            }
            return ret;
        }

        internal override object GetValue(XPathReader reader)
        {
            if (reader.HasValue) {
                return (object)reader.Value;
            } else {
                throw new XPathReaderException("Can't get the element value");
            }
        }
    }

    internal class DescendantQuery : BaseAxisQuery {

        public DescendantQuery(IQuery  qyInput, String name, String prefix, XPathNodeType type)
                                : base(qyInput, name,  prefix, type) {
        }

        internal override bool MatchNode(XPathReader reader)
        {

            bool ret = true;

            if (base.NodeType != XPathNodeType.All) {

                if (!MatchType(base.NodeType, reader.NodeType)) {
                    ret = false;
                } else if(base.Name != null && (base.Name != reader.Name || base.Prefix != reader.Prefix)) {
                    ret = false;
                }
            }

            return ret;
        }


        //
        // Desendant query value
        //
        // <e><e1>1</e1><e2>2</e2></e>
        //
        // e[desendant::node()=1]
        //
        // current context node need to be saved if
        // we need to solve the case for future.
        internal override object GetValue(XPathReader reader)
        {
            throw new XPathReaderException("Can't get the decendent nodes value");
        }

    }

    internal sealed class AbsoluteQuery : XPathSelfQuery {

    }


    internal sealed class UnionQuery : BaseAxisQuery {
        private IQuery query1, query2;

        internal UnionQuery(IQuery query1, IQuery query2) {
            this.query1 = query1;
            this.query2 = query2;
        }

        internal override bool MatchNode(XPathReader reader)
        {
            if (this.query1.MatchNode(reader)) {
                return true;
            }

            return (this.query2.MatchNode(reader));
        }

        //
        // <e><e1>1</e1><e2>2</e2></e>
        //
        // e[ e1 | e2 = 1]
        //
        // results:
        // e
        //
        // Union query needs to return two objects
        //
        // The only success situation is the two attribute situations
        // because only the attribute within current scope are cached.
        //
        internal override object GetValue(XPathReader reader)
        {
            object[] objArray;

            objArray = new object[2];
            objArray[0] = this.query1.GetValue(reader);
            objArray[1] = this.query2.GetValue(reader);
            return (object)objArray;
        }
    }


    internal sealed class GroupQuery : BaseAxisQuery {

        internal GroupQuery(IQuery queryInput): base(queryInput)
        {
        }

        internal override object GetValue(XPathReader reader)
        {
            return base.QueryInput.GetValue(reader);
        }

        internal override XPathResultType ReturnType()
        {
            return base.QueryInput.ReturnType();
        }

        internal override bool MatchNode(XPathReader reader)
        {
            return base.MatchNode(reader);
        }
    }
}
