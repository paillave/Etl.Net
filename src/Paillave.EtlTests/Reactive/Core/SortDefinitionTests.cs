using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;

namespace Paillave.EtlTests.Core
{
    [TestClass()]
    public class SortDefinitionTests
    {
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void AscIntComparison()
        {
            var sortDef = SortDefinition.Create<int, int>(i => i);
            Assert.IsTrue(sortDef.Compare(1, 1) == 0);
            Assert.IsTrue(sortDef.Equals(1, 1));
            Assert.IsTrue(sortDef.Compare(1, 0) > 0);
            Assert.IsFalse(sortDef.Equals(1, 0));
            Assert.IsTrue(sortDef.Compare(0, 1) < 0);
            Assert.IsFalse(sortDef.Equals(0, 1));
        }
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void DescIntComparison()
        {
            var sortDef = SortDefinition.Create<int, int>(i => i, -1);
            Assert.IsTrue(sortDef.Compare(1, 1) == 0);
            Assert.IsTrue(sortDef.Equals(1, 1));
            Assert.IsTrue(sortDef.Compare(1, 0) < 0);
            Assert.IsFalse(sortDef.Equals(1, 0));
            Assert.IsTrue(sortDef.Compare(0, 1) > 0);
            Assert.IsFalse(sortDef.Equals(0, 1));
        }
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void AscStringComparison()
        {
            var sortDef = SortDefinition.Create<string, string>(i => i);
            Assert.IsTrue(sortDef.Compare("1", "1") == 0);
            Assert.IsTrue(sortDef.Equals("1", "1"));
            Assert.IsTrue(sortDef.Compare("1", "0") > 0);
            Assert.IsFalse(sortDef.Equals("1", "0"));
            Assert.IsTrue(sortDef.Compare("0", "1") < 0);
            Assert.IsFalse(sortDef.Equals("0", "1"));
        }
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void DescStringComparison()
        {
            var sortDef = SortDefinition.Create<string, string>(i => i, -1);
            Assert.IsTrue(sortDef.Compare("1", "1") == 0);
            Assert.IsTrue(sortDef.Equals("1", "1"));
            Assert.IsTrue(sortDef.Compare("1", "0") < 0);
            Assert.IsFalse(sortDef.Equals("1", "0"));
            Assert.IsTrue(sortDef.Compare("0", "1") > 0);
            Assert.IsFalse(sortDef.Equals("0", "1"));
        }
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void SimpleObjectComparison()
        {
            var sortDef = CreateSortDef(new { I1 = 0, I2 = 0 }, i => new { A = i.I1, B = i.I2 });

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }) == 0);
            Assert.IsTrue(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }));
        }

        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison()
        {
            var sortDef = CreateSortDef(new { I1 = 0, I2 = 0 }, i => new { A = i.I1, B = i.I2 }, new { B = 1, A = 2 });

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }) == 0);
            Assert.IsTrue(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }));
        }
        
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison2()
        {
            var sortDef = CreateSortDef(new { I1 = 0, I2 = 0 }, i => new { A = i.I1, B = i.I2 }, new { B = -1, A = 2 });

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }) == 0);
            Assert.IsTrue(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }));
        }
        
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison3()
        {
            var sortDef = CreateSortDef(new { I1 = 0, I2 = 0 }, i => new { A = i.I1, B = i.I2 }, new { B = -1, A = -2 });

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }) == 0);
            Assert.IsTrue(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }));
        }
        
        [TestCategory(nameof(SortDefinitionTests))]
        [TestMethod]
        public void CustomObjectComparison4()
        {
            var sortDef = CreateSortDef(new { I1 = 0, I2 = 0 }, i => new { A = i.I1, B = i.I2 }, new { B = 1, A = -2 });

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }) == 0);
            Assert.IsTrue(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 1 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 1, I2 = 0 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 1 }, new { I1 = 0, I2 = 0 }));

            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }) > 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 1 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 1, I2 = 0 }, new { I1 = 1, I2 = 1 }));
            Assert.IsTrue(sortDef.Compare(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }) < 0);
            Assert.IsFalse(sortDef.Equals(new { I1 = 0, I2 = 0 }, new { I1 = 1, I2 = 1 }));
        }

        private static SortDefinition<T, TKey> CreateSortDef<T, TKey>(T prototype, Func<T, TKey> getKey, object keyPosition = null)
        {
            return SortDefinition.Create<T, TKey>(getKey, keyPosition);
        }
    }
}