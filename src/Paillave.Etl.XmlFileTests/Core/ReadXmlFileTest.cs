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
            public int Id { get; set; }
            public string Publisher { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
        }

        private class LoanedBookTest
        {
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
                  <loanedbook publisher='IDG books' on-loan='Sanjay'>
<id>1</id>
                     <title>XML Bible</title>
                     <author>Elliotte Rusty Harold</author>
                  </loanedbook>
                  <book publisher='Addison-Wesley'>
<id>2</id>
                     <title>The Mythical Man Month</title>
                     <author>Frederick Brooks</author>
                  </book>
                  <book publisher='WROX'>
<id>3</id>
                     <title>Professional XSLT 2nd Edition</title>
                     <author>Michael Kay</author>
                  </book>
                  <loanedbook  publisher='Prentice Hall' on-loan='Sander' >
<id>4</id>
                     <title>Definitive XML Schema</title>
                     <author>Priscilla Walmsley</author>
                  </loanedbook>
                  <book publisher='APress'>
<id>5</id>
                     <title>A Programmer's Introduction to C#</title>
                     <author>Eric Gunnerson</author>
                  </book>
                </books>"));
            var bookNodeDefinition = XmlNodeDefinition.Create("book", "/books/book", i => new BookTest
            {
                Id = i.ToXPathQuery<int>("/books/book/id"),
                Author = i.ToXPathQuery<string>("/books/book/author"),
                Publisher = i.ToXPathQuery<string>("/books/book/@publisher"),
                Title = i.ToXPathQuery<string>("/books/book/title"),
            });
            var loanedBookNodeDefinition = XmlNodeDefinition.Create("loanedbook", "/books/loanedbook", i => new LoanedBookTest
            {
                Id = i.ToXPathQuery<int>("/books/loanedbook/id"),
                Author = i.ToXPathQuery<string>("/books/loanedbook/author"),
                Publisher = i.ToXPathQuery<string>("/books/loanedbook/@publisher"),
                Title = i.ToXPathQuery<string>("/books/loanedbook/title"),
                Loaner = i.ToXPathQuery<string>("/books/loanedbook/@on-loan")
            });
            List<XmlNodeParsed> xmlNodeParseds = new List<XmlNodeParsed>();
            XmlObjectReader xmlObjectReader = new XmlObjectReader(reader, new XmlFileDefinition().AddNodeDefinition(bookNodeDefinition).AddNodeDefinition(loanedBookNodeDefinition), xmlNodeParseds.Add);
            xmlObjectReader.Read();
            CollectionAssert.AreEquivalent(new[] { "loanedbook", "book", "book", "loanedbook", "book" }, xmlNodeParseds.Select(i => i.Name).ToArray());
            CollectionAssert.AreEquivalent(new[] { typeof(LoanedBookTest), typeof(BookTest), typeof(BookTest), typeof(LoanedBookTest), typeof(BookTest) }, xmlNodeParseds.Select(i => i.Type).ToArray());

            var book0 = xmlNodeParseds[0].GetValue<LoanedBookTest>();
            Assert.AreEqual(1, book0.Id);
            Assert.AreEqual("Sanjay", book0.Loaner);
            Assert.AreEqual("IDG books", book0.Publisher);
            Assert.AreEqual("XML Bible", book0.Title);
            Assert.AreEqual("Elliotte Rusty Harold", book0.Author);

            var book1 = xmlNodeParseds[1].GetValue<BookTest>();
            Assert.AreEqual(2, book1.Id);
            Assert.AreEqual("Addison-Wesley", book1.Publisher);
            Assert.AreEqual("The Mythical Man Month", book1.Title);
            Assert.AreEqual("Frederick Brooks", book1.Author);

            var book2 = xmlNodeParseds[2].GetValue<BookTest>();
            Assert.AreEqual(3, book2.Id);
            Assert.AreEqual("WROX", book2.Publisher);
            Assert.AreEqual("Professional XSLT 2nd Edition", book2.Title);
            Assert.AreEqual("Michael Kay", book2.Author);

            var book3 = xmlNodeParseds[3].GetValue<LoanedBookTest>();
            Assert.AreEqual(4, book3.Id);
            Assert.AreEqual("Sander", book3.Loaner);
            Assert.AreEqual("Prentice Hall", book3.Publisher);
            Assert.AreEqual("Definitive XML Schema", book3.Title);
            Assert.AreEqual("Priscilla Walmsley", book3.Author);

            var book4 = xmlNodeParseds[4].GetValue<BookTest>();
            Assert.AreEqual(5, book4.Id);
            Assert.AreEqual("APress", book4.Publisher);
            Assert.AreEqual("A Programmer's Introduction to C#", book4.Title);
            Assert.AreEqual("Eric Gunnerson", book4.Author);
        }
    }
}
