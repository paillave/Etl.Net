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
namespace GotDotNet.XPath
{
	using System;
    using System.IO;
    using System.Xml.XPath;
    using System.Resources;
    using System.Runtime.Serialization;
    using System.Diagnostics;

	// The XPathException class contains XML parser errors.
	// 
	/// <include file='doc\XPathException.uex' path='docs/doc[@for="XPathException"]/*' />
	/// <devdoc>
	///    <para>
	///       Represents the exception that is thrown when there is error processing an
	///       XPath expression.
	///    </para>
	/// </devdoc>
	[Serializable]
	public class XPathException : SystemException
	{
		public XPathException(string message) : base (message)
		{
		}
	}
    // The XPathReaderException class contains XML parser errors.
    // 
    /// <include file='doc\XPathException.uex' path='docs/doc[@for="XPathException"]/*' />
    /// <devdoc>
    ///    <para>
    ///       Represents the exception that is thrown when there is error processing an
    ///       XPath expression.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public class XPathReaderException : XPathException
    {

        public XPathReaderException(string message):base(message)
        {
        }
    }
}

