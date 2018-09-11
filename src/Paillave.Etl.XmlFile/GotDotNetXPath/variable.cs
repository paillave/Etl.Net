//------------------------------------------------------------------------------
// <copyright file="Variable.cs" company="Microsoft">
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
	using System.Diagnostics;

    internal class Variable : AstNode {
        private String _Localname;
	    private String _Prefix = String.Empty;

        internal Variable(String name, String prefix) {
            _Localname = name;
	        _Prefix = prefix;
        }

        internal override QueryType TypeOfAst {
            get {return QueryType.Variable;}
        }

        internal override XPathResultType ReturnType {
            get {return XPathResultType.Error;}
        }

        internal String Name {
            get {
			    if( Prefix != String.Empty ) {
                    return _Prefix + ":" + _Localname;
			    }
			    else {
				    return _Localname;
				    }
			    }
        }
        
        internal String Localname {
            get {return _Localname;}
        }

       	internal String Prefix {
            get {return _Prefix;}
        }
    }
}
