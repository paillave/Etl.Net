//------------------------------------------------------------------------------
// <copyright file="XPathReader.cs" company="Microsoft">
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
namespace GotDotNet.XPath
{

    using System;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Xml;
    using System.Diagnostics;

    internal enum ReadMethods
    {
        Read,
        ReadUntil,
        MoveToAttribute,
        MoveToElement,
        None
    }

    public class XPathReader : XmlReader
    {

        XmlReader reader;
        XPathCollection xpathCollection;
        int processAttribute = -1;
        ReadMethods readMethod = ReadMethods.None;

        XPathReader()
        {
        }



        /// <summary>
        ///    Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"
        /// </summary>
        public XPathReader(XmlReader reader, XPathCollection xc) : this()
        {
            xpathCollection = xc;
            xc.SetReader = this;
            this.reader = reader;

        }



        /// <summary>
        ///    Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"
        /// </summary>

        //
        // For class function provides the url, we will construct the
        // XmlTextReader to process the URL
        //
        public XPathReader(string url, string xpath) : this()
        {
            this.reader = new XmlTextReader(url);
            xpathCollection = new XPathCollection();
            xpathCollection.Add(xpath);
        }



        /// <summary>
        ///    Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"
        /// </summary>

        //
        // For class function provides the url, we will construct the
        // XmlTextReader to process the URL
        //
        public XPathReader(TextReader reader, string xpath) : this()
        {
            this.reader = new XmlTextReader(reader);
            xpathCollection = new XPathCollection();
            xpathCollection.Add(xpath);
        }




        /// <summary>
        ///    Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"
        /// </summary>

        //
        // For class function provides the url, we will construct the
        // XmlTextReader to process the URL
        //
        public XPathReader(string url, XPathCollection xc) : this(new XmlTextReader(url), xc)
        {
        }



        /// <summary>
        ///    Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"
        /// </summary>

