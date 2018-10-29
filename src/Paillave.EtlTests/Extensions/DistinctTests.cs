using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests.Extensions
{
    [TestClass()]
    public class DistinctTests
    {
        [TestCategory(nameof(DistinctTests))]
        [TestMethod]
        public void SimpleDistinct()
        {
            #region simple distinct
            var inputList = Enumerable.Range(0, 100).Select(i => i).ToList();
            var outputList = new List<int>();

            StreamProcessRunner.Create<object>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", _ => inputList)
                    .Distinct("distinct", i => i % 10)
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

            var expected = Enumerable.Range(0, 10).Select(i => i).ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }
        [TestCategory(nameof(DistinctTests))]
        [TestMethod]
        public void SimpleSortedDistinct()
        {
            #region simple distinct on sorted
            var inputList = Enumerable.Range(0, 100).Select(i => i % 10).OrderBy(i => i).ToList();
            var outputList = new List<int>();

            StreamProcessRunner.Create<object>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", _ => inputList)
                    .Sort("sort output", i => i)
                    .Distinct("distinct")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

            var expected = Enumerable.Range(0, 10).Select(i => i).ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }
    }
}
