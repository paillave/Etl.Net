//------------------------------------------------------------------------------
// <copyright file="XPathNodeType.cs" company="Microsoft">
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
/*
    
    public enum XPathNodeType {
        
        Root                    = (int)XmlNodeType.Document,
        
        Element                 = (int)XmlNodeType.Element,
        
        Attribute               = (int)XmlNodeType.Attribute,
        Namespace               = 1931,
       
        Text                    = (int)XmlNodeType.Text,
        
        SignificantWhitespace   = (int)XmlNodeType.SignificantWhitespace,
        
        Whitespace              = (int)XmlNodeType.Whitespace,
        
        ProcessingInstruction   = (int)XmlNodeType.ProcessingInstruction,
        
        Comment                 = (int)XmlNodeType.Comment,
        
        All                     = (int)XmlNodeType.All
    }
*/
    
    public enum XPathNodeType {
        
        Root,
        
        Element,
        
        Attribute,
        
        Namespace,
        
        Text,
        
        SignificantWhitespace,
        
        Whitespace,
        
        ProcessingInstruction,
        
        Comment,
        
        All,
    }
}
