// using System;
// using System.IO;
// using System.Collections;
// using System.Xml;
// using GotDotNet.XPath;

// public class TestXPathReader
// {
// 	void DisplayNode(XPathReader reader)
// 	{
// 		Console.WriteLine("NodeType:'{0}', Name:'{1}', Value:'{2}'", reader.NodeType, reader.Name, reader.Value);
// 		Console.WriteLine();
// 	}

// 	void DisplayMatchingNode(XPathQuery xp, XPathReader reader)
// 	{
// 		Console.WriteLine("Found matching node with query: {0}", xp);
// 		Console.WriteLine("NodeType:'{0}', Name:'{1}', Value:'{2}'", reader.NodeType, reader.Name, reader.Value);
// 		Console.WriteLine();
// 	}

// 	void CheckQuery(XPathReader reader, XPathCollection xc, int index, string name)
// 	{
// 		if (reader.Match(index))
// 		{
// 			DisplayMatchingNode(xc[index], reader);
// 			if (reader.MoveToAttribute(name))
// 			{
// 				DisplayNode(reader);
// 				reader.MoveToElement();
// 			}
// 		}
// 	}

// 	void CheckQuery(XPathReader reader, XPathCollection xc, int index)
// 	{
// 		if (reader.Match(index))
// 		{
// 			DisplayMatchingNode(xc[index], reader);
// 		}
// 	}

// 	//
// 	// The simplest test, should pass
// 	//
// 	public void ChildAxisEmptyElement() 
// 	{

// 		Console.WriteLine("\nTestChildAxisEmptyElement");

// 		string doc = "<e/>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("/e");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("child::node()");
// 		int query3 = xc.Add("e");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read()) 
// 		{

// 			if (xpathReader.NodeType == XmlNodeType.Element || xpathReader.NodeType == XmlNodeType.EndElement) 
// 			{
// 				CheckQuery(xpathReader, xc, query1);
// 				CheckQuery(xpathReader, xc, query2);
// 				CheckQuery(xpathReader, xc, query3);
// 			}
// 		}
// 	}

// 	//
// 	// The simplest test, should pass
// 	//
// 	public void ChildAxis()	
// 	{

// 		Console.WriteLine("\nTestChildAxis");

// 		string doc = "<e>test</e>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("/e");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("child::node()");
// 		int query3 = xc.Add("e");
// 		int query4 = xc.Add("e/child::text()");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{

// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 				case XmlNodeType.EndElement:
// 					CheckQuery(xpathReader, xc, query1);
// 					CheckQuery(xpathReader, xc, query2);
// 					CheckQuery(xpathReader, xc, query3);

// 					break;

// 				case XmlNodeType.Text:
// 					if (xpathReader.Match(query4)) 
// 					{
// 						DisplayMatchingNode(xc[query4], xpathReader);
// 					}
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Without the namespace manager, the prefix case should fail.
// 	//
// 	public void ChildAxisWithPrefixWithoutNamespaceMgr()
// 	{

// 		Console.WriteLine("\nTestChildAxis");

// 		string doc = "<p:e xmlns:p='foo'>test</p:e>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("/p:e");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("child::node()");
// 		int query3 = xc.Add("p:e");
// 		int query4 = xc.Add("p:e/child::text()");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read()) 
// 		{

// 			switch (xpathReader.NodeType) 
// 			{
// 				case XmlNodeType.Element:
// 				case XmlNodeType.EndElement:
// 					CheckQuery(xpathReader, xc, query1);
// 					CheckQuery(xpathReader, xc, query2);
// 					CheckQuery(xpathReader, xc, query3);
// 					break;

// 				case XmlNodeType.Text:
// 					if (xpathReader.Match(query4)) 
// 					{
// 						DisplayMatchingNode(xc[query4], xpathReader);
// 					}
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	public void ChildAxisWithPrefixWithNamespaceMgr() 
// 	{

// 		Console.WriteLine("\nChildAxisWithPrefixWithNamespaceMgr");

// 		string doc = "<p:e xmlns:p='foo'>test</p:e>";
// 		Console.WriteLine("Document: {0}", doc);
// 		XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
// 		nsManager.AddNamespace("p", "foo");

// 		XPathCollection xc = new XPathCollection(nsManager);
// 		XPathQuery xp = new XPathQuery("/p:e");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("child::node()");
// 		int query3 = xc.Add("p:e");
// 		int query4 = xc.Add("p:e/child::text()");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read()) 
// 		{

// 			switch (xpathReader.NodeType) 
// 			{
// 				case XmlNodeType.Element:
// 				case XmlNodeType.EndElement:
// 					CheckQuery(xpathReader, xc, query1);
// 					CheckQuery(xpathReader, xc, query2);
// 					CheckQuery(xpathReader, xc, query3);
// 					break;

// 				case XmlNodeType.Text:
// 					if (xpathReader.Match(query4)) 
// 					{
// 						DisplayMatchingNode(xc[query4], xpathReader);
// 					}
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}
    
// 	void Parent()
// 	{
// 		Console.WriteLine("\nParent");

// 		string doc = "<Root><e a11='1' a12='2'/><e a21='2' a22='1'>text2</e><e a31='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		try
// 		{
// 			XPathQuery xp = new XPathQuery("/Root/e/parent::node()");
// 			int query1 = xc.Add(xp);
// 		}
// 		catch (Exception ex)
// 		{
// 			Console.WriteLine(ex.Message);
// 		}
// 	}

// 	void ParentShortSyntax()
// 	{
// 		Console.WriteLine("\nParentShortSyntax");

// 		string doc = "<Root><e a11='1' a12='2'/><e a21='2' a22='1'>text2</e><e a31='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		try
// 		{
// 			XPathQuery xp = new XPathQuery("/Root/e/..");
// 			int query1 = xc.Add(xp);
// 		}
// 		catch (Exception ex)
// 		{
// 			Console.WriteLine(ex.Message);
// 		}
// 	}


