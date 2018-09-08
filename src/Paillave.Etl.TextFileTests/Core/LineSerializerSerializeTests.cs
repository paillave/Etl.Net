using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.TextFile.Core;

namespace Paillave.EtlTests.TextFileTests.Core
{
    [TestClass]
    public class LineSerializerSerializeTests
    {
        private class MyClass
        {
            public int MyProperty1 { get; set; }
            public string MyProperty2 { get; set; }
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerSerializeTests))]
        public void SerializeColumnSeparatedWithDefaultSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>().GetSerializer();
            var res = lineSerializer.Serialize(new MyClass { MyProperty1 = 1, MyProperty2 = "2" });
            Assert.AreEqual("1;2", res);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerSerializeTests))]
        public void SerializeFixedColumnWithDefaultSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>().HasFixedColumnWidth(-3, -5).GetSerializer();
            var res = lineSerializer.Serialize(new MyClass { MyProperty1 = 1, MyProperty2 = "2" });
            Assert.AreEqual("1  2    ", res);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerSerializeTests))]
        public void ByNameSerializeColumnSeparatedWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .IsColumnSeparated(',')
                .MapColumnToProperty("P1", 2, i => i.MyProperty1)
                .MapColumnToProperty("P2", 1, i => i.MyProperty2)
                .GetSerializer();
            var res = lineSerializer.Serialize(new MyClass { MyProperty1 = 1, MyProperty2 = "2" });
            Assert.AreEqual("2,1", res);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerSerializeTests))]
        public void ByNameSerializeFixedWidthWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .HasFixedColumnWidth(-3, -5)
                .MapColumnToProperty("P1", 2, i => i.MyProperty1)
                .MapColumnToProperty("P2", 1, i => i.MyProperty2)
                .GetSerializer();
            var res = lineSerializer.Serialize(new MyClass { MyProperty1 = 1, MyProperty2 = "2" });
            Assert.AreEqual("2  1    ", res);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerSerializeTests))]
        public void ByPositionSerializeColumnSeparatedWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .IsColumnSeparated(',')
                .MapColumnToProperty(2, i => i.MyProperty1)
                .MapColumnToProperty(1, i => i.MyProperty2)
                .GetSerializer();
            var res = lineSerializer.Serialize(new MyClass { MyProperty1 = 1, MyProperty2 = "2" });
            Assert.AreEqual("2,1", res);
        }

        [TestMethod]
        [TestCategory(nameof(LineSerializerSerializeTests))]
        public void ByPositionSerializeFixedWidthWithGivenSettings()
        {
            var lineSerializer = new FlatFileDefinition<MyClass>()
                .HasFixedColumnWidth(-3, -5)
                .MapColumnToProperty(2, i => i.MyProperty1)
                .MapColumnToProperty(1, i => i.MyProperty2)
                .GetSerializer();
            var res = lineSerializer.Serialize(new MyClass { MyProperty1 = 1, MyProperty2 = "2" });
            Assert.AreEqual("2  1    ", res);
        }
    }
}
