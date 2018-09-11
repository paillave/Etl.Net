//------------------------------------------------------------------------------
// <copyright file="AstNode.cs" company="Microsoft">
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
    using System.Xml.XPath;
    using System.Diagnostics;

    internal  class   AstNode 
    {
        internal enum QueryType
        {
            Axis            ,
            Operator        ,
            Filter          ,
            ConstantOperand ,
            Function        ,
            Group           ,
            Root            ,
            Variable        ,        
            Error           
        };


        static internal AstNode NewAstNode(String parsestring)
        {
            try {
                return (XPathParser.ParseXPathExpresion(parsestring));
            }
            catch (XPathException e)
            {
                Debug.WriteLine(e.Message);
            }
            return null;
        }
        
        internal virtual  QueryType TypeOfAst {  
            get {return QueryType.Error;}
        }
        
        internal virtual  XPathResultType ReturnType {
            get {return XPathResultType.Error;}
        }

        internal virtual double DefaultPriority {
            get {return 0.5;}
        }
    }
}