// 	//
// 	//Test the XPath count function
// 	//
// 	void TestMethodOperandCount()
// 	{
// 		Console.WriteLine("\nTestMethodOperandCount");

// 		string doc = "<Root><e a11='1' a12='2'/><e a21='2' a22='1'>text2</e><e a31='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("/Root/e[count(@*)=2]");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 				case XmlNodeType.EndElement:
// 					CheckQuery(xpathReader, xc, query1);
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	void TestMethodOperandPosition()
// 	{
// 		Console.WriteLine("\nTestMethodOperandPosition");

// 		string doc = "<Root><e a11='1' a12='2'/><e a21='2' a22='1'>text2</e><e a31='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("/Root/e[position()=2]");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 				case XmlNodeType.EndElement:
// 					CheckQuery(xpathReader, xc, query1);
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	void TestMethodOperandNamespaceUri()
// 	{
// 		Console.WriteLine("\nTestMethodOperandNamespaceUri");

// 		string doc = "<Root><e a='1' b='2' xmlns='foo'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[namespace-uri()='foo']");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 				case XmlNodeType.EndElement:
// 					CheckQuery(xpathReader, xc, query1);
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	/*

// 	void TestMethodOperandName();
// 	void TestMethodOperandLocalName();
// 	*/
// 	void TestDescendentAxis()
// 	{
// 		Console.WriteLine("\nTestDescendentAxis");

// 		string doc = "<Root><e a='1' b='2' xmlns='foo'/><e a='1' b='1' xmlns='foo'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("//e[namespace-uri()='foo'");
// 		int query2 = xc.Add("/Root//e[namespace-uri()='foo'");
// 		int query3 = xc.Add("//*[local-name()='e' and namespace-uri()='foo']/*[local-name()='a' and namespace-uri()='foo']");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 				case XmlNodeType.EndElement:
// 					CheckQuery(xpathReader, xc, query1);
// 					CheckQuery(xpathReader, xc, query2);
// 					CheckQuery(xpathReader, xc, query3);
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	void DescendentAxisReadUtil()
// 	{
// 		Console.WriteLine("\nTestDescendentAxisReadUtil");

// 		string doc = "<Root><Root1><e a='1' b='2' xmlns='foo'/><e b='1' xmlns='foo'/><e a='3'/></Root1></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("//e[namespace-uri()='foo']/@a[namespace-uri()='foo']");


// 		//int query2 = xc.Add("//*[local-name()='e' and namespace-uri()='foo']/*[local-name()='a' and namespace-uri()='foo']");
// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.ReadUntilMatch())
// 		{
// 			if (xpathReader.Match(query1))
// 			{
// 				this.DisplayMatchingNode(xc[query1], xpathReader);
// 			}
// 			/*
// 			else if (xpathReader.Match(query2)){
// 				this.DisplayMatchingNode(xc[query2], xpathReader);
// 			}
// 			*/
// 		}
// 	}

// 	//
// 	// Test Read to see if we match the attribute query.
// 	// The match query should be tested in the Attribute
// 	//
// 	// This requires to check all the queries, if the one of incoming
// 	// queries in the query collection is attribute query,
// 	// we need to do an empty read and process.
// 	//
// 	// This is not going to happen.
// 	public void AttributeAxisOnRead()
// 	{
// 		Console.WriteLine("\nTestAttributeAxisOnRead");

// 		string doc = "<Root><e a='1'/><e a='2'>text2</e><e a='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/@a");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			if (xpathReader.NodeType == XmlNodeType.Attribute)
// 			{
// 				if (xpathReader.Match(query1))
// 				{
// 					DisplayMatchingNode(xc[query1], xpathReader);
// 				}
// 			}
// 		}
// 	}

// 	//
// 	// Mutliple attribute nodes
// 	// This is not going to happen
// 	//
// 	public void TestMultipleAttributeAxisOnRead()
// 	{
// 		Console.WriteLine("\nTestAttributeAxisOnRead");

// 		string doc = "<Root><e a11='1' a12='2'/><e a21='2'>text2</e><e a31='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/attribute::node()");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			//
// 			// results should be a11, a12, a21, a31
// 			//
// 			if (xpathReader.NodeType == XmlNodeType.Attribute)
// 			{
// 				if (xpathReader.Match(query1))
// 				{
// 					DisplayMatchingNode(xc[query1], xpathReader);
// 				}
// 			}
// 		}
// 	}

// 	//
// 	// Mutliple attribute nodes
// 	// This is not going to happen
// 	//
// 	public void TestMultipleAttributeAxisOnMoveToAttribute()
// 	{
// 		Console.WriteLine("\nTestMultipleAttributeAxisOnMoveToAttribute");

// 		string doc = "<Root><e a11='1' a12='2'/><e a21='2'>text2</e><e a31='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/attribute::node()");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			//
// 			// results should be a11, a12, a21, a31
// 			//
// 			if (xpathReader.NodeType == XmlNodeType.Element)
// 			{
// 				while (xpathReader.MoveToNextAttribute() && xpathReader.Match(query1))
// 				{
// 					DisplayMatchingNode(xc[query1], xpathReader);
// 				}
// 			}
// 		}
// 	}

// 	//
// 	// User do a reader move to the attribute
// 	//
// 	//  Create second query "/Root/e", match the second query first,
// 	//  then do a MoveToAttribute
// 	public void AttributeAxisOnMoveToAttributeFilterElement()
// 	{
// 		Console.WriteLine("\nAttributeAxisOnMoveToAttributeFilterElement");

