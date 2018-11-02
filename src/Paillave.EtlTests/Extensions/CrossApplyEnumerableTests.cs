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
    public class CrossApplyEnumerableTests
    {
        [TestCategory(nameof(CrossApplyEnumerableTests))]
        [TestMethod]
        public void ProduceList()
        {
            #region produce list
            var inputList = Enumerable.Range(0, 100).ToList();
            var outputList = new List<int>();

            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            CollectionAssert.AreEquivalent(inputList, outputList);
            #endregion
        }

        [TestCategory(nameof(CrossApplyEnumerableTests))]
        [TestMethod]
        public void ProduceListWithOneItem()
        {
            #region produce list with one "resource" item
            var inputList = Enumerable.Range(0, 100).ToList();
            var inputResource = 1;
            var outputList = new List<int>();

            StreamProcessRunner.CreateAndExecuteAsync(new
            {
                InputResource = inputResource,
                InputList = inputList
            }, rootStream =>
            {
                var resourceStream = rootStream.Select("get another resource", config => config.InputResource);
                rootStream
                    .CrossApplyEnumerable("list elements", resourceStream, (config, res) => config.InputList.Select(i => i + res).ToList())
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            CollectionAssert.AreEquivalent(inputList.Select(i => i + inputResource).ToList(), outputList);
            #endregion
        }
    }
}
