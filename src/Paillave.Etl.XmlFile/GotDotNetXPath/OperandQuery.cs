//------------------------------------------------------------------------------
// <copyright file="OperandQuery.cs" company="Microsoft">
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
	using FT = Function.FunctionType;
	//
    // Function which takes operand
    //
    internal sealed class MethodOperand : IQuery {
        IQuery opnd = null;
        FT funcType;

        internal MethodOperand(IQuery opnd, FT funcType){
            this.funcType = funcType;
            this.opnd = opnd;
        }

        //
        // The function context node has to be the axis selected node
        //  <E a='1' xmlns='test'> <E1/> </E>
        //
        //  /E/E1[namespaceuri(../E)= 'test']
        internal override object GetValue(XPathReader reader) {

            object ret = null;

            // the Opnd must be attribute, otherwise it will be in error
            switch (this.funcType) {
                case FT.FuncCount:
                    ret = reader.AttributeCount;
                    break;

                case FT.FuncPosition:
                    //we need to go back to the fileter query to get count
                    ret = base.PositionCount;
                    break;

                case FT.FuncNameSpaceUri:
                    ret = reader.NamespaceURI;
                    break;

                case FT.FuncLocalName:
                    ret = reader.LocalName;
                    break;

                case FT.FuncName :
                    ret = reader.Name;
                    break;
            }

            return ret;
        }

        internal override XPathResultType ReturnType() {
            if (this.funcType <= FT.FuncCount)
                return XPathResultType.Number;

            return XPathResultType.String;
        }
    }

    //
    // The leaf node for the expression
    //
    internal sealed class OperandQuery : IQuery {
        private object variable;
        private XPathResultType type;

        internal OperandQuery(object var, XPathResultType type)
        {
            this.variable = var;
            this.type = type;
        }

        internal override object GetValue(XPathReader reader)
        {
            return (this.variable);
        }

        internal override XPathResultType ReturnType()
        {
            return this.type;
        }

    }
}