// 		string doc = "<Root><e a='1'/><e a='2'>text2</e><e a='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/@a");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("Root/e");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			if (xpathReader.NodeType == XmlNodeType.Element)
// 			{
// 				if (xpathReader.Match(query2))
// 				{
// 					if (xpathReader.MoveToAttribute("a") && xpathReader.Match(query1))
// 					{
// 						DisplayMatchingNode(xc[query1], xpathReader);
// 					}
// 				}
// 			}
// 		}
// 	}

// 	//
// 	// User do a reader move to the attribute
// 	//
// 	// This is not recommented, since the user has to do a MoveToAttribute
// 	// on all the Element.
// 	//
// 	// The recommand methods:
// 	//   1). Check node type on attribute (TestAttributeAxisOnRead)
// 	//   2). Create second query "/Root/e", match the second query first,
// 	//       then do a MoveToAttribute (TestAttributeAxisOnMoveToAttributeFilterElement)
// 	public void AttributeAxisOnMoveToAttribute()
// 	{
// 		Console.WriteLine("\nAttributeAxisOnMoveToAttribute");

// 		string doc = "<Root><e a='1'/><e a='2'>text2</e><e a='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/@a");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read()) 
// 		{
// 			if (xpathReader.NodeType == XmlNodeType.Element) 
// 			{
// 				if (xpathReader.MoveToAttribute("a") && xpathReader.Match(query1)) 
// 				{
// 					DisplayMatchingNode(xc[query1], xpathReader);
// 				}
// 			}
// 		}
// 	}

// 	// Mutliple attribute nodes
// 	//
// 	public void AttributeReadMoveToAttribute()
// 	{
// 		Console.WriteLine("\nTestAttributeReadMoveToAttribute");

// 		string doc = "<Root><e a11='1' a12='2'/><e a21='2'>text2</e><e a31='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/@a11");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read()) 
// 		{
// 			//
// 			// The element is going to return first
// 			//
// 			if (xpathReader.NodeType == XmlNodeType.Element) 
// 			{
// 				if (xpathReader.MoveToAttribute("a11")) 
// 				{
// 					DisplayMatchingNode(xc[query1], xpathReader);
// 				}
// 			}
// 			//
// 			// after the attribute is done, return to the element for the next read.
// 			//
// 			if (xpathReader.NodeType == XmlNodeType.Attribute) 
// 			{
// 				if (xpathReader.Match(query1))
// 				{
// 					DisplayMatchingNode(xc[query1], xpathReader);
// 				}
// 			}
// 		}
// 	}

// 	//
// 	// Self query
// 	//
// 	public void SelfAxis()
// 	{
// 		Console.WriteLine("\nTestSelfAxis");

// 		string doc = "<Root><e a='1'/><e a='2'>text2</e><e a='3'>test3</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/self::node()");
// 		int query1 = xc.Add(xp);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			if (xpathReader.NodeType == XmlNodeType.Element)
// 			{
// 				if (xpathReader.Match(query1))
// 				{
// 					DisplayMatchingNode(xc[query1], xpathReader);
// 				}
// 			}
// 		}
// 	}

// 	//
// 	// Add Minutes numeric Expression
// 	//
// 	public void NumericExpressionAddMinutes()
// 	{
// 		Console.WriteLine("\nTestNumericExpressionAddMinutes");

// 		string doc = "<Root><e a='1' b='2'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[@a+1]");
// 		int query2 = xc.Add("/Root/e[@b - 1]");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "b");
// 					CheckQuery(xpathReader, xc, query2, "b");
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Multiple and div numeric Expression
// 	//
// 	public void TestNumericExpressionMutliDiv()
// 	{
// 		Console.WriteLine("\nTestNumericExpressionMutliDiv");

// 		string doc = "<Root><e a='1' b='4'/><e a='1' b='2'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[@a*1]");
// 		int query2 = xc.Add("/Root/e[@b mod 1]");
// 		int query3 = xc.Add("/Root/e[@b div 1]");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "b");
// 					CheckQuery(xpathReader, xc, query2, "b");
// 					CheckQuery(xpathReader, xc, query3, "b");
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	/*

// 		string doc = "<Root><e a='1'/><e a='2'>test</e><e a='3'>test1</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("Root/e/@a");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("/Root/e");
// 		int query3 = xc.Add("/Root/e[@a='1']");
// 		int query4 = xc.Add("/Root/e[1]");
// 		int query5 = xc.Add("/Root/e[1+2]");
// 	*/
// 	public void TestLogicalExpression()
// 	{
// 		Console.WriteLine("\nTestLogicalExpression");

// 		string doc = "<Root><e a='1' b='2'/><e a='1' b='5'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[@a='1' and @b='2']");
// 		int query2 = xc.Add("/Root/e[@a='1' or @b='3']");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "b");
// 					CheckQuery(xpathReader, xc, query2, "b");
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Test the results in nodeset
// 	//
// 	public void TestNodeSetExpression()
// 	{
// 		Console.WriteLine("\nTestNodeSetExpression");

// 		string doc = "<Root><e a='1' b='2'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[@a]");
// 		int query2 = xc.Add("/Root/e[@b]");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "b");
// 					CheckQuery(xpathReader, xc, query2, "b");
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Test the results in nodeset
// 	//
// 	public void TestStringExpression()
// 	{
// 		Console.WriteLine("\nTestStringExpression");

// 		string doc = "<Root><e a='1' b='2'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[@a+'1']");
// 		int query2 = xc.Add("/Root/e[@b+'1']");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "b");
// 					CheckQuery(xpathReader, xc, query2, "b");
// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Test string functions
// 	//
// 	// string string(object?);
// 	// string concat(string, string, string*)
// 	// boolean starts-with(string, string)
// 	// boolean contains(string, string)
// 	// string substring-before(string, string);
// 	// string substring-after(string, string);
// 	// string substring(string, number, number);
// 	public void TestStringFunctions()
// 	{
// 		Console.WriteLine("\nTestStringFunctions");

