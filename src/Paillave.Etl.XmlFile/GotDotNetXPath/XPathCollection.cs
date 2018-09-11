//------------------------------------------------------------------------------
// <copyright file="XPathCollection.cs" company="Microsoft">
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
    using System.Xml;
    using System.Text;
    using System.Collections;
    using System.Diagnostics;
    //
    // Enumerator
    //
    internal sealed class XPathCollectionEnumerator : IEnumerator {

        IDictionaryEnumerator hashEnum;

        public XPathCollectionEnumerator(Hashtable xpathes) {
            hashEnum = xpathes.GetEnumerator();
        }

        public bool MoveNext() {
            return hashEnum.MoveNext();
        }

        public Object Current {
            get { return ((DictionaryEntry)hashEnum.Current).Value;}
        }

        public void Reset() {
            hashEnum.Reset();
        }
    }


    //-----------------------------------------------------------------------------------------
    // The class is the place to associate the xpath string expression
    // and it's compiled query expression

    /// <include file='doc\XPathExpression.uex' path='docs/doc[@for="Array.CreateInstance"]/*' />

    public class XPathQuery{

        string xpath;
        ArrayList compiledXPath;
        int matchIndex = 0;
        int matchCount = 0;
        int [] depthLookup;
        bool matchState = false;
        int treeDepth = -1;
        int key;

        public XPathQuery () {
        }

        // once the xpathexpression is constructed
        // the xpath is compiled into the query format
        public XPathQuery (string xpath) {
            this.xpath = xpath;
            Compile();
        }

        public XPathQuery(string xpath, int depth) {
            this.xpath = xpath;
            this.treeDepth = depth - 1;
            Compile();
        }

        public XPathQuery Clone() {
            XPathQuery clone = new XPathQuery();
            clone.xpath = xpath;
            clone.compiledXPath = compiledXPath;
            clone.depthLookup = depthLookup;
            clone.treeDepth = treeDepth;
            return clone;
        }

        internal int Key {
            set { this.key = value; }
            get { return this.key; }
        }

        //
        // Compile the xpath
        //
        private void Compile() {
            compiledXPath = new ArrayList();
            QueryBuilder builder = new QueryBuilder();

            //
            // Need to set the query with current reader heigth;
            //
            builder.Build(xpath, compiledXPath, this.treeDepth);

            // Index is 0 based , but the count is 1 based
            // plus the null query we added.
            int lookupLength = ((BaseAxisQuery)compiledXPath[compiledXPath.Count-2]).Depth + 1;

            depthLookup = new int[lookupLength];

            //exclude the null query
            for (int i = 0; i < compiledXPath.Count-1; ++i) {

                if (depthLookup[((BaseAxisQuery)compiledXPath[i]).Depth] == 0 ) {
                    depthLookup[((BaseAxisQuery)compiledXPath[i]).Depth] = i;
                }
            }
        }

        public string XPath {
            get { return xpath; }
            set { xpath = value; }
        }

        ///
        /// use can store this compiled expression to query other documents
        /// <include file='doc\XPathExpression.uex' path='docs/doc[@for="Array.CreateInstance"]/*' />
        public ArrayList GetXPathQueries {
            get { return compiledXPath; }
        }

        public override string ToString() {
            return xpath;
        }

        //
        // report if the current query is matched
        //
        internal bool Match()
        {
#if DEBUG1
            Console.WriteLine("GetState: Query: {0}, matchState {1}", xpath, matchState);
#endif
            return matchState;
        }


        //
        // Look if the current query match what we have found
        // Two conditions to evaluate a match
        // 1). matchingIndex points to the end of query
        // 2). the depth of the tree matches the depth of the query

        internal void SetMatchState(XPathReader reader)
        {

            if (matchIndex < 1)
            {
                return;
            }

            int queryCount = compiledXPath.Count - 1; // take out the null query;
            int queryDepth = ((BaseAxisQuery)compiledXPath[matchIndex - 1]).Depth;

            if (matchCount == queryCount && queryDepth == reader.Depth)
            {
                matchState = true;
            }

#if DEBUG1
            Console.WriteLine("\nMatch query: {0}", xpath);
            Console.WriteLine("MatchCount {0}, query count{1}", matchCount, queryCount);
            Console.WriteLine("reader depth {0}, queryDepth{1}", matchCount, queryCount);
            Console.WriteLine("Name: {0}", reader.Name);
            Console.WriteLine("matchState {0}", matchState);
#endif
        }


        //
        //check if the current processing query is attribute query
        //
        internal bool IsAttributeQuery() {
            if (compiledXPath[matchIndex] is AttributeQuery) {
                return true;
            }
            return false;
        }

        //
        // Reset the matching index if the reader move to the
        // node depth less than the expected depth
        //
        internal void ResetMatching(XPathReader reader) {

            matchState = false;

            // reset the matching index
            int count = compiledXPath.Count;

            if (reader.Depth < ((BaseAxisQuery)compiledXPath[matchIndex]).Depth) {
                matchIndex = depthLookup[reader.Depth];
                matchCount = matchIndex + 1;
            }

            if (matchCount == count - 1 && matchIndex > 0) {
                --matchCount;
                --matchIndex;
            }

#if DEBUG1
            Console.WriteLine("\nResetMatchingIndex: {0}\n", xpath);
            Console.WriteLine("matchIndex: {0}, matchCount {1}, compiledXPath.Count {2}", matchIndex, matchCount, count);
#endif
        }


        // In the query expression, we need to be
        // store the depth of tree that we have treversed
        // we shouldn't move the reader, if we move to the
        // an attribute, we need to move it back

        internal void Advance(XPathReader reader) {

            ResetMatching(reader);


            if ((IQuery)compiledXPath[matchIndex] is DescendantQuery) {
                //look through the subtree for the node is
                //looking for
                if (((IQuery)compiledXPath[matchIndex+1]).MatchNode(reader)){
                    //found the node that we were looking for
                        matchIndex = matchIndex + 2;
                        matchCount = matchIndex;

                        //set the expected depth for the rest of query
                        for (int i = matchCount; i < compiledXPath.Count - 1; ++i) {
                            ((BaseAxisQuery)compiledXPath[matchIndex]).Depth += reader.Depth -1;
                        }
                }
            }
            else {

                while (reader.Depth == ((BaseAxisQuery)compiledXPath[matchIndex]).Depth) {

                    if (((IQuery)compiledXPath[matchIndex]).MatchNode(reader)){

                        ++matchIndex;
                        matchCount = matchIndex;
                    }
                    else {
                        //--matchIndex;
                        break;
                    }
                }

                SetMatchState(reader);
            }
        }

        internal void AdvanceUntil(XPathReader reader) {

            Advance(reader);

            if (compiledXPath[matchIndex] is AttributeQuery) {
                reader.ProcessAttribute = reader.Depth + 1; // the attribute depth should be current element plus one
            }
        }
    }


    //XPathCollection class
    public class XPathCollection : ICollection {

        Hashtable xpathes;
        int processCount = 0;
        XPathReader reader;
        int key = 0; // number of xpathes added into collection as keys
        XmlNamespaceManager nsManager;

        public XPathCollection() {
            xpathes = new Hashtable();
        }

        public XPathCollection(XmlNamespaceManager nsManager) : this() {
            this.nsManager = nsManager;
        }

        internal XPathReader SetReader {
            set { this.reader = value; }
        }

        internal int ProcessCount  {
            get { return this.processCount; }
            set { this.processCount = value; }
        }

        public XmlNamespaceManager NamespaceManager {
            set { this.nsManager = value; }
            get { return this.nsManager; }
        }

        public void CopyTo(Array array, int index) {
        }

        //
        // Add matched query index into an array list
        //
        internal bool MatchesAny(ArrayList list, int depth) {
            bool ret = false;

            if (list == null) {
                throw new ArgumentException();
            }

            list.Clear();

            foreach (XPathQuery expr in this){
                if (expr.Match()) {
                   list.Add(expr.Key);
                   ret = true;
                }
            }
            return ret;
        }

        //
        // If the current processing query is attribute
        // query Read will not Read, it will MoveToAttribute
        // instead.
        //
        internal bool CurrentContainAttributeQuery() {
            bool ret = false;
            foreach (XPathQuery expr in this){
                if(expr.IsAttributeQuery()) {
                   ret = true;
                   break;
               }
            }

            return ret;
        }

        //
        // walk through all the queries to see
        // if any query matches with the current
        // read
        //
        internal void Advance(XPathReader reader) {

            foreach (XPathQuery expr in this) {
                expr.Advance(reader);
            }
        }

        //
        // walk through all the queries to see
        // if any query matches with the current
        // read
        //
        internal void AdvanceUntil(XPathReader reader) {

            foreach (XPathQuery expr in this) {
                expr.AdvanceUntil(reader);
            }

            if (!CurrentContainAttributeQuery()) {
                reader.ProcessAttribute = -1;
            }
        }

        //
        // Look throw all the query list
        // to see if any query has reach the end
        // in MatchAnyQuery, we need to move to
        // attribute as well we there is a query
        //
        //
        internal bool MatchAnyQuery() {

            foreach (XPathQuery expr in this) {
                if (expr.Match()) {
                   return true;
               }
            }
            return false;
        }

        //
        // check if a expression contains in the collection
        //
        public bool Contains(XPathQuery expr) {
            return xpathes.ContainsValue(expr);
        }

        public bool Contains(string xpath) {
            bool ret = false;

            foreach (XPathQuery xpathexpr in xpathes) {
                if (xpathexpr.ToString() == xpath) {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        public int Add(string xpath) {
            XPathQuery xpathexpr;

            if (reader != null) {
                xpathexpr = new XPathQuery(xpath, reader.Depth);
                if (reader.ReadState == ReadState.Interactive) {
                    xpathexpr.Advance(this.reader);
                }
            }
            else {
                xpathexpr = new XPathQuery(xpath);
            }

            xpathexpr.Key = key;

            xpathes.Add(key++, xpathexpr);

            return (key - 1);
        }

        public int Add(XPathQuery xpathexpr) {
            xpathexpr.Key = key;
            xpathes.Add(key++, xpathexpr);
            return (key - 1);
        }

        public XPathQuery this[int index] {
            get { return (XPathQuery)xpathes[index]; }
        }

        public int Count {
            get {return xpathes.Count;}
        }

        public Object SyncRoot {
            get {return this;}
        }

        public bool IsSynchronized {
            get {return false;}
        }

        //IEnumerable interface
        public IEnumerator GetEnumerator() {
            return (new XPathCollectionEnumerator(xpathes));
        }

        public void Clear() {
            xpathes.Clear();
        }

        public bool IsReadOnly {
            get { return false; }
        }


        public bool IsFixedSize {
            get { return false; }
        }

        public void Remove(XPathQuery xpathexpr) {
            xpathes.Remove(xpathexpr.Key);
        }

        public void Remove(string xpath) {

            foreach (XPathQuery xpathexpr in xpathes) {
                if (xpathexpr.ToString() == xpath) {
                    Remove(xpathexpr.Key);
                }
            }
        }

        public void Remove(int index) {
            xpathes.Remove(index);
        }
    }
}
