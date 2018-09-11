//------------------------------------------------------------------------------
// <copyright file="Function.cs" company="Microsoft">
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
	using System.Collections;

    internal class Function : AstNode {
        internal enum FunctionType {
            FuncLast = 0,
            FuncPosition,
            FuncCount,
            FuncLocalName,
            FuncNameSpaceUri,
            FuncName,
            FuncString,
            FuncBoolean,
            FuncNumber,
            FuncTrue,
            FuncFalse,
            FuncNot,
            FuncID,            
            FuncConcat,        
            FuncStartsWith,    
            FuncContains,    
            FuncSubstringBefore,
            FuncSubstringAfter,
            FuncSubstring,
            FuncStringLength,
            FuncNormalize,
            FuncTranslate,
            FuncLang,
            FuncSum,
            FuncFloor,
            FuncCeiling,
            FuncRound,
            FuncUserDefined,
            Error
        };

        private FunctionType _functionType = FunctionType.Error;
        private ArrayList _argumentList;

        static private  String[] str = {
            "last()",
            "position()",
            "count()",
            "localname()",
            "namespaceuri()",
            "name()",
            "string()",
            "boolean()", 
            "number()",
            "true()",
            "false()",
            "not()",
            "id()",
            "concat()",
            "starts-with()",
            "contains()",
            "substring-before()",
            "substring-after()",
            "substring()",
            "string-length()",
            "normalize-space()",
            "translate()",
            "lang()",
            "sum()",
            "floor()", 
            "celing()",
            "round()",
        };

        private String _Name = null;
        private String _Prefix = null;

        internal Function(FunctionType ftype, ArrayList argumentList) {
            _functionType = ftype;
            _argumentList = new ArrayList(argumentList);
        }

        internal Function(String prefix, String name, ArrayList argumentList) {
            _functionType = FunctionType.FuncUserDefined;
            _Prefix = prefix;
            _Name = name;
            _argumentList = new ArrayList(argumentList);
        }

        internal Function(FunctionType ftype) {
            _functionType = ftype;
        }

        internal Function(FunctionType ftype, AstNode arg) {
            _functionType = ftype;
            _argumentList = new ArrayList();
            _argumentList.Add(arg);
        }

        internal override QueryType TypeOfAst {
            get {return  QueryType.Function;}
        }

        internal override XPathResultType ReturnType {
            get {
                switch (_functionType) {
                    case FunctionType.FuncLast  : return XPathResultType.Number;
                    case FunctionType.FuncPosition  : return XPathResultType.Number;
                    case FunctionType.FuncCount  : return XPathResultType.Number ;
                    case FunctionType.FuncID : return XPathResultType.NodeSet;
                    case FunctionType.FuncLocalName : return XPathResultType.String;
                    case FunctionType.FuncNameSpaceUri : return XPathResultType.String;
                    case FunctionType.FuncName  : return XPathResultType.String;
                    case FunctionType.FuncString  : return XPathResultType.String;
                    case FunctionType.FuncBoolean : return XPathResultType.Boolean; 
                    case FunctionType.FuncNumber : return XPathResultType.Number;
                    case FunctionType.FuncTrue: return XPathResultType.Boolean;
                    case FunctionType.FuncFalse : return XPathResultType.Boolean; 
                    case FunctionType.FuncNot : return XPathResultType.Boolean;
                    case FunctionType.FuncConcat : return XPathResultType.String;
                    case FunctionType.FuncStartsWith: return XPathResultType.Boolean;
                    case FunctionType.FuncContains : return XPathResultType.Boolean;
                    case FunctionType.FuncSubstringBefore: return XPathResultType.String; 
                    case FunctionType.FuncSubstringAfter  : return XPathResultType.String;
                    case FunctionType.FuncSubstring : return XPathResultType.String;
                    case FunctionType.FuncStringLength : return XPathResultType.Number;
                    case FunctionType.FuncNormalize : return XPathResultType.String;
                    case FunctionType.FuncTranslate : return XPathResultType.String;
                    case FunctionType.FuncLang : return XPathResultType.Boolean;
                    case FunctionType.FuncSum : return XPathResultType.Number;
                    case FunctionType.FuncFloor : return XPathResultType.Number; 
                    case FunctionType.FuncCeiling : return XPathResultType.Number;
                    case FunctionType.FuncRound : return XPathResultType.Number;
                    case FunctionType.FuncUserDefined : return XPathResultType.Error;
                }
                return XPathResultType.Error;
            }
        }

        internal FunctionType TypeOfFunction {
            get {return _functionType;}
        }

        internal ArrayList ArgumentList {
            get {return _argumentList;}
        }

        internal String Prefix {
            get {return _Prefix;}
        }

        internal String Name {
            get {return _functionType == FunctionType.FuncUserDefined ? _Name : str[(int)_functionType];}
        }
    }
}