// 		string doc = "<Root><e a='1text' b='2'>textNode</e><e a='2text' b='5'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[string(@a)='1text']");
// 		int query2 = xc.Add("/Root/e[string(@a) = concat('1te','xt')]");
// 		int query3 = xc.Add("/Root/e[starts-with(string(@a), '2')]");
// 		int query4 = xc.Add("/Root/e[contains(string(@a), '3')]");


// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "a");
// 					CheckQuery(xpathReader, xc, query2, "a");
// 					CheckQuery(xpathReader, xc, query3, "a");
// 					CheckQuery(xpathReader, xc, query4, "a");

// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Test number functions
// 	//
// 	// number number(object?);
// 	// number sum(node-set) //current context node value
// 	// number floor(number)
// 	// number ceiling(number)
// 	// number round(number);
// 	//
// 	public void TestNumberFunctions()
// 	{
// 		Console.WriteLine("\nTestNumberFunctions");

// 		string doc = "<Root><e a='1' b='2'>textNode</e><e a='2text' b='5'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[number('1')]");
// 		try
// 		{
// 			int query2 = xc.Add("/Root/e[sum(@a)]");
// 		}
// 		catch
// 		{
// 		}
// 		int query3 = xc.Add("/Root/e[floor(2.2)]"); //2
// 		int query4 = xc.Add("/Root/e[ceiling(2.2)]"); //3
// 		int query5 = xc.Add("/Root/e[round(2.2)]"); //2
// 		int query6 = xc.Add("/Root/e[round(2.6)]"); //3


// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "a");
// 					//CheckQuery(xpathReader, xc, query2, "a");
// 					CheckQuery(xpathReader, xc, query3, "a");
// 					CheckQuery(xpathReader, xc, query4, "a");
// 					CheckQuery(xpathReader, xc, query5, "a");
// 					CheckQuery(xpathReader, xc, query6, "a");

// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Test boolean functions
// 	//
// 	// boolean boolean(object);
// 	// boolean not(boolean) //current context node value
// 	// boolean true()
// 	// boolean false()
// 	// boolean lang(string);
// 	//
// 	public void TestBooleanFunctions()
// 	{
// 		Console.WriteLine("\nTestBooleanFunctions");

// 		string doc = "<Root xml:lang='en'><e a='1'>test</e><e a='2text' b='5'/><e a='3'/><e/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[boolean(@a)]");
// 		int query2 = xc.Add("/Root/e[not(boolean(@a))]");
// 		int query3 = xc.Add("/Root/e[true()]");
// 		int query4 = xc.Add("/Root/e[false()]");
// 		int query5 = xc.Add("/Root/e[lang('en')]");



// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query1, "a");
// 					CheckQuery(xpathReader, xc, query2, "a");
// 					CheckQuery(xpathReader, xc, query3, "a");
// 					CheckQuery(xpathReader, xc, query4, "a");
// 					CheckQuery(xpathReader, xc, query5, "a");

// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// Test nodeset functions
// 	//
// 	// number last();
// 	// number position() //current context node value
// 	// number count(Node-Set)
// 	// node-set id(object)
// 	//
// 	public void TestNodeSetFunctions()
// 	{
// 		Console.WriteLine("\nTestNodeSetFunctions");

// 		string doc = "<Root xml:lang='en'><e a='1'>test</e><e a='2text' b='5'/><e a='3'/><e/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		try
// 		{
// 			int query1 = xc.Add("/Root/e[last()]");
// 		}
// 		catch (Exception ex)
// 		{
// 			Console.WriteLine("Message: {0}", ex.Message);
// 		}

// 		int query2 = xc.Add("/Root/e[position() = 2]");
// 		int query3 = xc.Add("/Root/e[position() > 2]");
// 		int query4 = xc.Add("/Root/e[count(@a) = 2]");

// 		/*
// 		try
// 		{
// 			int query5 = xc.Add("/Root/id(@a)");
// 		}
// 		catch (Exception ex)
// 		{
// 			Console.WriteLine("Message: {0}", ex.Message);
// 		}
// 		*/



// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.Read())
// 		{
// 			switch (xpathReader.NodeType)
// 			{
// 				case XmlNodeType.Element:
// 					CheckQuery(xpathReader, xc, query2, "a");
// 					CheckQuery(xpathReader, xc, query3, "a");
// 					CheckQuery(xpathReader, xc, query4, "a");

// 					break;

// 				default:
// 					break;
// 			}
// 		}
// 	}

// 	//
// 	// ReadUntil Method will stop on if any one of the query is matched.
// 	//
// 	public void TestReadUntil()
// 	{
// 		Console.WriteLine("TestReadUntil");

// 		string doc = "<Root>rootText<e a='1'>text</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("/Root/e");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("/Root/e/child::text()");
// 		int query3 = xc.Add("/Root/e/@a");
// 		int query4 = xc.Add("/Root/child::text()");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.ReadUntilMatch())
// 		{
// 			if (xpathReader.Match(query1))
// 			{
// 				DisplayMatchingNode(xc[query1], xpathReader);
// 			}

// 			if (xpathReader.Match(query2))
// 			{
// 				DisplayMatchingNode(xc[query2], xpathReader);
// 			}

// 			if (xpathReader.Match(query3))
// 			{
// 				DisplayMatchingNode(xc[query3], xpathReader);
// 			}

// 			if (xpathReader.Match(query4))
// 			{
// 				DisplayMatchingNode(xc[query4], xpathReader);
// 			}
// 		}
// 	}

// 	//
// 	// ReadUntil Method with MatchAny
// 	//
// 	public void TestMatchAny()
// 	{
// 		ArrayList matchList = new ArrayList();

