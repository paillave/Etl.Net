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

     
     /// <summary>
     ///    [To be supplied.]
     /// </summary>
    public enum XmlSortOrder {
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Ascending       = 1,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Descending      = 2,
    }

     
     /// <summary>
     ///    [To be supplied.]
     /// </summary>
    public enum XmlCaseOrder {
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        None            = 0,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        UpperFirst      = 1,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        LowerFirst      = 2,
    }

     
     /// <summary>
     ///    [To be supplied.]
     /// </summary>    
    public enum XmlDataType {
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Text            = 1,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Number          = 2,
    }

     
     /// <summary>
     ///    [To be supplied.]
     /// </summary>
    public enum XPathResultType {
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Number         = 0 ,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        String          = 1,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Boolean         = 2,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        NodeSet        = 3,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Navigator       = XPathResultType.String,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
         Any            = 5,
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        Error
    };

     
     /// <summary>
     ///    [To be supplied.]
     /// </summary>
     /// 
	/*
    public abstract class XPathExpression {
        //Added for UE
        internal XPathExpression(){}
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        public  abstract string Expression { get; }
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        public abstract void AddSort(object expr, IComparer comparer);
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        public abstract void AddSort(object expr, XmlSortOrder order,
                    XmlCaseOrder caseOrder, string lang, XmlDataType dataType);
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        public abstract XPathExpression Clone();
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        public abstract void SetContext(XmlNamespaceManager nsManager);
         
         /// <summary>
         ///    [To be supplied.]
         /// </summary>
        public abstract XPathResultType ReturnType { get; }
    }
	*/
}
