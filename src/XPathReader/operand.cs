//------------------------------------------------------------------------------
// <copyright file="Operand.cs" company="Microsoft">
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

    internal class Operand : AstNode {
        private object _var;
	    private String _prefix = String.Empty;
        private XPathResultType _type;

        internal Operand(String var) {
            _var = var;
            _type = XPathResultType.String;
        }

        internal Operand(double var) {
            _var = var;
            _type = XPathResultType.Number;
        }

        internal Operand(bool var) {
            _var = var;
            _type = XPathResultType.Boolean;
        }

        internal override QueryType TypeOfAst {
            get {return QueryType.ConstantOperand;}
        }

        internal override XPathResultType ReturnType {
            get {return _type;}
        }

        internal String OperandType {
            get {
                switch (_type) {
                    case XPathResultType.Number : return "number";
                    case XPathResultType.String : return "string";
                    case XPathResultType.Boolean : return "boolean";
                }
                return null;
            }
        }

        internal object OperandValue {
            get {return _var;}
        }

       	internal String Prefix {
            get {return _prefix;}
        }
    }
}
