using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.TextFile.Core;

namespace Paillave.EtlTests.TextFileTests.Core
{
    [TestClass]
    public class LineSerializerDeserializeTests
    {
        private class MyClass
        {
            public int MyProperty1 { get; set; }
            public string MyProperty2 { get; set; }
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerDeserializeTests))]
        public void DeserialiseColumnSeparatedWithDefaultSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>().GetSerializer();
            var res = lineSerializer.Deserialize("1;2");
            Assert.AreEqual(1, res.MyProperty1);
            Assert.AreEqual("2", res.MyProperty2);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerDeserializeTests))]
        public void DeserialiseFixedColumnWithDefaultSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>().HasFixedColumnWidth(3, 5).GetSerializer();
            var res = lineSerializer.Deserialize("1  2    ");
            Assert.AreEqual(1, res.MyProperty1);
            Assert.AreEqual("2", res.MyProperty2);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerDeserializeTests))]
        public void ByNameDeserialiseColumnSeparatedWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .IsColumnSeparated(',')
                .MapColumnToProperty("P1", i => i.MyProperty1)
                .MapColumnToProperty("P2", i => i.MyProperty2)
                .GetSerializer("P2,P1");
            var res = lineSerializer.Deserialize("2,1");
            Assert.AreEqual(1, res.MyProperty1);
            Assert.AreEqual("2", res.MyProperty2);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerDeserializeTests))]
        public void ByNameDeserialiseFixedWidthWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .HasFixedColumnWidth(3, 5)
                .MapColumnToProperty("P1", i => i.MyProperty1)
                .MapColumnToProperty("P2", i => i.MyProperty2)
                .GetSerializer("P2 P1   ");
            var res = lineSerializer.Deserialize("2  1     ");
            Assert.AreEqual(1, res.MyProperty1);
            Assert.AreEqual("2", res.MyProperty2);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerDeserializeTests))]
        public void ByPositionDeserialiseColumnSeparatedWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .IsColumnSeparated(',')
                .MapColumnToProperty(2, i => i.MyProperty1)
                .MapColumnToProperty(1, i => i.MyProperty2)
                .GetSerializer();
            var res = lineSerializer.Deserialize("2,1");
            Assert.AreEqual(1, res.MyProperty1);
            Assert.AreEqual("2", res.MyProperty2);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerDeserializeTests))]
        public void ByPositionDeserialiseFixedWidthWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .HasFixedColumnWidth(3, 5)
                .MapColumnToProperty(2, i => i.MyProperty1)
                .MapColumnToProperty(1, i => i.MyProperty2)
                .GetSerializer("P2 P1   ");
            var res = lineSerializer.Deserialize("2  1     ");
            Assert.AreEqual(1, res.MyProperty1);
            Assert.AreEqual("2", res.MyProperty2);
        }
    }
}
