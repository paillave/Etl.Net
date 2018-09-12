//------------------------------------------------------------------------------
// <copyright file="Operator.cs" company="Microsoft">
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
	internal class Operator : AstNode {
        internal enum Op {
            PLUS = 1,
            MINUS = 2,
            MUL  = 3 ,
            MOD = 4,
            DIV = 5,
            NEGATE = 6,
            LT = 7,
            GT = 8,
            LE = 9,
            GE = 10,
            EQ = 11,
            NE = 12,
            OR = 13,
            AND = 14,
            UNION = 15,
            INVALID
        };

        private String[] str = {
            "+",
            "-",
            "multiply",
            "mod",
            "divde",
            "negate",
            "<",
            ">",
            "<=",
            ">=",
            "=",
            "!=", 
            "or",
            "and",
            "union"
        };

        private Op _operatorType;
        private AstNode _opnd1;
        private AstNode _opnd2;

        internal Operator(Op op, AstNode opnd1, AstNode opnd2) {
            _operatorType = op;    
            _opnd1 = opnd1;
            _opnd2 = opnd2;
        }

        internal override QueryType TypeOfAst {
            get {return  QueryType.Operator;}
        }

        internal override XPathResultType ReturnType {
            get {
                if (_operatorType < Op.LT)
                    return XPathResultType.Number;
                if (_operatorType < Op.UNION)
                    return XPathResultType.Boolean;
                return XPathResultType.NodeSet;
            }
        }

        internal Op OperatorType {
            get { return _operatorType;}
        }

        internal AstNode Operand1
        {
            get {return _opnd1;}
        }

        internal AstNode Operand2
        {
            get {return _opnd2;}
        }

        internal String OperatorTypeName {
            get {return str[(int)_operatorType-1];}
        }

        internal override double DefaultPriority {
            get {
                if (_operatorType == Op.UNION) {
                    double pri1 = _opnd1.DefaultPriority;
                    double pri2 = _opnd2.DefaultPriority;

                    if (pri1 > pri2)
                        return pri1;
                    return pri2;
                }
                else
                    return 0.5;
            }

        }


    }
}
