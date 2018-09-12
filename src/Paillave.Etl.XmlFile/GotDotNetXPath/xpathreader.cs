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
namespace GotDotNet.XPath {

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

    public class XPathReader : XmlReader {

        XmlReader reader;
        XPathCollection xpathCollection;
        int processAttribute = -1;
        ReadMethods readMethod = ReadMethods.None;

        XPathReader() {
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XmlTextReader1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"</para>
        /// </devdoc>
        public XPathReader(XmlReader reader, XPathCollection xc) : this() {
            xpathCollection = xc;
            xc.SetReader = this;
            this.reader = reader;

        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XPathReader1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"</para>
        /// </devdoc>

        //
        // For class function provides the url, we will construct the
        // XmlTextReader to process the URL
        //
        public XPathReader(string url, string xpath) : this(){
            this.reader = new XmlTextReader(url);
            xpathCollection = new XPathCollection();
            xpathCollection.Add(xpath);
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XPathReader1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"</para>
        /// </devdoc>

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


        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XPathReader1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"</para>
        /// </devdoc>

        //
        // For class function provides the url, we will construct the
        // XmlTextReader to process the URL
        //
        public XPathReader(string url, XPathCollection xc) : this(new XmlTextReader(url), xc)
        {
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XPathReader1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"</para>
        /// </devdoc>

        //
        // check the query index in the query
        // collections match the result that we are
        // looking for.
        public bool Match(int queryIndex) {
#if DEBUG1
            Console.WriteLine("queryIndex: {0}", queryIndex);
#endif
            if (xpathCollection[queryIndex] != null) {
                return (xpathCollection[queryIndex].Match());
            }
            else {
                return false;
            }
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XPathReader1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Initializes a new instance of the XPathReader class with the specified XmlNameTable.
        ///    This constructor is used when creating reader with "new XPathReader(..)"</para>
        /// </devdoc>
        public bool Match(string xpathQuery) {
            return true;
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XPathReader1"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>return true when the </para>
        /// </devdoc>
        public bool Match(XPathQuery xpathExpr) {

            if (xpathCollection.Contains(xpathExpr) && xpathExpr.Match()) {
                return true;
            }

            return false;
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.MatchesAny"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para> return true if one of the queries matches with the XmlReader context. </para>
        /// </devdoc>
        public bool MatchesAny(ArrayList queryList) {

            return (xpathCollection.MatchesAny(queryList, this.reader.Depth));
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.ReadUntilMatch"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para> return true if one of the queries matches with the XmlReader context. </para>
        /// </devdoc>
        public bool ReadUntilMatch() {

            while (true) {

                if (this.processAttribute > 0) {
                    //need to process the attribute one at time

                    if (MoveToNextAttribute()) { //attributeIndex < AttributeCount) {
                        //MoveToAttribute(attributeIndex++);
                        if (xpathCollection.MatchAnyQuery()) {
                            return true;
                        }
                    } else {
                        this.processAttribute = -1; //stop attributes processing.
                    }

                } else if (this.reader.Read()) {
                    xpathCollection.AdvanceUntil(this);
                    if (xpathCollection.MatchAnyQuery()) {
                        return true;
                    }
                } else {
                    return false;
                }
            }
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.Read"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Reads the next
        ///       node from the stream.
        ///    </para>
        /// </devdoc>
        public override bool Read() {

            this.readMethod = ReadMethods.Read;

            bool ret = true;
            ret = this.reader.Read();

            if(ret) {
                xpathCollection.Advance(this);
            }
            return ret;
        }


        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.MoveToAttribute"]/*' />
        /// <devdoc>
        /// <para>Moves to the attribute with the specified <see cref='System.Xml.XPathReader.Name'/> .</para>
        /// </devdoc>
        public override bool MoveToAttribute(string name) {
            this.readMethod = ReadMethods.MoveToAttribute;

            bool ret = false;

            ret = this.reader.MoveToAttribute(name);

            if (ret) {
                xpathCollection.Advance(this);
            }
            return ret;
        }


        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.MoveToAttribute1"]/*' />
        /// <devdoc>
        /// <para>Moves to the attribute with the specified <see cref='System.Xml.XPathReader.localName'/>
        /// and <see cref='System.Xml.XPathReader.nsURI'/> .</para>
        /// </devdoc>
        public override bool MoveToAttribute(string name, string ns) {
            bool ret = false;
            ret = this.reader.MoveToAttribute(name, ns);

            if (ret) {
                xpathCollection.Advance(this);
            }
            return ret;
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.MoveToAttribute2"]/*' />
        /// <devdoc>
        ///    <para>Moves to the attribute with the specified index.</para>
        /// </devdoc>
        public override void MoveToAttribute(int i) {
            this.readMethod = ReadMethods.MoveToAttribute;
            this.reader.MoveToAttribute(i);
            xpathCollection.Advance(this);

        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.MoveToFirstAttribute"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Moves to the first attribute.
        ///    </para>
        /// </devdoc>
        public override bool MoveToFirstAttribute() {
            bool ret = false;

            ret = this.reader.MoveToFirstAttribute();

            if (ret) {
                xpathCollection.Advance(this);
            }
            return ret;
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.MoveToNextAttribute"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Moves to the next attribute.
        ///    </para>
        /// </devdoc>
        public override bool MoveToNextAttribute() {
            bool ret = false;

            ret = this.reader.MoveToNextAttribute();

            if (ret) {
                xpathCollection.Advance(this);
            }
            return ret;
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.MoveToElement"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Moves to the element that contains the current attribute node.
        ///    </para>
        /// </devdoc>
        public override bool MoveToElement() {
            bool ret = false;

            readMethod = ReadMethods.MoveToElement;

            ret = this.reader.MoveToElement();

            if (ret) {
                xpathCollection.Advance(this);
            }

            return ret;
        }

        // Node Properties
        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.NodeType"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the type of the current node.
        ///    </para>
        /// </devdoc>
        public override XmlNodeType NodeType {
            get {return(this.reader.NodeType);}
        }
        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.Name"]/*' />
        /// <devdoc>
        ///    <para>Gets the name of
        ///       the current node, including the namespace prefix.</para>
        /// </devdoc>
        public override string Name {
            get {return (this.reader.Name);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.LocalName"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the name of the current node without the namespace prefix.
        ///    </para>
        /// </devdoc>
        public override string LocalName {
            get {return (this.reader.LocalName);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.NamespaceURI"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the namespace URN (as defined in the W3C Namespace Specification) of the current namespace scope.
        ///    </para>
        /// </devdoc>
        public override string NamespaceURI {
            get {
                return (this.reader.NamespaceURI);
            }
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.Prefix"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the namespace prefix associated with the current node.
        ///    </para>
        /// </devdoc>
        public override string Prefix {
            get {return (this.reader.Prefix);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.HasValue"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether
        ///    <see cref='System.Xml.XPathReader.Value'/> has a value to return.
        ///    </para>
        /// </devdoc>
        public override bool HasValue {
            get {return (this.reader.HasValue);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.Value"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the text value of the current node.
        ///    </para>
        /// </devdoc>
        public override string Value {
            get {return (this.reader.Value);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.Depth"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the depth of the
        ///       current node in the XML element stack.
        ///    </para>
        /// </devdoc>
        public override int Depth {
            get {return (this.reader.Depth);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.BaseURI"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the base URI of the current node.
        ///    </para>
        /// </devdoc>
        public override string BaseURI {
            get {return (this.reader.BaseURI);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.IsEmptyElement"]/*' />
        /// <devdoc>
        ///    <para>Gets a value indicating whether
        ///       the current
        ///       node is an empty element (for example, &lt;MyElement/&gt;).</para>
        /// </devdoc>
        public override bool IsEmptyElement {
            get {return (this.reader.IsEmptyElement);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.IsDefault"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the current node is an
        ///       attribute that was generated from the default value defined
        ///       in the DTD or schema.
        ///    </para>
        /// </devdoc>
        public override bool IsDefault {
            get {return (this.reader.IsDefault);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.QuoteChar"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the quotation mark character used to enclose the value of an attribute
        ///       node.
        ///    </para>
        /// </devdoc>
        public override char QuoteChar {
            get {return (this.reader.QuoteChar);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XmlSpace"]/*' />
        /// <devdoc>
        ///    <para>Gets the current xml:space scope.</para>
        /// </devdoc>
        public  override XmlSpace XmlSpace {
            get {return (this.reader.XmlSpace);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.XmlLang"]/*' />
        /// <devdoc>
        ///    <para>Gets the current xml:lang scope.</para>
        /// </devdoc>
        public  override string XmlLang {
            get {return (this.reader.XmlLang);}
        }

        // Attribute Accessors
        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.AttributeCount"]/*' />
        /// <devdoc>
        ///    <para> The number of attributes on the current node.</para>
        /// </devdoc>
        public override int AttributeCount {
            get {return (this.reader.AttributeCount);}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.GetAttribute"]/*' />
        /// <devdoc>
        ///    <para>Gets the value of the attribute with the specified
        ///    <see cref='System.Xml.XPathReader.Name'/> .</para>
        /// </devdoc>
        public override string GetAttribute(string name) {
            return this.reader.GetAttribute(name);
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.GetAttribute1"]/*' />
        /// <devdoc>
        ///    <para>Gets the value of the attribute with the
        ///       specified <see cref='System.Xml.XPathReader.LocalName'/> and <see cref='System.Xml.XPathReader.NamespaceURI'/> .</para>
        /// </devdoc>
        public override string GetAttribute(string name, string namespaceURI) {
            return this.reader.GetAttribute(name, namespaceURI);
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.GetAttribute2"]/*' />
        /// <devdoc>
        ///    <para>Gets the value of the attribute with the specified index.</para>
        /// </devdoc>
        public override string GetAttribute(int i) {
            return this.reader.GetAttribute(i);
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.this"]/*' />
        /// <devdoc>
        ///    <para>Gets the value of the attribute with the specified index.</para>
        /// </devdoc>
        public override string this [ int i ] {
            get {return this.reader [i];}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.this1"]/*' />
        /// <devdoc>
        ///    <para>Gets the value of the attribute with the specified
        ///    <see cref='System.Xml.XPathReader.Name'/> .</para>
        /// </devdoc>
        public override string this [ string name ] {
            get {return this.reader[name];}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.this2"]/*' />
        /// <devdoc>
        ///    <para>Gets the value of the attribute with the
        ///       specified <see cref='System.Xml.XPathReader.LocalName'/> and <see cref='System.Xml.XPathReader.NamespaceURI'/> .</para>
        /// </devdoc>
        public override string this [ string name,string namespaceURI ] {
            get {return this.reader[name, namespaceURI];}
        }

        //UE Atention
        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.CanResolveEntity"]/*' />
        public override bool CanResolveEntity  {
            get  {return this.reader.CanResolveEntity;}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.EOF"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       a value indicating whether XmlReader is positioned at the end of the
        ///       stream.
        ///    </para>
        /// </devdoc>
        public override bool EOF {
            get {return this.reader.EOF;}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.Close"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Closes the stream, changes the <see cref='System.Xml.XPathReader.ReadState'/>
        ///       to Closed, and sets all the properties back to zero.
        ///    </para>
        /// </devdoc>
        public override void Close() {
            this.reader.Close();
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.ReadState"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Returns
        ///       the read state of the stream.
        ///    </para>
        /// </devdoc>
        public override ReadState ReadState {
            get {return this.reader.ReadState;}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.ReadString"]/*' />
        /// <devdoc>
        ///    <para>Reads the contents of an element as a string.</para>
        /// </devdoc>
        public override string ReadString() {
            return this.reader.ReadString();
        }


        // Nametable and Namespace Helpers
        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.NameTable"]/*' />
        /// <devdoc>
        ///    <para>Gets the XmlNameTable associated with this
        ///       implementation.</para>
        /// </devdoc>
        public override XmlNameTable NameTable {
            get {return this.reader.NameTable;}
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.LookupNamespace"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Resolves a namespace prefix in the current element's scope.
        ///    </para>
        /// </devdoc>
        public override string LookupNamespace(string prefix) {
            return this.reader.LookupNamespace(prefix);

        }
        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.ResolveEntity"]/*' />
        /// <devdoc>
        ///    <para>Resolves the entity reference for nodes of NodeType EntityReference.</para>
        /// </devdoc>
        public override void ResolveEntity() {
            this.reader.ResolveEntity();
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.ReadAttributeValue"]/*' />
        /// <devdoc>
        ///    <para>Parses the attribute value into one or more Text and/or EntityReference node
        ///       types.</para>
        /// </devdoc>
        public override bool ReadAttributeValue() {
            return this.reader.ReadAttributeValue();
        }

        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.ReadInnerXml"]/*' />
        /// <devdoc>
        ///    <para>Reads all the content (including markup) as a string.</para>
        /// </devdoc>
        public override string ReadInnerXml() {
            return this.reader.ReadInnerXml();
        }
        /// <include file='doc\XPathReader.uex' path='docs/doc[@for="XPathReader.ReadOuterXml"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ReadOuterXml(){
            return this.reader.ReadOuterXml();
        }

        //----------------------------------------------------
        // internal methods
        //

        internal XmlReader BaseReader {
            get { return this.reader; }
        }

        internal int ProcessAttribute {
            get { return this.processAttribute; }
            set { this.processAttribute = value; }
        }

        internal ReadMethods ReadMethod  {
            get { return readMethod; }
        }

        internal bool MapPrefixWithNamespace(string prefix) {

            XmlNamespaceManager nsMgr = xpathCollection.NamespaceManager;

            if (nsMgr != null && nsMgr.LookupNamespace(prefix) == this.NamespaceURI) {
                return true;
            }

            return false;
        }
    }
}