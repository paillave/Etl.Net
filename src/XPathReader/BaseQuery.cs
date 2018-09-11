//------------------------------------------------------------------------------
// <copyright file="BaseQuery.cs" company="Microsoft">
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

    internal class BaseAxisQuery : IQuery {
        IQuery queryInput = null;
        string name = String.Empty;
        string prefix = String.Empty;
        XPathNodeType nodeType;
        int expectedDepth = -1;

        internal BaseAxisQuery() {
        }

        internal BaseAxisQuery(IQuery queryInput) {
            this.queryInput = queryInput;
        }

        internal BaseAxisQuery(IQuery queryInput, string name, string prefix, XPathNodeType nodeType) {
            this.prefix = prefix;
            this.queryInput = queryInput;
            this.name = name;
            this.nodeType = nodeType;
        }

        internal override XPathResultType ReturnType() {
            return XPathResultType.NodeSet;
        }

        internal string Name {
            get { return this.name; }
        }
        
        internal string Prefix {
            get { return this.prefix; }
        }

        internal XPathNodeType NodeType {
            get { return this.nodeType; }
        }

        internal int Depth {
            get { return this.expectedDepth; }
            set { this.expectedDepth = value; }
        }

        internal IQuery QueryInput {
            get { return this.queryInput; }
            set { this.queryInput = value; }
        }

        internal bool MatchType(XPathNodeType xType, XmlNodeType type) {
            bool ret = false;

            switch (xType) {
                case XPathNodeType.Element:
                    if (type == XmlNodeType.Element || type == XmlNodeType.EndElement) {
                        ret = true;
                    }
                    break;

                case XPathNodeType.Attribute:
                    if (type == XmlNodeType.Attribute) {
                        ret = true;
                    }
                    break;

                case XPathNodeType.Text:
                    if (type == XmlNodeType.Text) {
                        ret = true;
                    }
                    break;

                case XPathNodeType.ProcessingInstruction:
                    if (type == XmlNodeType.ProcessingInstruction) {
                        ret = true;
                    }
                    break;

                case XPathNodeType.Comment:
                    if (type == XmlNodeType.Comment) {
                        ret = true;
                    }
                    break;

                default:
                    throw new XPathReaderException("Unknown nodeType");
            }
            return ret;
        }
    }
}