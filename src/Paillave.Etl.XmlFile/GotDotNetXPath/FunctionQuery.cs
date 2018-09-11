//------------------------------------------------------------------------------
// <copyright file="FunctionQuery.cs" company="Microsoft">
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
	using FT = Function.FunctionType;

    internal sealed class StringFunctions : IQuery {
        private ArrayList argList;

		FT funcType;

        public StringFunctions() {
        }

        public StringFunctions(ArrayList qy, FT funcType){
            this.argList = qy;
            this.funcType = funcType;
        }

        internal override object GetValue(XPathReader reader) {

            object obj = new object();

            switch (this.funcType) {
                case FT.FuncString :
                    obj = toString(reader);
                    break;

                case FT.FuncConcat :
                    obj = Concat(reader);
                    break;

                case FT.FuncStartsWith :
                    obj = Startswith(reader);
                    break;

                case FT.FuncContains :
                    obj = Contains(reader);
                    break;

                case FT.FuncSubstringBefore :
                    obj = Substringbefore(reader);
                    break;

                case FT.FuncSubstringAfter :
                    obj = Substringafter(reader);
                    break;

                case FT.FuncSubstring :
                    obj = Substring(reader);
                    break;

                case FT.FuncStringLength :
                    obj = StringLength(reader);
                    break;

                case FT.FuncNormalize :
                    obj = Normalize(reader);
                    break;

                case FT.FuncTranslate :
                    obj = Translate(reader);
                    break;
            }

            return obj;
        }

        internal override XPathResultType ReturnType() {
            if (this.funcType == FT.FuncStringLength)
                return XPathResultType.Number;
            if (this.funcType == FT.FuncStartsWith  ||
                this.funcType == FT.FuncContains)
                return XPathResultType.Boolean;
            return XPathResultType.String;
        }


        private static String toString(double num) {
            return Convert.ToString(num);
        }

        private static String toString(Boolean b) {
            if (b)
                return "true";
            return "false";
        }


        //
        // string string(object?)
        // object
        // node-set: string value of the first node
        //           node-set = null, String.Empty return
        // number: NaN -> "NaN"
        //         +0->"0", -0->"0",
        //         +infinity -> "Infinity" -infinity -> "Infinity"
        // boolean: true -> "ture" false -> "false"
        //
        // Example: <Root><e a1='1' a2='2'/>text1</e>
        //                <e a='12'> text2</e>
        //          </Root>
        // /Root/e[string(self::node())="text"]
        // /Root/e[string(attribute::node())='1']
        // /Root[string(/e)="text"]

        private String toString(XPathReader reader) {

            if (this.argList != null && this.argList.Count > 0) {
                IQuery query = (IQuery) this.argList[0];

                object obj = query.GetValue(reader);

                if (obj == null) {
                    return String.Empty;
                }

                return obj.ToString();
            }
            return String.Empty;
        }

        String Concat(XPathReader reader) {
            int count = 0;
            StringBuilder s = new StringBuilder();
            while (count < this.argList.Count)
                s.Append(((IQuery)this.argList[count++]).GetValue(reader).ToString());
            return s.ToString();
        }

        Boolean Startswith(XPathReader reader) {
            String str1 = ((IQuery)this.argList[0]).GetValue(reader).ToString();
            String str2 = ((IQuery)this.argList[1]).GetValue(reader).ToString();

            return str1.StartsWith(str2);
        }

        Boolean Contains(XPathReader reader) {
            String str1 = ((IQuery)this.argList[0]).GetValue(reader).ToString();
            String str2 = ((IQuery)this.argList[1]).GetValue(reader).ToString();
            int index = str1.IndexOf(str2);
            if (index != -1)
                return true;
            return false;
        }

        String Substringbefore(XPathReader reader) {
            String str1 = ((IQuery)this.argList[0]).GetValue(reader).ToString();
            String str2 = ((IQuery)this.argList[1]).GetValue(reader).ToString();
            int index = str1.IndexOf(str2);
            if (index != -1)
                return str1.Substring(0,index);
            else
                return String.Empty;
        }

        String Substringafter(XPathReader reader) {
            String str1 = ((IQuery)this.argList[0]).GetValue(reader).ToString();
            String str2 = ((IQuery)this.argList[1]).GetValue(reader).ToString();
            int index = str1.IndexOf(str2);
            if (index != -1)
                return str1.Substring(index+str2.Length);
            else
                return String.Empty;
        }

        String Substring(XPathReader reader) {
            String str1 = ((IQuery)this.argList[0]).GetValue(reader).ToString();
            double num = Math.Round(Convert.ToDouble(((IQuery)this.argList[1]).GetValue(reader))) - 1;
            if (double.IsNaN(num))
                return String.Empty;
            if (this.argList.Count == 3) {
                double num1 = Math.Round(Convert.ToDouble(((IQuery)this.argList[2]).GetValue(reader))) ;
                if (double.IsNaN(num1))
                    return String.Empty;
                if (num < 0) {
                    num1 = num + num1;
                    if (num1 <= 0)
                        return String.Empty;
                    num = 0;
                }
                double maxlength = str1.Length - num;
                if (num1 > maxlength)
                    num1 = maxlength;
                return str1.Substring((int)num,(int)num1);
            }
            if (num < 0)
                num = 0;
            return str1.Substring((int)num);
        }

        Double StringLength(XPathReader reader) {
            if (this.argList != null && this.argList.Count > 0) {
                return((IQuery)this.argList[0]).GetValue(reader).ToString().Length;
            }
            return 0;
        }

        String Normalize(XPathReader reader) {
            String str1;
            if (this.argList != null && this.argList.Count > 0)
                str1 = ((IQuery)this.argList[0]).GetValue(reader).ToString().Trim();
            else
                str1 = String.Empty;
            int count = 0;
            StringBuilder str2 = new StringBuilder();;
            bool FirstSpace = true;
            while (count < str1.Length) {
                if (!XmlCharType.IsWhiteSpace(str1[count])) {
                    FirstSpace = true;
                    str2.Append(str1[count]);
                }
                else
                    if (FirstSpace) {
                    FirstSpace = false;
                    str2.Append(str1[count]);
                }
                count++;
            }
            return str2.ToString();
        }

        String Translate(XPathReader reader) {
            String str1 = ((IQuery)this.argList[0]).GetValue(reader).ToString();
            String str2 = ((IQuery)this.argList[1]).GetValue(reader).ToString();
            String str3 = ((IQuery)this.argList[2]).GetValue(reader).ToString();
            StringBuilder str = new StringBuilder();
            int count = 0, index;
            while (count < str1.Length) {
                index = str2.IndexOf(str1[count]);
                if (index != -1) {
                    if (index < str3.Length)
                        str.Append(str3[index]);
                }
                else
                    str.Append(str1[count]);
                count++;
            }
            return str.ToString();
        }

    }

    internal sealed class NumberFunctions : IQuery {
        private IQuery _qy = null;
        private FT _FuncType;

        public NumberFunctions(IQuery qy,
                                 FT ftype) {
            _qy = qy;
            _FuncType = ftype;
        }

        public NumberFunctions() {
        }

        public NumberFunctions(IQuery qy) {
            _qy = qy;
            _FuncType = FT.FuncNumber;
        }

        //
        //
        internal override object GetValue(XPathReader reader)
        {
            object obj = new object();

            switch (_FuncType) {
                case FT.FuncNumber:
                    obj = Number(reader);
                    break;

               // case FT.FuncSum:
               //     obj = Sum(reader);
               //     break;

                case FT.FuncFloor:
                    obj = Floor(reader);
                    break;

                case FT.FuncCeiling:
                    obj = Ceiling(reader);
                    break;

                case FT.FuncRound:
                    obj = Round(reader);
                    break;
            }

            return obj;
        }

        //
        //
        internal override XPathResultType ReturnType()
        {
            return XPathResultType.Number;
        }

        //
        //
        internal static double Number(bool _qy) {
            return(double) Convert.ToInt32(_qy);
        }

        //
        //
        internal static double Number(String _qy) {
            try {
                return Convert.ToDouble(_qy);
            }
            catch (System.Exception) {
                return double.NaN;
            }
        }

        //
        //
        //internal static double sum(XPathReader reader) {
        //    return 0;

        //}

        //
        //
        internal static double Number(double num) {
            return num;
        }

        //
        // number number(object?)
        // string: IEEE 754, NaN
        // boolean: true 1, false 0
        // node-set: number(string(node-set))
        //
        // <Root><e a='1'/></Root>
        // /Root/e[@a=number('1')]
        // /Root/e[number(@a)=1]

        private double Number(XPathReader reader) {

            if (_qy != null ) {
                object obj = _qy.GetValue(reader);

                if (obj == null) {
                    return double.NaN;
                }

                return (Convert.ToDouble(obj));

            }
            return double.NaN;

        }


        private double Floor(XPathReader reader) {
            return Math.Floor(Convert.ToDouble(_qy.GetValue(reader)));

        }

        private double Ceiling(XPathReader reader) {
            return Math.Ceiling(Convert.ToDouble(_qy.GetValue(reader)));
        }

        private double Round(XPathReader reader) {
            double n = Convert.ToDouble(_qy.GetValue(reader));
            // Math.Round does bankers rounding and Round(1.5) == Round(2.5) == 2
            // This is incorrect in XPath and to fix this we are useing Math.Floor(n + 0.5) istead
            // To deal with -0.0 we have to use Math.Round in [0.5, 0.0]
            return (-0.5 <= n && n <= 0.0) ? Math.Round(n) : Math.Floor(n + 0.5);
        }
    }

    //
    // BooleanFunctions
    //
    internal sealed class BooleanFunctions : IQuery {

        IQuery _qy;
        FT _FuncType;

        internal BooleanFunctions(IQuery qy, FT ftype)
        {
            _qy = qy;
            _FuncType = ftype;
        }

        internal BooleanFunctions(IQuery qy)
        {
            _qy = qy;
            _FuncType = FT.FuncBoolean;
        }

        //
        //
        //
        internal override object GetValue(XPathReader reader)
        {

            object obj = new object();

            switch (_FuncType) {
                case FT.FuncBoolean :
                    obj = toBoolean(reader);
                    break;

                case FT.FuncNot :
                    obj = Not(reader);
                    break;

                case FT.FuncTrue :
                    obj = true;
                    break;

                case FT.FuncFalse :
                    obj = false;
                    break;

                case FT.FuncLang :
                    obj = Lang(reader);
                    break;
            }
            return obj;
        }

        //
        //
        //
        internal override XPathResultType ReturnType()
        {
            return XPathResultType.Boolean;
        }


        //
        //
        //
        internal static Boolean toBoolean(double number) {

            if (number == 0 || double.IsNaN(number)) {
                return false;
            }
            else {
                return true;
            }
        }

        //
        // boolean boolean(object)
        //
        //
        internal static Boolean toBoolean(String str) {
            if (str.Length > 0)
                return true;
            return false;
        }

        //
        // boolean boolean(object)
        // Number: NaN, 0 -> false
        // String: Length = 0 -> false
        // Node-set: empty -> false
        //
        // <Root><e a='1'>text</e></Root>
        // 1). /Root/e[boolean(2)]
        // 2). /Root/e[boolean[string(child::text)]
        // 3). /Root/e[boolean(child::text)]
        internal Boolean toBoolean(XPathReader reader) {
            Boolean ret = true;

            object obj = _qy.GetValue(reader);

            if (obj is System.Double) {
                double number = Convert.ToDouble(obj);
                if (number == 0 || number == double.NaN) {
                    ret = false;
                }
            }
            else if (obj is System.String) {
                if (obj.ToString().Length == 0) {
                    ret = false;
                }

            }
            else if (obj is System.Boolean) {
                ret = Convert.ToBoolean(obj);
            }
            else if (obj == null && reader.NodeType != XmlNodeType.EndElement) {
                ret = false;
            }

            return ret;

        } // toBoolean

        //
        // boolean not(boolean)
        //
        private Boolean Not(XPathReader reader) {
            Boolean ret = toBoolean(reader);
            return !ret;
        }

        //
        // boolean lang(string)
        //
        private Boolean Lang(XPathReader reader) {
            String str = _qy.GetValue(reader).ToString();
            String lang = reader.XmlLang.ToLower();
            return (lang.Equals(str) || str.Equals(lang.Split('-')[0]));
        }
    }

}