// 		Console.WriteLine("\nTestMatchAny");

// 		string doc = "<Root>rootText<e a='1'>text</e></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XPathCollection xc = new XPathCollection();
// 		XPathQuery xp = new XPathQuery("/Root/e");
// 		int query1 = xc.Add(xp);
// 		int query2 = xc.Add("/Root/e/child::text()");
// 		int query3 = xc.Add("/Root/e/@a");
// 		int query4 = xc.Add("/Root/child::text()");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));
// 		XPathReader xpathReader = new XPathReader(reader, xc);

// 		while (xpathReader.ReadUntilMatch())
// 		{
// 			xpathReader.MatchesAny(matchList);
// 			foreach (int i in matchList)
// 			{
// 				DisplayMatchingNode(xc[i], xpathReader);
// 			}
// 		}
// 	}

// 	//
// 	// process xpath collection on the current file.
// 	//
// 	internal void DumpResults(string datafile, XPathCollection xc)
// 	{
// 		Console.WriteLine("datafile name: {0}", datafile);
// 		foreach (XPathQuery expr in xc)
// 		{
// 			Console.WriteLine("xpath: {0}", expr);
// 		}
// 	}

// 	public void TestMultipleLevelDoc()
// 	{
// 		Console.WriteLine("\nTestMultipleLevelDoc");

// 		string doc = @"<root>
//                             <datafile name='ado.xml'>
//                                 <xpath name='firstname' value='/data/row/@au_fname' />
//                                 <xpath name='lastname' value='/data/row/@au_lname' />
//                                 <xpath name='state' value='/data/row[@state=1]' />
//                             </datafile>
//                       </root>";

// 		Console.WriteLine("Document: {0}", doc);

// 		string xpath1 = "/root/datafile/@name";
// 		string xpath2 = "/root/datafile/xpath/@value";
// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add(xpath1);
// 		int query2 = xc.Add(xpath2);

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xr = new XPathReader(reader, xc);

// 		XPathCollection xcTest = new XPathCollection();

// 		string dataFileName = String.Empty;

// 		while (xr.ReadUntilMatch())
// 		{
// 			if (xr.Match(query1))
// 			{
// 				DisplayMatchingNode(xc[query1], xr);

// 				if (xcTest.Count > 0)
// 				{
// 					DumpResults(dataFileName, xcTest);
// 					xcTest.Clear();
// 				}

// 				dataFileName = xr.Value;
// 			}
// 			else if (xr.Match(query2))
// 			{
// 				DisplayMatchingNode(xc[query2], xr);
// 				xcTest.Add(xr.Value);
// 			}
// 		}

// 		if (xcTest.Count > 0)
// 		{
// 			DumpResults(dataFileName, xcTest);
// 			xcTest.Clear();
// 		}
// 	}

// 	public void LogicalExprAttributeRead()
// 	{
// 		Console.WriteLine("\nLogicalExprAttributeRead");

// 		string doc = @"<data><row au_lname='White' au_fname='Johnson'  name='sysobjects' state='CA'/></data>";

// 		Console.WriteLine("Document: {0}", doc);

// 		string xpath1 = "/data/row[@au_fname='Johnson' and @au_lname='White']/@name";
// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add(xpath1);
// 		int query2 = xc.Add("/data/row[@au_lname='White']/@name");
// 		int query3 = xc.Add("/data/row[@state='CA']/@au_lname");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xr = new XPathReader(reader, xc);

// 		while (xr.Read())
// 		{
// 			if (xr.NodeType == XmlNodeType.Element)
// 			{
// 				if (xr.MoveToAttribute("name"))
// 				{
// 					CheckQuery(xr, xc, query1);
// 					CheckQuery(xr, xc, query2);
// 				}

// 				if (xr.MoveToAttribute("au_lname"))
// 				{
// 					CheckQuery(xr, xc, query3);
// 				}
// 			}
// 		}
// 	}

// 	public void LogicalExprAttributeReadUntil()
// 	{
// 		Console.WriteLine("\nLogicalExprAttributeReadUntil");

// 		string doc = @"<data><row au_lname='White' au_fname='Johnson'  name='sysobjects' state='CA'/></data>";

// 		Console.WriteLine("Document: {0}", doc);

// 		string xpath1 = "/data/row[@au_fname='Johnson' and @au_lname='White']/@name";
// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add(xpath1);
// 		int query2 = xc.Add("/data/row[@au_lname='White']/@name");
// 		int query3 = xc.Add("/data/row[@state='CA']/@au_lname");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(doc));

// 		XPathReader xr = new XPathReader(reader, xc);

// 		while (xr.ReadUntilMatch())
// 		{
// 			if (xr.Match(query2))
// 			{
// 				DisplayMatchingNode(xc[query2], xr);
// 			}

// 			if (xr.Match(query1))
// 			{
// 				DisplayMatchingNode(xc[query1], xr);
// 			}

// 			if (xr.Match(query3))
// 			{
// 				DisplayMatchingNode(xc[query3], xr);
// 			}
// 		}
// 	}

// 	public void TestMultipleDocument()
// 	{
// 		//expr
// 		XPathCollection xc1 = new XPathCollection();
// 		XPathQuery xp1 = new XPathQuery("/e");
// 		xc1.Add(xp1);
// 		xc1.Add("/e1");
// 		xc1.Add("e");



// 		XPathCollection xc2 = new XPathCollection();
// 		xc2.Add("/e");
// 		xc2.Add("/e2");


// 		XmlTextReader reader1 = new XmlTextReader("doc1.xml");
// 		XmlTextReader reader2 = new XmlTextReader("doc2.xml");

// 		XPathReader xr1 = new XPathReader(reader1, xc1);
// 		XPathReader xr2 = new XPathReader(reader2, xc2);
// 	}

