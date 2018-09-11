//------------------------------------------------------------------------------
// <copyright file="XPathException.cs" company="Microsoft">
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
	using FT = Function.FunctionType;

	internal class QueryBuilder {

        public QueryBuilder () {
        }


        //public enum XmlNodeType {
        //    None,
        //    Element,
        //    Attribute,
        //    Text,
        //    CDATA,
        //    EntityReference,
        //    Entity,
        //    ProcessingInstruction,
        //    Comment,
        //    Document,
        //    DocumentType,
        //    DocumentFragment,
        //    Notation,
        //    Whitespace,
        //    SignificantWhitespace,
        //    EndElement,
        //    EndEntity,
        //    XmlDeclaration
        //}

        //public enum XPathNodeType {
        //    Root,
        //    Element,
        //    Attribute,
        //    Namespace,
        //    Text,
        //    SignificantWhitespace,
        //    Whitespace,
        //    ProcessingInstruction,
        //    Comment,
        //    All,
        //}

        // xpath defines its own NodeType
        // it should use the XmlNodeType instead
        // we just map between them for now
        // so that the construct query will have
        // the XmlNodeType instead of XPathNodeType

        XmlNodeType MapNodeType(XPathNodeType type) {

            XmlNodeType ret = XmlNodeType.None;
            switch (type) {
                case XPathNodeType.Element:
                    ret = XmlNodeType.Element;
                    break;
                case XPathNodeType.Attribute:
                    ret = XmlNodeType.Attribute;
                    break;
                case XPathNodeType.Text:
                    ret = XmlNodeType.Text;
                    break;
                case XPathNodeType.ProcessingInstruction:
                    ret = XmlNodeType.ProcessingInstruction;
                    break;
                case XPathNodeType.Comment:
                    ret = XmlNodeType.Comment;
                    break;
                default:
                    break;
            }

            return ret;
        }

        IQuery ProcessFilter(Filter root) {

            //condition
            IQuery opnd = ProcessNode(root.Condition, null);

            //axis
            IQuery qyInput = ProcessNode(root.Input, null);

            return new FilterQuery(qyInput, opnd);
        }

        IQuery ProcessOperand(Operand root) {
            return new OperandQuery(root.OperandValue, root.ReturnType);
        }

        IQuery ProcessFunction(Function root, IQuery qyInput) {

            IQuery qy = null;

            switch (root.TypeOfFunction) {

                case FT.FuncPosition :
                    qy =  new MethodOperand(null, root.TypeOfFunction);
                    return qy;

                // we should be able to count how many attributes
                case FT.FuncCount :
                    qy = ProcessNode((AstNode)(root.ArgumentList[0]), null);

                    if (qy is AttributeQuery) {
                        return new MethodOperand(qy, FT.FuncCount);
                    }
                    //none attribute count funciton result in error.

                    break;

                case FT.FuncLocalName :
                case FT.FuncNameSpaceUri :
                case FT.FuncName :
                    if (root.ArgumentList != null && root.ArgumentList.Count > 0) {
                        return new MethodOperand( ProcessNode((AstNode)(root.ArgumentList[0]),null),
                                                  root.TypeOfFunction);
                     } else{
                        return new MethodOperand(null, root.TypeOfFunction);
                    }

                case FT.FuncString:
                case FT.FuncConcat:
                case FT.FuncStartsWith:
                case FT.FuncContains:
                case FT.FuncSubstringBefore:
                case FT.FuncSubstringAfter:
                case FT.FuncSubstring:
                case FT.FuncStringLength:
                case FT.FuncNormalize:
                case FT.FuncTranslate:
                    ArrayList ArgList = null;
                    if (root.ArgumentList != null) {
                        int count = 0;
                        ArgList = new ArrayList();
                        while (count < root.ArgumentList.Count)
                            ArgList.Add(ProcessNode((AstNode)root.ArgumentList[count++], null));
                    }
                    return new StringFunctions(ArgList, root.TypeOfFunction);

                case FT.FuncNumber:
                //case FT.FuncSum:
                case FT.FuncFloor:
                case FT.FuncCeiling:
                case FT.FuncRound:
                    if (root.ArgumentList != null) {
                        return new NumberFunctions(ProcessNode((AstNode)root.ArgumentList[0], null),
                                                   root.TypeOfFunction);
                    } else {
                        return new NumberFunctions(null);
                    }
                    
                case FT.FuncTrue:
                case FT.FuncFalse:
                    return new BooleanFunctions(null, root.TypeOfFunction);

                case FT.FuncNot:
                case FT.FuncLang:
                case FT.FuncBoolean:
                    return new BooleanFunctions(ProcessNode((AstNode)root.ArgumentList[0], null),
                                                root.TypeOfFunction);


                // Unsupport functions
                case FT.FuncID :

                // Last Function is not supported, because we don't know
                // how many we get in the list
                // <Root> <e a="1"/> <e a="2"/></Root>
                // /Root/e[last()=2]
                // we will not return the first one because
                // we don't if we have two e elements.
                case FT.FuncLast :
                    //qy = new MethodOperand(null, root.TypeOfFunction);
                    //return qy;

                default :
                    throw new XPathReaderException("The XPath query is not supported.");
            }

            return null;
        }



        //
        // Operator: Or, and, |
        //           +, -, *, div,
        //           >, >=, <, <=, =, !=
        //
        private IQuery ProcessOperator(Operator root, IQuery qyInput) {

            IQuery ret = null;

            switch (root.OperatorType) {

                case Operator.Op.OR:
                    ret = new OrExpr(ProcessNode(root.Operand1, null),
                                      ProcessNode(root.Operand2, null));
                    return ret;

                case Operator.Op.AND :
                    ret = new AndExpr(ProcessNode(root.Operand1, null),
                                       ProcessNode(root.Operand2, null));
                    return ret;
            }

            switch (root.ReturnType) {

                case XPathResultType.Number:
                    ret = new NumericExpr(root.OperatorType,
                                           ProcessNode(root.Operand1, null),
                                           ProcessNode(root.Operand2, null));
                    return ret;

                case XPathResultType.Boolean:
                    ret = new LogicalExpr(root.OperatorType,
                                           ProcessNode(root.Operand1, null),
                                           ProcessNode(root.Operand2, null));
                    return ret;
            }

            return ret;
        }

        //
        ///
        ///
        private IQuery ProcessAxis(Axis root , IQuery qyInput) {
            IQuery result = null;

            switch (root.TypeOfAxis) {

                case Axis.AxisType.Attribute:
                    result = new AttributeQuery(qyInput, root.Name, root.Prefix, root.Type);
                    break;

                case Axis.AxisType.Self:
                    result = new XPathSelfQuery(qyInput,  root.Name, root.Prefix, root.Type);
                    break;

                case Axis.AxisType.Child:
                    result = new ChildQuery( qyInput, root.Name, root.Prefix, root.Type);
                    break;

                case Axis.AxisType.Descendant:
                case Axis.AxisType.DescendantOrSelf:
                    result = new DescendantQuery(qyInput, root.Name, root.Prefix, root.Type);
                    break;

                default:
                    throw new XPathReaderException("xpath is not supported!"); 
            }

            return result;
        }

        private IQuery ProcessNode(AstNode root, IQuery qyInput) {
            IQuery result = null;

            if (root == null)
                return null;

            switch (root.TypeOfAst) {

                case AstNode.QueryType.Axis:
                    Axis axis = (Axis)root;
                    result = ProcessAxis(axis, ProcessNode(axis.Input, qyInput));
                    break;

                case AstNode.QueryType.Operator:
                    result = ProcessOperator((Operator)root, null);
                    break;

                case AstNode.QueryType.Filter:
                    result = ProcessFilter((Filter)root);
                    break;

                case AstNode.QueryType.ConstantOperand:
                    result = ProcessOperand((Operand)root);
                    break;

                case AstNode.QueryType.Function:
                    result = ProcessFunction((Function)root, qyInput);
                    break;

                case AstNode.QueryType.Root:
                    result = new AbsoluteQuery();
                    break;

                case AstNode.QueryType.Group:
                    result = new GroupQuery(ProcessNode(((Group)root).GroupNode, qyInput));
                    break;
                default:
                    Debug.Assert(false, "Unknown QueryType encountered!!");
                    break;
            }
            return result;
        }

        public void Build(string xpath, ArrayList compiledXPath, int depth) {
            IQuery query;
            //
            // build the AST node first
            //
            AstNode root = XPathParser.ParseXPathExpresion(xpath);

            Stack stack = new Stack();

            query = ProcessNode(root, null);

            while (query != null) {
                if (query is BaseAxisQuery) {
                    stack.Push(query);
                    query = ((BaseAxisQuery)query).QueryInput;

                }else {
                    // these queries are not supported
                    // for example, the primary exprission not in the predicate.
                    throw new XPathReaderException("XPath query is not supported!");
                }
            }

            query = (IQuery)stack.Peek();

            if (query is AbsoluteQuery) { //AbsoluteQuery at root means nothing. Throw it away.
                stack.Pop();
            }

            // reverse the query
            // compute the query depth table
            while(stack.Count > 0) {
                compiledXPath.Add(stack.Pop());
                BaseAxisQuery currentQuery = (BaseAxisQuery) compiledXPath[compiledXPath.Count - 1];

                FilterQuery filterQuery = null;

                if (currentQuery is FilterQuery) {
                    filterQuery = (FilterQuery)currentQuery;
                    currentQuery = ((FilterQuery)currentQuery).Axis;
                }

                if (currentQuery is ChildQuery || currentQuery is AttributeQuery || currentQuery is DescendantQuery) {
                    ++depth;
                } else if (currentQuery is AbsoluteQuery) {
                    depth = 0;
                }

                currentQuery.Depth = depth;

                if (filterQuery != null) {
                    filterQuery.Depth = depth;
                }

            }

            //
            // matchIndex always point to the next query to match.
            // We use the matchIndex to retriev the query depth info,
            // without this added Null query, we need to check the
            // condition all the time in the Expression Advance method.
            //
            compiledXPath.Add(new NullQuery());
        }
    }
}