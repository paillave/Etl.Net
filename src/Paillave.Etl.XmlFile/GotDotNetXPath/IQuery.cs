//------------------------------------------------------------------------------
// <copyright file="IQuery.cs" company="Microsoft">
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

    internal abstract class IQuery {
        int positionCount = 0;

        internal virtual object GetValue(XPathReader reader)
        {
            return null;
        }

        internal virtual int PositionCount {
            get { return this.positionCount; }
            set { this.positionCount = value; }
        }

        //the default always not matched
        internal virtual bool MatchNode(XPathReader reader)
        {
            return false;
        }

        internal abstract XPathResultType ReturnType();

    }
}