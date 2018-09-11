//------------------------------------------------------------------------------
// <copyright file="XPathExpr.cs" company="Microsoft">
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
    using System.Xml;
    using System.Collections;

     /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlSortOrder"]/*' />
     /// <devdoc>
     ///    <para>[To be supplied.]</para>
     /// </devdoc>
    public enum XmlSortOrder {
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlSortOrder.Ascending"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Ascending       = 1,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlSortOrder.Descending"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Descending      = 2,
    }

     /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlCaseOrder"]/*' />
     /// <devdoc>
     ///    <para>[To be supplied.]</para>
     /// </devdoc>
    public enum XmlCaseOrder {
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlCaseOrder.None"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        None            = 0,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlCaseOrder.UpperFirst"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        UpperFirst      = 1,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlCaseOrder.LowerFirst"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        LowerFirst      = 2,
    }

     /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlDataType"]/*' />
     /// <devdoc>
     ///    <para>[To be supplied.]</para>
     /// </devdoc>    
    public enum XmlDataType {
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlDataType.Text"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Text            = 1,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XmlDataType.Number"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Number          = 2,
    }

     /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType"]/*' />
     /// <devdoc>
     ///    <para>[To be supplied.]</para>
     /// </devdoc>
    public enum XPathResultType {
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType.String"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Number         = 0 ,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType.Boolean"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        String          = 1,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType.Number"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Boolean         = 2,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType.Navigator"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        NodeSet        = 3,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType.NodeSet"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Navigator       = XPathResultType.String,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType.Any"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
         Any            = 5,
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathNodeType.Error"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        Error
    };

     /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathExpression"]/*' />
     /// <devdoc>
     ///    <para>[To be supplied.]</para>
     /// </devdoc>
     /// 
	/*
    public abstract class XPathExpression {
        //Added for UE
        internal XPathExpression(){}
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathExpression.Expressiom"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        public  abstract string Expression { get; }
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathExpression.AddSort"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        public abstract void AddSort(object expr, IComparer comparer);
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathExpression.AddSort1"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        public abstract void AddSort(object expr, XmlSortOrder order,
                    XmlCaseOrder caseOrder, string lang, XmlDataType dataType);
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathExpression.Clone"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        public abstract XPathExpression Clone();
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathExpression.SetContext"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        public abstract void SetContext(XmlNamespaceManager nsManager);
         /// <include file='doc\XPathExpr.uex' path='docs/doc[@for="XPathExpression.ReturnType"]/*' />
         /// <devdoc>
         ///    <para>[To be supplied.]</para>
         /// </devdoc>
        public abstract XPathResultType ReturnType { get; }
    }
	*/
}
