//------------------------------------------------------------------------------
// <copyright file="ExprQuery.cs" company="Microsoft">
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
    using System.Xml;
    using System.IO;
    using System.Text;
    using System.Collections;

    //
    // Logical Expression should result in Boolean
    // Rules:
    // 1). Opnd1: node-set, opnd2: node-set
    //  string(opnd1) op string(opnd1)
    //
    // 2). Opnd1: node-set
    //  opnd2: string
    //  string(opnd1) op opnd2
    //
    //  opnd2: number
    //  number(string(opnd1)) op opnd2
    //
    //  opnd2: boolean
    //  boolean(opnd1) op opnd2
    //
    // 3). Opnd convert sequence: boolean, number, string ( =, !=)
    //     (<=, <, >=, >) convert to number
    //
    // Example: <Root><e a1='1' a2='2'/></Root>
    // 1). /Root/e[@a1<@a2]: string(@a) < string(@b)
    // 2). /Root/e[@a1 = true]: boolean(@a) = true
    // 3). /Root/e["a" = true]: boolean("a") = true

    // opnd1 = opnd2
    // opnd1 != opnd2
    // opnd1 > opnd2
    // opnd1 < opnd2
    // opnd1 >= opnd2
    // opnd1 <= opnd2

    internal sealed class LogicalExpr : IQuery {
        IQuery opnd1;
        IQuery opnd2;
        internal Operator.Op op;

        internal LogicalExpr(Operator.Op op, IQuery opnd1, IQuery opnd2){
            this.opnd1= opnd1;
            this.opnd2= opnd2;
            this.op= op;
        }

        bool CompareAsNumber(XPathReader reader) {
            double n1 = 0, n2=0;
            object opndVar1, opndVar2;

            opndVar1 = this.opnd1.GetValue(reader);
            opndVar2 = this.opnd2.GetValue(reader);

            try {
                n1 = Convert.ToDouble(opndVar1);
                n2 = Convert.ToDouble(opndVar2);
            }  catch (System.Exception) {
                return false;
            }

            switch (op) {
                case Operator.Op.LT :
                    if (n1 < n2)
                        return true;
                    break;
                case Operator.Op.GT :
                    if (n1 > n2)
                        return true;
                    break;
                case Operator.Op.LE :
                    if (n1 <= n2)
                        return true;
                    break;
                case Operator.Op.GE  :
                    if (n1 >= n2)
                        return true;
                    break;
                case Operator.Op.EQ :
                    if (n1 == n2)
                        return true;
                    break;
                case Operator.Op.NE :
                    if (n1 != n2)
                        return true;
                    break;
            }

            return false;
        }

        bool CompareAsString(XPathReader reader) {

            bool ret = false;

            object opndVar1, opndVar2;

            opndVar1 = this.opnd1.GetValue(reader);
            opndVar2 = this.opnd2.GetValue(reader);
            string s1 =  null, s2 = null;
            if (opndVar1 == null || opndVar2 == null) {
                return false;
            }
            s1 = opndVar1.ToString();
            s2 = opndVar2.ToString();

            if (op > Operator.Op.GE) {
                if ((Operator.Op.EQ == op && s1.Equals(s2)) || (Operator.Op.NE == op && !s2.Equals(s2))) {
                    ret = true;
                }
            } else {
                //need to covert the string to the number and compare the numbers.
                double n1 = 0, n2 =0;
                try {
                    n1 = Convert.ToDouble(s1);
                    n2 = Convert.ToDouble(s2);
                } catch {

                }

                switch (op) {
                    case Operator.Op.LT : if (n1 < n2) ret = true;
                        break;
                    case Operator.Op.GT : if (n1 > n2) ret = true;
                        break;
                    case Operator.Op.LE : if (n1 <= n2) ret= true;
                        break;
                    case Operator.Op.GE  :if (n1 >= n2) ret = true;
                        break;
                }
            }
#if DEBUG1
            Console.WriteLine("s1 {0}, s2 {1}, op {2}, ret: {3}", s1, s2, op, ret);
#endif
            return ret;
        }

        bool CompareAsBoolean(XPathReader reader) {

            object opndVar1, opndVar2;
            bool b1, b2;

            opndVar1 = this.opnd1.GetValue(reader);
            opndVar2 = this.opnd2.GetValue(reader);

            if (opnd1.ReturnType() == XPathResultType.NodeSet) {
                b1 = (opndVar1 != null);
            } else {
                b1 = Convert.ToBoolean(opndVar1);
            }

            if (opnd1.ReturnType() == XPathResultType.NodeSet) {
                b2 = (opndVar2 != null);
            } else {
                b2 = Convert.ToBoolean(opndVar2);
            }

            if (op > Operator.Op.GE) {
                if ((Operator.Op.EQ == op && b1 == b2) || (Operator.Op.NE == op && b1 != b2)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                //need to covert the string to the number and compare the numbers.
                double n1 = 0, n2 =0;
                try {
                    n1 = Convert.ToDouble(b1);
                    n2 = Convert.ToDouble(b2);
                } catch {
                    return false;
                }

                switch (op) {
                    case Operator.Op.LT : if (n1 < n2) return true;
                        break;
                    case Operator.Op.GT : if (n1 > n2) return true;
                        break;
                    case Operator.Op.LE : if (n1 <= n2) return true;
                        break;
                    case Operator.Op.GE  :if (n1 >= n2) return true;
                        break;
                }
            }
            return false;
        }

        internal override object GetValue(XPathReader reader)
        {
            XPathResultType type1, type2;

            type1 = this.opnd1.ReturnType();
            type2 = this.opnd2.ReturnType();

            if (type1 == XPathResultType.Boolean || type2 == XPathResultType.Boolean)
                return CompareAsBoolean(reader);
            else
                if (type1 == XPathResultType.Number || type2 == XPathResultType.Number)
                return CompareAsNumber(reader);
            else
                return CompareAsString(reader);
        }

        internal override XPathResultType ReturnType()
        {
            return XPathResultType.Boolean;
        }

    }

    //
    // Get Value and result in number
    //
    internal sealed class NumericExpr : IQuery {
        internal IQuery opnd1;
        internal IQuery opnd2;
        internal Operator.Op op;

        //
        // The operand needs to use the number function
        // to covert to numbers
        internal NumericExpr(Operator.Op op, IQuery opnd1, IQuery opnd2) {

            if (opnd1.ReturnType() != XPathResultType.Number) {
                this.opnd1= new NumberFunctions(opnd1);
            }
            else {
                this.opnd1 = opnd1;
            }

            if (opnd2 != null && (opnd2.ReturnType() != XPathResultType.Number)) {
                this.opnd2= new NumberFunctions(opnd2);
            }
            else{
                this.opnd2 = opnd2;
            }

            this.op= op;
        }

        internal override object GetValue(XPathReader reader)
        {
            double n1 = 0, n2 = 0;

            n1 = Convert.ToDouble(this.opnd1.GetValue(reader));

            if (this.op != Operator.Op.NEGATE) {
                n2 = Convert.ToDouble(this.opnd2.GetValue(reader));
            }

            switch (this.op) {
                case Operator.Op.PLUS   : return  n1 + n2;
                case Operator.Op.MINUS  : return  n1 - n2;
                case Operator.Op.MOD    : return  n1 % n2;
                case Operator.Op.DIV    : return  n1 / n2;
                case Operator.Op.MUL    : return  n1 * n2;
                case Operator.Op.NEGATE : return -n1;
            }

            return null;
        }

        internal override XPathResultType ReturnType()
        {
            return XPathResultType.Number;
        }
    }

    //
    // Process Or expression
    //
    // Data type between the two operations
    //
    // LogicalExpression or LogicalExpression
    //
    //
    // for example:
    //     @a > 1 or @b < 2
    //     @a or @b
    //
    //   @a | @b or @c
    internal sealed class OrExpr :IQuery {
        private BooleanFunctions opnd1;
        private BooleanFunctions opnd2;

        internal OrExpr(IQuery opnd1, IQuery opnd2) {

            this.opnd1 = new BooleanFunctions(opnd1);
            this.opnd2 = new BooleanFunctions(opnd2);
        }

        internal override object GetValue(XPathReader reader)
        {
            object ret = this.opnd1.GetValue(reader);

            if (Convert.ToBoolean(ret) == false) {
                ret = this.opnd2.GetValue(reader);
            }
#if DEBUG1
            Console.WriteLine("OrExpr: {0}", ret);
#endif
            return ret;
        }

        internal override XPathResultType ReturnType()
        {
            return XPathResultType.Boolean;
        }
    }

    internal sealed class AndExpr : IQuery {
        private BooleanFunctions opnd1;
        private BooleanFunctions opnd2;

        AndExpr() {
        }

        internal AndExpr(IQuery opnd1, IQuery opnd2)
        {
            this.opnd1 = new BooleanFunctions(opnd1);
            this.opnd2 = new BooleanFunctions(opnd2);
        }

        internal override object GetValue(XPathReader reader)
        {
            object ret = this.opnd1.GetValue(reader);

            if (Convert.ToBoolean(ret) == true) {
                ret = this.opnd2.GetValue(reader);
            }
            return ret;
        }

        internal override XPathResultType ReturnType()
        {
            return XPathResultType.Boolean;
        }
    }
}