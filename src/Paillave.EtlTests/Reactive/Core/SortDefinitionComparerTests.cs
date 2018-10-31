using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;

namespace Paillave.EtlTests.Core
{
    [TestClass()]
    public class SortDefinitionComparerTests
    {
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void AscIntComparison()
        {
            var comp = CreateSortComp(0, "", i => i.ToString(), i => i);
            Assert.IsTrue(comp.Compare(1, "1") == 0);
            Assert.IsTrue(comp.Compare(1, "0") > 0);
            Assert.IsTrue(comp.Compare(0, "1") < 0);
        }
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void DescIntComparison()
        {
            var comp = CreateSortComp(0, "", i => i.ToString(), i => i, -1);
            Assert.IsTrue(comp.Compare(1, "1") == 0);
            Assert.IsTrue(comp.Compare(1, "0") < 0);
            Assert.IsTrue(comp.Compare(0, "1") > 0);
        }
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void SimpleObjectComparison()
        {
            var comp = CreateSortComp(new { I1 = 0, I2 = 0 }, new { i2 = 0, i1 = 0 }, i => new { A = i.I1, B = i.I2 }, i => new { A = i.i1, B = i.i2 });

            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 1 }) == 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 1 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 0 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 0 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 0 }, new { i2 = 1, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 1 }, new { i2 = 1, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 0 }, new { i2 = 1, i1 = 1 }) < 0);
        }

        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison()
        {
            var comp = CreateSortComp(new { I1 = 0, I2 = 0 }, new { i2 = 0, i1 = 0 }, i => new { A = i.I1, B = i.I2 }, i => new { A = i.i1, B = i.i2 }, new { B = 1, A = 2 });

            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 1 }) == 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 0 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 1 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 0 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 1 }, new { i2 = 1, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 0 }, new { i2 = 1, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 0 }, new { i2 = 1, i1 = 1 }) < 0);
        }

        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison2()
        {
            var comp = CreateSortComp(new { I1 = 0, I2 = 0 }, new { i2 = 0, i1 = 0 }, i => new { A = i.I1, B = i.I2 }, i => new { A = i.i1, B = i.i2 }, new { B = -1, A = 2 });

            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 1 }) == 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 0 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 0 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 1 }, new { i2 = 1, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 0 }, new { i2 = 1, i1 = 1 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 0 }, new { i2 = 1, i1 = 1 }) > 0);
        }

        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison3()
        {
            var comp = CreateSortComp(new { I1 = 0, I2 = 0 }, new { i2 = 0, i1 = 0 }, i => new { A = i.I1, B = i.I2 }, i => new { A = i.i1, B = i.i2 }, new { B = -1, A = -2 });

            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 1 }) == 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 0 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 0 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 1 }, new { i2 = 1, i1 = 1 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 0 }, new { i2 = 1, i1 = 1 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 0 }, new { i2 = 1, i1 = 1 }) > 0);
        }

        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison4()
        {
            var comp = CreateSortComp(new { I1 = 0, I2 = 0 }, new { i2 = 0, i1 = 0 }, i => new { A = i.I1, B = i.I2 }, i => new { A = i.i1, B = i.i2 }, new { B = 1, A = -2 });

            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 1 }) == 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 1, i1 = 0 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 1 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 1 }, new { i2 = 0, i1 = 0 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 1 }, new { i2 = 1, i1 = 1 }) > 0);
            Assert.IsTrue(comp.Compare(new { I1 = 1, I2 = 0 }, new { i2 = 1, i1 = 1 }) < 0);
            Assert.IsTrue(comp.Compare(new { I1 = 0, I2 = 0 }, new { i2 = 1, i1 = 1 }) < 0);
        }

        private static SortDefinitionComparer<T1, T2, TKey> CreateSortComp<T1, T2, TKey>(T1 prototype1, T2 prototype2, Func<T1, TKey> getKey1, Func<T2, TKey> getKey2, object keyPosition = null)
        {
            return new SortDefinitionComparer<T1, T2, TKey>(SortDefinition.Create<T1, TKey>(getKey1, keyPosition), SortDefinition.Create<T2, TKey>(getKey2, keyPosition));
        }
        private static SortDefinition<T, TKey> CreateSortDef<T, TKey>(T prototype, Func<T, TKey> getKey, object keyPosition = null)
        {
            return SortDefinition.Create<T, TKey>(getKey, keyPosition);
        }
    }
}
