using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.TextFile.Core;

namespace Paillave.Etl.TextFileTests.Core
{
    [TestClass]
    public class FixedColumnWidthLineSplitterTests
    {
        [TestMethod]
        [TestCategory(nameof(FixedColumnWidthLineSplitterTests))]
        public void FixedColumnWidthLineSplit()
        {
            ILineSplitter splitter = new FixedColumnWidthLineSplitter(2, 5, 3, 6);
            CollectionAssert.AreEquivalent(new[] { "22", "55555", "333", "666666" }, splitter.Split("2255555333666666").ToArray());
        }

        [TestMethod]
        [TestCategory(nameof(FixedColumnWidthLineSplitterTests))]
        public void FixedColumnWidthLineJoin()
        {
            ILineSplitter splitter = new FixedColumnWidthLineSplitter(2, 5, 3, 6);
            Assert.AreEqual("2255555333666666", splitter.Join(new[] { "22", "55555", "333", "666666" }));
        }




        [TestMethod]
        [TestCategory(nameof(FixedColumnWidthLineSplitterTests))]
        public void WithSpaceFixedColumnWidthLineSplit()
        {
            ILineSplitter splitter = new FixedColumnWidthLineSplitter(-2, -5, -3, -6);
            CollectionAssert.AreEquivalent(new[] { "2 ", "5    ", "3  ", "6     " }, splitter.Split("2 5    3  6     ").ToArray());
        }

        [TestMethod]
        [TestCategory(nameof(FixedColumnWidthLineSplitterTests))]
        public void WithSpaceFixedColumnWidthLineJoin()
        {
            ILineSplitter splitter = new FixedColumnWidthLineSplitter(-2, -5, -3, -6);
            Assert.AreEqual("2 5    3  6     ", splitter.Join(new[] { "2", "5", "3", "6" }));
        }


        [TestMethod]
        [TestCategory(nameof(FixedColumnWidthLineSplitterTests))]
        public void RightAlignedWithSpaceFixedColumnWidthLineJoin()
        {
            ILineSplitter splitter = new FixedColumnWidthLineSplitter(2, 5, 3, 6);
            Assert.AreEqual(" 2    5  3     6", splitter.Join(new[] { "2", "5", "3", "6" }));
        }
    }
}