        //
        // check the query index in the query
        // collections match the result that we are
        // looking for.
        public bool Match(int queryIndex)
        {
#if DEBUG1
            Console.WriteLine("queryIndex: {0}", queryIndex);
#endif
            if (xpathCollection[queryIndex] != null)
            {
                return (xpathCollection[queryIndex].Match());
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        ///    Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"
        /// </summary>
        public bool Match(string xpathQuery)
        {
            return true;
        }



        /// <summary>
        ///    return true when the 
        /// </summary>
        public bool Match(XPathQuery xpathExpr)
        {

            if (xpathCollection.Contains(xpathExpr) && xpathExpr.Match())
            {
                return true;
            }

            return false;
        }



        /// <summary>
        ///     return true if one of the queries matches with the XmlReader context. 
        /// </summary>
        public bool MatchesAny(ArrayList queryList)
        {

            return (xpathCollection.MatchesAny(queryList, this.reader.Depth));
        }

        /// <summary>
        ///     return true if one of the queries matches with the XmlReader context. 
        /// </summary>
        public bool ReadUntilMatch(Action endOfUnmatchedElementAction = null)
        {

            while (true)
            {

                if (this.processAttribute > 0)
                {
                    //need to process the attribute one at time

                    if (MoveToNextAttribute())
                    { //attributeIndex < AttributeCount) {
                        //MoveToAttribute(attributeIndex++);
                        if (xpathCollection.MatchAnyQuery())
                        {
                            return true;
                        }
                    }
                    else
                    {
                        this.processAttribute = -1; //stop attributes processing.
                    }

                }
                else if (this.reader.Read())
                {
                    xpathCollection.AdvanceUntil(this);
                    if (xpathCollection.MatchAnyQuery())
                    {
                        return true;
                    }
                    else
                    {
                        if (endOfUnmatchedElementAction != null && this.reader.NodeType == XmlNodeType.EndElement) endOfUnmatchedElementAction();
                    }
                }
                else
                {
                    return false;
                }
            }
        }


        /// <summary>
        ///    
        ///       Reads the next
        ///       node from the stream.
        ///    
        /// </summary>
        public override bool Read()
        {

            this.readMethod = ReadMethods.Read;

            bool ret = true;
            ret = this.reader.Read();

            if (ret)
            {
                xpathCollection.Advance(this);
            }
            return ret;
        }



        /// <summary>
        /// Moves to the attribute with the specified <see cref='System.Xml.XPathReader.Name'/> .
        /// </summary>
        public override bool MoveToAttribute(string name)
        {
            this.readMethod = ReadMethods.MoveToAttribute;

            bool ret = false;

            ret = this.reader.MoveToAttribute(name);

            if (ret)
            {
                xpathCollection.Advance(this);
            }
            return ret;
        }



        /// <summary>
        /// Moves to the attribute with the specified <see cref='System.Xml.XPathReader.localName'/>
        /// and <see cref='System.Xml.XPathReader.nsURI'/> .
        /// </summary>
        public override bool MoveToAttribute(string name, string ns)
        {
            bool ret = false;
            ret = this.reader.MoveToAttribute(name, ns);

            if (ret)
            {
                xpathCollection.Advance(this);
            }
            return ret;
        }


        /// <summary>
        ///    Moves to the attribute with the specified index.
        /// </summary>
        public override void MoveToAttribute(int i)
        {
            this.readMethod = ReadMethods.MoveToAttribute;
            this.reader.MoveToAttribute(i);
            xpathCollection.Advance(this);

        }


        /// <summary>
        ///    
        ///       Moves to the first attribute.
        ///    
        /// </summary>
        public override bool MoveToFirstAttribute()
        {
            bool ret = false;

            ret = this.reader.MoveToFirstAttribute();

            if (ret)
            {
                xpathCollection.Advance(this);
            }
            return ret;
        }


        /// <summary>
        ///    
        ///       Moves to the next attribute.
        ///    
        /// </summary>
        public override bool MoveToNextAttribute()
        {
            bool ret = false;

            ret = this.reader.MoveToNextAttribute();

            if (ret)
            {
                xpathCollection.Advance(this);
            }
            return ret;
        }


        /// <summary>
        ///    
        ///       Moves to the element that contains the current attribute node.
        ///    
        /// </summary>
        public override bool MoveToElement()
        {
            bool ret = false;

            readMethod = ReadMethods.MoveToElement;

            ret = this.reader.MoveToElement();

            if (ret)
            {
                xpathCollection.Advance(this);
            }

            return ret;
        }

        // Node Properties

        /// <summary>
        ///    
        ///       Gets the type of the current node.
        ///    
        /// </summary>
        public override XmlNodeType NodeType
        {
            get { return (this.reader.NodeType); }
        }

        /// <summary>
        ///    Gets the name of
        ///       the current node, including the namespace prefix.
        /// </summary>
        public override string Name
        {
            get { return (this.reader.Name); }
        }


        /// <summary>
        ///    
        ///       Gets the name of the current node without the namespace prefix.
        ///    
        /// </summary>
        public override string LocalName
        {
            get { return (this.reader.LocalName); }
        }


        /// <summary>
        ///    
        ///       Gets the namespace URN (as defined in the W3C Namespace Specification) of the current namespace scope.
        ///    
        /// </summary>
        public override string NamespaceURI
        {
            get
            {
                return (this.reader.NamespaceURI);
            }
        }


        /// <summary>
        ///    
        ///       Gets the namespace prefix associated with the current node.
        ///    
        /// </summary>
        public override string Prefix
        {
            get { return (this.reader.Prefix); }
        }


        /// <summary>
        ///    
        ///       Gets a value indicating whether
        ///    <see cref='System.Xml.XPathReader.Value'/> has a value to return.
        ///    
        /// </summary>
        public override bool HasValue
        {
            get { return (this.reader.HasValue); }
        }


        /// <summary>
        ///    
        ///       Gets the text value of the current node.
        ///    
        /// </summary>
        public override string Value
        {
            get { return (this.reader.Value); }
        }


        /// <summary>
        ///    
        ///       Gets the depth of the
        ///       current node in the XML element stack.
        ///    
        /// </summary>
        public override int Depth
        {
            get { return (this.reader.Depth); }
        }


        /// <summary>
        ///    
        ///       Gets the base URI of the current node.
        ///    
        /// </summary>
        public override string BaseURI
        {
            get { return (this.reader.BaseURI); }
        }


        /// <summary>
        ///    Gets a value indicating whether
        ///       the current
        ///       node is an empty element (for example, &lt;MyElement/&gt;).
        /// </summary>
        public override bool IsEmptyElement
        {
            get { return (this.reader.IsEmptyElement); }
        }


        /// <summary>
        ///    
        ///       Gets a value indicating whether the current node is an
        ///       attribute that was generated from the default value defined
        ///       in the DTD or schema.
        ///    
        /// </summary>
        public override bool IsDefault
        {
            get { return (this.reader.IsDefault); }
        }


        /// <summary>
        ///    
        ///       Gets the quotation mark character used to enclose the value of an attribute
        ///       node.
        ///    
        /// </summary>
        public override char QuoteChar
        {
            get { return (this.reader.QuoteChar); }
        }


        /// <summary>
        ///    Gets the current xml:space scope.
        /// </summary>
        public override XmlSpace XmlSpace
        {
            get { return (this.reader.XmlSpace); }
        }


        /// <summary>
        ///    Gets the current xml:lang scope.
        /// </summary>
        public override string XmlLang
        {
            get { return (this.reader.XmlLang); }
        }

        // Attribute Accessors

        /// <summary>
        ///     The number of attributes on the current node.
        /// </summary>
        public override int AttributeCount
        {
            get { return (this.reader.AttributeCount); }
        }


        /// <summary>
        ///    Gets the value of the attribute with the specified
        ///    <see cref='System.Xml.XPathReader.Name'/> .
        /// </summary>
        public override string GetAttribute(string name)
        {
            return this.reader.GetAttribute(name);
        }


        /// <summary>
        ///    Gets the value of the attribute with the
        ///       specified <see cref='System.Xml.XPathReader.LocalName'/> and <see cref='System.Xml.XPathReader.NamespaceURI'/> .
        /// </summary>
        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.reader.GetAttribute(name, namespaceURI);
        }


        /// <summary>
        ///    Gets the value of the attribute with the specified index.
        /// </summary>
        public override string GetAttribute(int i)
        {
            return this.reader.GetAttribute(i);
        }


        /// <summary>
        ///    Gets the value of the attribute with the specified index.
        /// </summary>
        public override string this[int i]
        {
            get { return this.reader[i]; }
        }


        /// <summary>
        ///    Gets the value of the attribute with the specified
        ///    <see cref='System.Xml.XPathReader.Name'/> .
        /// </summary>
        public override string this[string name]
        {
            get { return this.reader[name]; }
        }


        /// <summary>
        ///    Gets the value of the attribute with the
        ///       specified <see cref='System.Xml.XPathReader.LocalName'/> and <see cref='System.Xml.XPathReader.NamespaceURI'/> .
        /// </summary>
        public override string this[string name, string namespaceURI]
        {
            get { return this.reader[name, namespaceURI]; }
        }

        //UE Atention

        public override bool CanResolveEntity
        {
            get { return this.reader.CanResolveEntity; }
        }


        /// <summary>
        ///    
        ///       Gets
        ///       a value indicating whether XmlReader is positioned at the end of the
        ///       stream.
        ///    
        /// </summary>
        public override bool EOF
        {
            get { return this.reader.EOF; }
        }


        /// <summary>
        ///    
        ///       Closes the stream, changes the <see cref='System.Xml.XPathReader.ReadState'/>
        ///       to Closed, and sets all the properties back to zero.
        ///    
        /// </summary>
        public override void Close()
        {
            this.reader.Close();
        }


        /// <summary>
        ///    
        ///       Returns
        ///       the read state of the stream.
        ///    
        /// </summary>
        public override ReadState ReadState
        {
            get { return this.reader.ReadState; }
        }


        /// <summary>
        ///    Reads the contents of an element as a string.
        /// </summary>
        public override string ReadString()
        {
            return this.reader.ReadString();
        }


        // Nametable and Namespace Helpers

        /// <summary>
        ///    Gets the XmlNameTable associated with this
        ///       implementation.
        /// </summary>
        public override XmlNameTable NameTable
        {
            get { return this.reader.NameTable; }
        }


        /// <summary>
        ///    
        ///       Resolves a namespace prefix in the current element's scope.
        ///    
        /// </summary>
        public override string LookupNamespace(string prefix)
        {
            return this.reader.LookupNamespace(prefix);

        }

        /// <summary>
        ///    Resolves the entity reference for nodes of NodeType EntityReference.
        /// </summary>
        public override void ResolveEntity()
        {
            this.reader.ResolveEntity();
        }


        /// <summary>
        ///    Parses the attribute value into one or more Text and/or EntityReference node
        ///       types.
        /// </summary>
        public override bool ReadAttributeValue()
        {
            return this.reader.ReadAttributeValue();
        }


        /// <summary>
        ///    Reads all the content (including markup) as a string.
        /// </summary>
        public override string ReadInnerXml()
        {
            return this.reader.ReadInnerXml();
        }

        /// <summary>
        ///    [To be supplied.]
        /// </summary>
        public override string ReadOuterXml()
        {
            return this.reader.ReadOuterXml();
        }

        //----------------------------------------------------
        // internal methods
        //

        internal XmlReader BaseReader
        {
            get { return this.reader; }
        }

        internal int ProcessAttribute
        {
            get { return this.processAttribute; }
            set { this.processAttribute = value; }
        }

        internal ReadMethods ReadMethod
        {
            get { return readMethod; }
        }

        internal bool MapPrefixWithNamespace(string prefix)
        {

            XmlNamespaceManager nsMgr = xpathCollection.NamespaceManager;

            if (nsMgr != null && nsMgr.LookupNamespace(prefix) == this.NamespaceURI)
            {
                return true;
            }

            return false;
        }
    }
}