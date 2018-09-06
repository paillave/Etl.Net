using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.TextFile.Core;

namespace Paillave.EtlTests.TextFileTests.Core
{
    [TestClass]
    public class ColumnSeparatorLineSplitterTests
    {
        [TestMethod]
        [TestCategory(nameof(ColumnSeparatorLineSplitterTests))]
        public void ColumnSeparatorLineSplit()
        {
            ILineSplitter splitter = new ColumnSeparatorLineSplitter(';');
            CollectionAssert.AreEquivalent(new[] { "22", "55555", "333", "666666" }, splitter.Split("22;55555;333;666666").ToArray());
            CollectionAssert.AreEquivalent(new[] { "22", "55;555", "333", "666666" }, splitter.Split("22;\"55;555\";333;666666").ToArray());
        }

        [TestMethod]
        [TestCategory(nameof(ColumnSeparatorLineSplitterTests))]
        public void ColumnSeparatorLineJoin()
        {
            ILineSplitter splitter = new ColumnSeparatorLineSplitter(';');
            Assert.AreEqual("22;55555;333;666666", splitter.Join(new[] { "22", "55555", "333", "666666" }));
            Assert.AreEqual("22;\"55;555\";333;666666", splitter.Join(new[] { "22", "55;555", "333", "666666" }));
        }
    }
}
