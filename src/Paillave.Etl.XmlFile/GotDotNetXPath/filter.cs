//------------------------------------------------------------------------------
// <copyright file="Filter.cs" company="Microsoft">
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
	internal class Filter : AstNode {
        private AstNode _input;
        private AstNode _condition;

        internal Filter( AstNode input, AstNode condition) {
            _input = input;
            _condition = condition;
        }

        internal override QueryType TypeOfAst {
            get {return  QueryType.Filter;}
        }

        internal override XPathResultType ReturnType {
            get {return XPathResultType.NodeSet;}
        }

        internal AstNode Input {
            get { return _input;}
        }

        internal AstNode Condition {
            get {return _condition;}
        }
    }
}