// 	public void TestDynamicQuery()
// 	{
// 		string doc = @"<data><row au_lname='White' au_fname='Johnson'  name='sysobjects' state='CA'/></data>";

// 		XmlTextReader xt = new XmlTextReader(new StringReader(doc));

// 		XPathCollection xc1 = new XPathCollection();
// 		int query1 = xc1.Add("data/row");

// 		XPathReader xr = new XPathReader(xt, xc1);


// 		while (xr.ReadUntilMatch())
// 		{
// 			if (xr.Match(query1))
// 			{
// 				int query2 = xc1.Add("@au_lname");
// 			}
// 			DisplayMatchingNode(xc1[query1], xr);
// 		}
// 	}

// 	//
// 	//
// 	//
// 	public void TestRemoveXPathByIndex()
// 	{
// 		Console.WriteLine("\nTestRemoveXPathByIndex");

// 		string doc = "<Root><e a='1' b='2'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XmlTextReader xt = new XmlTextReader(new StringReader(doc));

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[@b=2]");
// 		int query2 = xc.Add("/Root/e[@a=1]");

// 		XPathReader xr = new XPathReader(xt, xc);

// 		while (xr.ReadUntilMatch())
// 		{
// 			if (xr.Match(query2))
// 			{
// 				DisplayMatchingNode(xc[query2], xr);
// 				if (xr.MoveToAttribute("b"))
// 				{
// 					DisplayNode(xr);
// 					xr.MoveToElement();
// 				}
// 			}

// 			if (xr.Match(query1))
// 			{
// 				DisplayMatchingNode(xc[query1], xr);
// 				if (xr.MoveToAttribute("b"))
// 				{
// 					DisplayNode(xr);
// 					xr.MoveToElement();
// 				}
// 				xc.Remove(query2);
// 			}
// 		}
// 	}

// 	//
// 	//
// 	//
// 	public void TestRemoveXPathByExpression()
// 	{
// 		Console.WriteLine("\nTestRemoveXPathByExpression");

// 		string doc = "<Root><e a='1' b='2'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XmlTextReader xt = new XmlTextReader(new StringReader(doc));

// 		XPathCollection xc = new XPathCollection();
// 		int query1 = xc.Add("/Root/e[@b=2]");
// 		int query2 = xc.Add("/Root/e[@a=1]");

// 		XPathReader xr = new XPathReader(xt, xc);

// 		while (xr.ReadUntilMatch())
// 		{
// 			if (xr.Match(query2))
// 			{
// 				DisplayMatchingNode(xc[query2], xr);
// 				if (xr.MoveToAttribute("b"))
// 				{
// 					DisplayNode(xr);
// 					xr.MoveToElement();
// 				}
// 			}

// 			if (xr.Match(query1))
// 			{
// 				DisplayMatchingNode(xc[query1], xr);
// 				if (xr.MoveToAttribute("b"))
// 				{
// 					DisplayNode(xr);
// 					xr.MoveToElement();
// 				}
// 				xc.Remove(xc[query2]);
// 			}
// 		}
// 	}

// 	//
// 	//
// 	//
// 	public void RecursiveQuery()
// 	{
// 		Console.WriteLine("\nRecursiveQuery");

// 		string doc = "<Root><e a='1' b='2' xmlns='foo'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XmlTextReader xt = new XmlTextReader(new StringReader(doc));

// 		XPathCollection xc = new XPathCollection();
// 		int query = xc.Add("//*[local-name()='e' and namespace-uri()='foo']");
// 		int query1 = xc.Add("//e[namespace-uri()='foo']");

// 		XPathReader xr = new XPathReader(xt, xc);

// 		while (xr.ReadUntilMatch())
// 		{
// 			if (xr.Match(query))
// 			{
// 				DisplayMatchingNode(xc[query], xr);
// 				if (xr.MoveToAttribute("b"))
// 				{
// 					DisplayNode(xr);
// 					xr.MoveToElement();
// 				}
// 			}
// 		}
// 	}

// 	//
// 	//
// 	//
// 	public void ValidatingReader()
// 	{
// 		Console.WriteLine("\nVadatingReader");

// 		string doc = "<Root><e a='1' b='2' xmlns='foo'/><e a='1' b='1'/><e a='3'/></Root>";
// 		Console.WriteLine("Document: {0}", doc);

// 		XmlTextReader xt = new XmlTextReader(new StringReader(doc));
// 		XmlValidatingReader vr = new XmlValidatingReader(xt);

// 		XPathCollection xc = new XPathCollection();
// 		int query = xc.Add("Root/*[local-name()='e' and namespace-uri()='foo']");
// 		int query1 = xc.Add("//e[namespace-uri()='foo']");

// 		XPathReader xr = new XPathReader(vr, xc);

// 		while (xr.ReadUntilMatch())
// 		{
// 			DisplayNode(xr);
// 		}
// 	}


// 	void BizTalk1()
// 	{
// 		XPathCollection xc = new XPathCollection();
// 		xc.NamespaceManager = new XmlNamespaceManager(new NameTable());


