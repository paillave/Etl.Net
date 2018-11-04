using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests.Extensions
{
    [TestClass()]
    public class SelectTests
    {
        [TestCategory(nameof(SelectTests))]
        [TestMethod]
        public void SimpleSelect()
        {
            #region simple select
            var inputList = Enumerable.Range(0, 10);
            var outputList = new List<string>();
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Select("issue new value", i => (i * 2).ToString())
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();
            var expectedList = inputList.Select(i => (i * 2).ToString()).ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }
        [TestCategory(nameof(SelectTests))]
        [TestMethod]
        public void SimpleSelectWithIndex()
        {
            #region simple select
            var inputList = Enumerable.Range(0, 10);
            var outputList = new List<string>();
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Select("issue new value", (i, idx) => $"{i}-{idx}")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();
            var expectedList = inputList.Select((i, idx) => $"{i}-{idx}").ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }

        // TODO: Finish select tests
    }
}