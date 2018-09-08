using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.TextFile.Core;

namespace Paillave.EtlTests.TextFileTests.Core
{
    [TestClass]
    public class FileDefinitionHeaderTests
    {
        private class MyClass
        {
            public int MyProperty1 { get; set; }
            public string MyProperty2 { get; set; }
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithIndex1()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty(1, i => i.MyProperty2);
            fd.MapColumnToProperty(0, i => i.MyProperty1);
            Assert.AreEqual("MyProperty1;MyProperty2", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithIndex2()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty(0, i => i.MyProperty2);
            fd.MapColumnToProperty(1, i => i.MyProperty1);
            Assert.AreEqual("MyProperty2;MyProperty1", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithName1()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty("prop1", i => i.MyProperty1);
            fd.MapColumnToProperty("prop2", i => i.MyProperty2);
            Assert.AreEqual("prop1;prop2", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithName2()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty("prop2", i => i.MyProperty2);
            fd.MapColumnToProperty("prop1", i => i.MyProperty1);
            Assert.AreEqual("prop2;prop1", fd.GenerateDefaultHeaderLine());
        }

        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithNameAndIndex1()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty(1, i => i.MyProperty1);
            fd.MapColumnToProperty("prop1", i => i.MyProperty1);
            fd.MapColumnToProperty(0, i => i.MyProperty2);
            fd.MapColumnToProperty("prop2", i => i.MyProperty2);
            Assert.AreEqual("prop2;prop1", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithNameAndIndex2()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty(0, i => i.MyProperty1);
            fd.MapColumnToProperty("prop1", i => i.MyProperty1);
            fd.MapColumnToProperty(1, i => i.MyProperty2);
            fd.MapColumnToProperty("prop2", i => i.MyProperty2);
            Assert.AreEqual("prop1;prop2", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithNameAndPartialIndex()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty(1, i => i.MyProperty2);
            fd.MapColumnToProperty("prop2", i => i.MyProperty2);
            fd.MapColumnToProperty("prop1", i => i.MyProperty1);
            Assert.AreEqual("prop2;prop1", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithNameAndIndex3()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty("prop1", 1, i => i.MyProperty1);
            fd.MapColumnToProperty("prop2", 0, i => i.MyProperty2);
            Assert.AreEqual("prop2;prop1", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithNameAndIndex4()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty("prop1", 0, i => i.MyProperty1);
            fd.MapColumnToProperty("prop2", 1, i => i.MyProperty2);
            Assert.AreEqual("prop1;prop2", fd.GenerateDefaultHeaderLine());
        }
        [TestMethod]
        [TestCategory(nameof(FileDefinitionHeaderTests))]
        public void GenerateDefaultHeaderWithNameAndPartialIndex2()
        {
            var fd = new FlatFileDefinition<MyClass>();
            fd.IsColumnSeparated(';');
            fd.MapColumnToProperty("prop2", 0, i => i.MyProperty2);
            fd.MapColumnToProperty("prop1", i => i.MyProperty1);
            Assert.AreEqual("prop2;prop1", fd.GenerateDefaultHeaderLine());
        }
    }
}
