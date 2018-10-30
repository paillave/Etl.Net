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
    public class LeftJoinTests
    {
        [TestCategory(nameof(LeftJoinTests))]
        [TestMethod]
        public void SimpleLeftJoin()
        {
            #region simple left join
            var inputLeftList = Enumerable.Range(0, 100).ToList();
            var inputRightList = Enumerable.Range(0, 10).ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<object>(rootStream =>
            {
                var leftStream = rootStream
                    .CrossApplyEnumerable("input left elements", _ => inputLeftList)
                    .EnsureSorted("ensure left is sorted", i => i / 10);
                var rightStream = rootStream
                    .CrossApplyEnumerable("input right elements", _ => inputRightList)
                    .EnsureKeyed("ensure right is keyed", i => i);
                leftStream
                    .LeftJoin("join left and right", rightStream, (left, right) => $"{left}-{right}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

            var expected = Enumerable.Range(0, 100).Select(i => $"{i}-{i / 10}").ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }

        //TODO: others tests for LeftJoinTests
    }
}
