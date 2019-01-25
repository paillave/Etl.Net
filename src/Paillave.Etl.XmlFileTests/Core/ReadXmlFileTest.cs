using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.XmlFile.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Paillave.Etl.XmlFileTests.Core
{
    [TestClass]
    public class ReadXmlFileTest
    {
        [TestMethod]
        public void ConfigurationFieldTest()
        {
            var xmlNodeDef = XmlNodeDefinition.Create("test", "/root/test", i => new
            {
                Prop1 = i.ToXPathQuery<DateTime>("azer1"),
                Prop2 = i.ToXPathQuery<int>("azer2"),
                Prop3 = i.ToXPathQuery<string>("azer3"),
                Prop4 = i.ToXPathQuery<decimal>("azer4"),
                Prop5 = i.ToXPathQuery<bool>("azer5"),
            });
            CollectionAssert.AreEquivalent(
                new[] { "azer1", "azer2", "azer3", "azer4", "azer5" },
                xmlNodeDef.GetXmlFieldDefinitions().Select(i => i.XPathQuery).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "Prop1", "Prop2", "Prop3", "Prop4", "Prop5" },
                xmlNodeDef.GetXmlFieldDefinitions().Select(i => i.TargetPropertyInfo.Name).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { typeof(DateTime), typeof(int), typeof(string), typeof(decimal), typeof(bool) },
                xmlNodeDef.GetXmlFieldDefinitions().Select(i => i.TargetPropertyInfo.PropertyType).ToArray());
        }

        private class BookTest
        {
            public string CollectionName { get; set; }
            public int CollectionSize { get; set; }
            public int Id { get; set; }
            public string Publisher { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
        }

        private class LoanedBookTest
        {
            public string CollectionName { get; set; }
            public int CollectionSize { get; set; }
            public int Id { get; set; }
            public string Publisher { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Loaner { get; set; }
        }

        [TestMethod]
        public void TestReadXml()
        {
            XmlTextReader reader = new XmlTextReader(new StringReader(@"
                <books>
                  <collection name='col1'>
                    <caracs size='6546' />
                    <book publisher='Addison-Wesley'>
                      <title>The Mythical Man Month</title>
                      <author>Frederick Brooks</author>
                      <id>1</id>
                    </book>
                    <loanedbook publisher='IDG books' on-loan='Sanjay'>
                      <title>XML Bible</title>
                      <author>Elliotte Rusty Harold</author>
                      <id>2</id>
                    </loanedbook>
                  </collection>
                  <collection name='col2'>
                    <caracs size='524' />
                    <book publisher='APress'>
                      <title>A Programmer's Introduction to C#</title>
                      <author>Eric Gunnerson</author>
                      <id>3</id>
                    </book>
                    <loanedbook publisher='Prentice Hall' on-loan='Sander'>
                      <title>Definitive XML Schema</title>
                      <author>Priscilla Walmsley</author>
                      <id>4</id>
                    </loanedbook>
                    <book publisher='WROX'>
                      <title>Professional XSLT 2nd Edition</title>
                      <author>Michael Kay</author>
                      <id>5</id>
                    </book>
                  </collection>
                  <collection name='col3'>
                    <book publisher='APress'>
                      <title>A Programmer's Introduction to C#</title>
                      <author>Eric Gunnerson</author>
                      <id>6</id>
                    </book>
                    <loanedbook publisher='Prentice Hall' on-loan='Sander'>
                      <title>Definitive XML Schema</title>
                      <author>Priscilla Walmsley</author>
                      <id>7</id>
                    </loanedbook>
                  </collection>
                  <collection>
                    <caracs size='3' />
                    <book publisher='WROX'>
                      <title>Professional XSLT 2nd Edition</title>
                      <author>Michael Kay</author>
                      <id>8</id>
                    </book>
                  </collection>
                </books>"));
            var bookNodeDefinition = XmlNodeDefinition.Create("book", "/books/collection/book", i => new BookTest
            {
                Id = i.ToXPathQuery<int>("/books/collection/book/id"),
                Author = i.ToXPathQuery<string>("/books/collection/book/author"),
                Publisher = i.ToXPathQuery<string>("/books/collection/book/@publisher"),
                Title = i.ToXPathQuery<string>("/books/collection/book/title"),
                CollectionName = i.ToXPathQuery<string>("/books/collection/@name"),
                CollectionSize = i.ToXPathQuery<int>("/books/collection/caracs/@size", -1)
            });
            var loanedBookNodeDefinition = XmlNodeDefinition.Create("loanedbook", "/books/collection/loanedbook", i => new LoanedBookTest
            {
                Id = i.ToXPathQuery<int>("/books/collection/loanedbook/id"),
                Author = i.ToXPathQuery<string>("/books/collection/loanedbook/author"),
                Publisher = i.ToXPathQuery<string>("/books/collection/loanedbook/@publisher"),
                Title = i.ToXPathQuery<string>("/books/collection/loanedbook/title"),
                Loaner = i.ToXPathQuery<string>("/books/collection/loanedbook/@on-loan"),
                CollectionName = i.ToXPathQuery<string>("/books/collection/@name"),
                CollectionSize = i.ToXPathQuery<int>("/books/collection/caracs/@size", -1)
            });
            List<XmlNodeParsed> xmlNodeParseds = new List<XmlNodeParsed>();
            XmlObjectReader xmlObjectReader = new XmlObjectReader(reader, new XmlFileDefinition().AddNodeDefinition(bookNodeDefinition).AddNodeDefinition(loanedBookNodeDefinition), xmlNodeParseds.Add);
            xmlObjectReader.Read();
            CollectionAssert.AreEquivalent(new[] { "book", "loanedbook", "book", "loanedbook", "book", "book", "loanedbook", "book" }, xmlNodeParseds.Select(i => i.Name).ToArray());
            CollectionAssert.AreEquivalent(new[] { typeof(BookTest), typeof(LoanedBookTest), typeof(BookTest), typeof(LoanedBookTest), typeof(BookTest), typeof(BookTest), typeof(LoanedBookTest), typeof(BookTest) }, xmlNodeParseds.Select(i => i.Type).ToArray());

            var book0 = xmlNodeParseds[0].GetValue<BookTest>();
            Assert.AreEqual(1, book0.Id);
            Assert.AreEqual("Addison-Wesley", book0.Publisher);
            Assert.AreEqual("The Mythical Man Month", book0.Title);
            Assert.AreEqual("Frederick Brooks", book0.Author);
            Assert.AreEqual("col1", book0.CollectionName);
            Assert.AreEqual(6546, book0.CollectionSize);

            var book1 = xmlNodeParseds[1].GetValue<LoanedBookTest>();
            Assert.AreEqual(2, book1.Id);
            Assert.AreEqual("Sanjay", book1.Loaner);
            Assert.AreEqual("IDG books", book1.Publisher);
            Assert.AreEqual("XML Bible", book1.Title);
            Assert.AreEqual("Elliotte Rusty Harold", book1.Author);
            Assert.AreEqual("col1", book1.CollectionName);
            Assert.AreEqual(6546, book1.CollectionSize);

            var book2 = xmlNodeParseds[2].GetValue<BookTest>();
            Assert.AreEqual(3, book2.Id);
            Assert.AreEqual("APress", book2.Publisher);
            Assert.AreEqual("A Programmer's Introduction to C#", book2.Title);
            Assert.AreEqual("Eric Gunnerson", book2.Author);
            Assert.AreEqual("col2", book2.CollectionName);
            Assert.AreEqual(524, book2.CollectionSize);

            var book3 = xmlNodeParseds[3].GetValue<LoanedBookTest>();
            Assert.AreEqual(4, book3.Id);
            Assert.AreEqual("Sander", book3.Loaner);
            Assert.AreEqual("Prentice Hall", book3.Publisher);
            Assert.AreEqual("Definitive XML Schema", book3.Title);
            Assert.AreEqual("Priscilla Walmsley", book3.Author);
            Assert.AreEqual("col2", book3.CollectionName);
            Assert.AreEqual(524, book3.CollectionSize);

            var book4 = xmlNodeParseds[4].GetValue<BookTest>();
            Assert.AreEqual(5, book4.Id);
            Assert.AreEqual("WROX", book4.Publisher);
            Assert.AreEqual("Professional XSLT 2nd Edition", book4.Title);
            Assert.AreEqual("Michael Kay", book4.Author);
            Assert.AreEqual("col2", book4.CollectionName);
            Assert.AreEqual(524, book4.CollectionSize);

            var book5 = xmlNodeParseds[5].GetValue<BookTest>();
            Assert.AreEqual(6, book5.Id);
            Assert.AreEqual("APress", book5.Publisher);
            Assert.AreEqual("A Programmer's Introduction to C#", book5.Title);
            Assert.AreEqual("Eric Gunnerson", book5.Author);
            Assert.AreEqual("col3", book5.CollectionName);
            Assert.AreEqual(0, book5.CollectionSize);

            var book6 = xmlNodeParseds[6].GetValue<LoanedBookTest>();
            Assert.AreEqual(7, book6.Id);
            Assert.AreEqual("Sander", book6.Loaner);
            Assert.AreEqual("Prentice Hall", book6.Publisher);
            Assert.AreEqual("Definitive XML Schema", book6.Title);
            Assert.AreEqual("Priscilla Walmsley", book6.Author);
            Assert.AreEqual("col3", book6.CollectionName);
            Assert.AreEqual(0, book6.CollectionSize);

            var book7 = xmlNodeParseds[7].GetValue<BookTest>();
            Assert.AreEqual(8, book7.Id);
            Assert.AreEqual("WROX", book7.Publisher);
            Assert.AreEqual("Professional XSLT 2nd Edition", book7.Title);
            Assert.AreEqual("Michael Kay", book7.Author);
            Assert.AreEqual(null, book7.CollectionName);
            Assert.AreEqual(3, book7.CollectionSize);
        }
    }
}