// 		// 1) /XmlEnvelope/Attributes/@integer
// 		xc.Add("/*[local-name()='XmlEnvelope' and namespace-uri()='http://PropPromotion.XmlEnvelope']/*[local-name()='Attributes']/@*[local-name()='integer']");
// 		// 2) /XmlEnvelope/Attributes/@language
// 		xc.Add("/*[local-name()='XmlEnvelope' and namespace-uri()='http://PropPromotion.XmlEnvelope']/*[local-name()='Attributes']/@*[local-name()='language']");
// 		// 3) /XmlEnvelope/FieldElements/anyuri
// 		xc.Add("/*[local-name()='XmlEnvelope' and namespace-uri()='http://PropPromotion.XmlEnvelope']/*[local-name()='FieldElements']/*[local-name()='anyuri']");
// 		// 4) /XmlEnvelope/FieldElements/boolean
// 		xc.Add("/*[local-name()='XmlEnvelope' and namespace-uri()='http://PropPromotion.XmlEnvelope']/*[local-name()='FieldElements']/*[local-name()='boolean']");
// 		// 5) /XmlEnvelope/Body
// 		xc.Add("/*[local-name()='XmlEnvelope' and namespace-uri()='http://PropPromotion.XmlEnvelope']/*[local-name()='Body']");
// 		// 6) /XmlEnvelope/Body/Attributes/@integer
// 		xc.Add("/*[local-name()='XmlEnvelope' and namespace-uri()='http://PropPromotion.XmlEnvelope']/*[local-name()='Body']/*[local-name()='XmlBody' and namespace-uri()='http://PropPromotion.XmlBody']/*[local-name()='Attributes']/@*[local-name()='integer']");
// 		// 7) /XmlEnvelope/Body/Attributes/@language
// 		xc.Add("/*[local-name()='XmlEnvelope' and namespace-uri()='http://PropPromotion.XmlEnvelope']/*[local-name()='Body']/*[local-name()='XmlBody' and namespace-uri()='http://PropPromotion.XmlBody']/*[local-name()='FieldElements']/*[local-name()='anyuri']");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(@"
//               <XmlEnvelope xmlns='http://PropPromotion.XmlEnvelope'>
//                  <Attributes integer='57604' language='en-us'/>
//                     <FieldElements>
//                        <anyuri>ftp://bla.bla</anyuri>
//                        <boolean>1</boolean>
//                     </FieldElements>
//                     <Body>
//                        <XmlBody xmlns='http://PropPromotion.XmlBody'>
//                           <Attributes integer='-3368' language='fr-ca' />
//                           <FieldElements>
//                              <anyuri>ftp://yes.yes</anyuri>
//                              <boolean>false</boolean>
//                           </FieldElements>
//                        </XmlBody>
//                     </Body>
//                </XmlEnvelope>"));
// 		XPathReader xr = new XPathReader(reader, xc);
// 		ArrayList matchList = new ArrayList();

// 		while (xr.ReadUntilMatch())
// 		{
// 			xr.MatchesAny(matchList);
// 			foreach (int i in matchList)
// 			{
// 				DisplayMatchingNode(xc[i], xr);
// 			}
// 		}
// 	}

// 	void BizTalk2()
// 	{
// 		XPathCollection xc = new XPathCollection();
// 		xc.NamespaceManager = new XmlNamespaceManager(new NameTable());
// 		xc.NamespaceManager.AddNamespace("xe", "http://PropPromotion.XmlEnvelope");
// 		xc.NamespaceManager.AddNamespace("xb", "http://PropPromotion.XmlBody");

// 		// 1) /XmlEnvelope/Attributes/@integer
// 		xc.Add("/xe:XmlEnvelope/xe:Attributes/@integer");
// 		// 2) /XmlEnvelope/Attributes/@language
// 		xc.Add("/xe:XmlEnvelope/xe:Attributes/@language");
// 		// 3) /XmlEnvelope/FieldElements/anyuri
// 		xc.Add("/xe:XmlEnvelope/xe:FieldElements/@anyuri");
// 		// 4) /XmlEnvelope/FieldElements/boolean
// 		xc.Add("/xe:XmlEnvelope/xe:FieldElements/xe:boolean");
// 		// 5) /XmlEnvelope/Body
// 		xc.Add("/xe:XmlEnvelope/xe:Body");
// 		// 6) /XmlEnvelope/Body/Attributes/@integer
// 		xc.Add("/xe:XmlEnvelope/xe:Body/xb:XmlBody/xb:Attributes/@integer");
// 		// 7) /XmlEnvelope/Body/Attributes/@language
// 		xc.Add("/xe:XmlEnvelope/xe:Body/xb:XmlBody/xb:FieldElements/xb:anyuri");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(@"
//               <XmlEnvelope xmlns='http://PropPromotion.XmlEnvelope'>
//                  <Attributes integer='57604' language='en-us'/>
//                     <FieldElements>
//                        <anyuri>ftp://bla.bla</anyuri>
//                        <boolean>1</boolean>
//                     </FieldElements>
//                     <Body>
//                        <XmlBody xmlns='http://PropPromotion.XmlBody'>
//                           <Attributes integer='-3368' language='fr-ca' />
//                           <FieldElements>
//                              <anyuri>ftp://yes.yes</anyuri>
//                              <boolean>false</boolean>
//                           </FieldElements>
//                        </XmlBody>
//                     </Body>
//                </XmlEnvelope>"));
// 		XPathReader xr = new XPathReader(reader, xc);
// 		ArrayList matchList = new ArrayList();

// 		while (xr.ReadUntilMatch())
// 		{
// 			xr.MatchesAny(matchList);
// 			foreach (int i in matchList)
// 			{
// 				DisplayMatchingNode(xc[i], xr);
// 			}
// 		}
// 	}

// 	public void LoanBook()
// 	{
// 		XPathCollection xc = new XPathCollection();

// 		xc.Add("//book/title");

// 		XmlTextReader reader = new XmlTextReader(new StringReader(@"
//                <books>
//                   <book publisher='IDG books' on-loan='Sanjay'>
//                      <title>XML Bible</title>
//                      <author>Elliotte Rusty Harold</author>
//                   </book>
//                   <book publisher='Addison-Wesley'>
//                      <title>The Mythical Man Month</title>
//                      <author>Frederick Brooks</author>
//                   </book>
//                   <book publisher='WROX'>
//                      <title>Professional XSLT 2nd Edition</title>
//                      <author>Michael Kay</author>
//                   </book>
//                   <book publisher='Prentice Hall' on-loan='Sander' >
//                      <title>Definitive XML Schema</title>
//                      <author>Priscilla Walmsley</author>
//                   </book>
//                   <book publisher='APress'>
//                      <title>A Programmer's Introduction to C#</title>
//                      <author>Eric Gunnerson</author>
//                   </book>
//                 </books>"));
// 		XPathReader xr = new XPathReader(reader, xc);
// 		ArrayList matchList = new ArrayList();

// 		while (xr.ReadUntilMatch())
// 		{
// 			Console.WriteLine(xr.ReadString());
// 		}
// 	}


// 	public void LoanBook2()
// 	{
// 		Console.WriteLine("\nLoanBook2");
// 		XmlTextReader reader = new XmlTextReader(new StringReader(@"
//                <books>
//                   <book publisher='IDG books' on-loan='Sanjay'>
//                      <title>XML Bible</title>
//                      <author>Elliotte Rusty Harold</author>
//                   </book>
//                   <book publisher='Addison-Wesley'>
//                      <title>The Mythical Man Month</title>
//                      <author>Frederick Brooks</author>
//                   </book>
//                   <book publisher='WROX'>
//                      <title>Professional XSLT 2nd Edition</title>
//                      <author>Michael Kay</author>
//                   </book>
//                   <book publisher='Prentice Hall' on-loan='Sander' >
//                      <title>Definitive XML Schema</title>
//                      <author>Priscilla Walmsley</author>
//                   </book>
//                   <book publisher='APress'>
//                      <title>A Programmer's Introduction to C#</title>
//                      <author>Eric Gunnerson</author>
//                   </book>
//                 </books>"));

// 		XPathCollection xc = new XPathCollection();
// 		int onloanQuery = xc.Add("/books/book[@on-loan]");
// 		int titleQuery = xc.Add("/books/book[@on-loan]/title");
// 		int authorQuery = xc.Add("/books/book[@on-loan]/author");

// 		XPathReader xpr = new XPathReader(reader, xc);

// 		while (xpr.ReadUntilMatch())
// 		{

// 			if (xpr.Match(onloanQuery))
// 			{
// 				Console.Write("\n {0} was loaned ", xpr.GetAttribute("on-loan"));
// 			}
// 			else if (xpr.Match(titleQuery))
// 			{
// 				Console.Write(xpr.ReadString());
// 			}
// 			else if (xpr.Match(authorQuery))
// 			{
// 				Console.WriteLine(" by {0}", xpr.ReadString());
// 			}
// 		}
// 	}

    

// 	public static void Main()
// 	{
// 		TestXPathReader testxpath = new TestXPathReader();
// 		testxpath.LoanBook();
// 		testxpath.LoanBook2();
// 		testxpath.BizTalk1();

// 		testxpath.BizTalk2();

// 		//
// 		// BaseQueries
// 		//
// 		// child
// 		testxpath.ChildAxisEmptyElement();
// 		testxpath.ChildAxis();
// 		testxpath.ChildAxisWithPrefixWithoutNamespaceMgr();
// 		testxpath.ChildAxisWithPrefixWithNamespaceMgr();

// 		// attributes
// 		testxpath.AttributeAxisOnMoveToAttribute();
// 		testxpath.AttributeAxisOnMoveToAttributeFilterElement();
// 		testxpath.AttributeAxisOnRead(); //no support this
// 		testxpath.TestMultipleAttributeAxisOnRead(); //not support this
// 		testxpath.TestMultipleAttributeAxisOnMoveToAttribute();

// 		//self
// 		testxpath.SelfAxis();
// 		//descendent
// 		//testxpath.TestDescendentAxis();
// 		testxpath.DescendentAxisReadUtil();

// 		//Reverse Axis
// 		//Parent
// 		testxpath.Parent();
// 		testxpath.ParentShortSyntax();

// 		//
// 		// ValidatingReader
// 		//
// 		testxpath.ValidatingReader();
// 		testxpath.RecursiveQuery();
        
// 		//
// 		// Predicates
// 		//
		
// 		//MethodOperand
// 		testxpath.TestMethodOperandCount();
// 		testxpath.TestMethodOperandPosition();
// 		testxpath.TestMethodOperandNamespaceUri();
// 		//testxpath.TestMethodOperandName();
// 		//testxpath.TestMethodOperandLocalName();
// 		// numberic expression
// 		testxpath.NumericExpressionAddMinutes();
// 		testxpath.TestNumericExpressionMutliDiv();
// 		// Logical Expression
// 		testxpath.TestLogicalExpression();
// 		// String
// 		testxpath.TestStringExpression();
// 		// NodeSet
// 		testxpath.TestNodeSetExpression();
		
// 		//
// 		// Functions
// 		//
// 		//string functions
// 		testxpath.TestStringFunctions();
// 		//number functions
// 		testxpath.TestNumberFunctions();
// 		//boolean functions
// 		testxpath.TestBooleanFunctions();
// 		//node functions
// 		testxpath.TestNodeSetFunctions();
		
        
        
        
// 		// readUntil Method.
// 		testxpath.TestReadUntil();
// 		// MatchAny
// 		testxpath.TestMatchAny();
// 		testxpath.TestMultipleLevelDoc();
// 		// Mix read and readUntil
// 		//testxpath.TestMixRead();
// 		//
// 		testxpath.LogicalExprAttributeRead();
// 		testxpath.LogicalExprAttributeReadUntil();




// 		//testxpath.TestMultipleDocument();
// 		testxpath.TestRemoveXPathByIndex();
// 		testxpath.TestRemoveXPathByExpression();

// 		testxpath.RecursiveQuery();
// 	}
// }
