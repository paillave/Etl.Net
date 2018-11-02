using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Core;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests.Extensions
{
    [TestClass()]
    public class EnsureSingleTests
    {
        [TestCategory(nameof(EnsureSingleTests))]
        [TestMethod]
        public void SuccessfulEnsureSingle()
        {
            #region ensure single without pb
            var inputList = new[] { 1 }.ToList();
            var outputList = new List<int>();

            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .EnsureSingle("ensure single")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            CollectionAssert.AreEquivalent(inputList, outputList);
            #endregion
        }
        [TestCategory(nameof(EnsureSingleTests))]
        [TestMethod]
        public void FailEnsureSingleNotSortedWithNoException()
        {
            #region ensure single not sorted without exception
            var inputList = new[] { 1, 2 }.ToList();
            var outputList = new List<int>();

            var task = StreamProcessRunner.CreateAndExecuteWithNoFaultAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .EnsureSingle("ensure single")
                    .ThroughAction("collect values", outputList.Add);
            });
            task.Wait();

            Assert.IsTrue(task.Result.Failed);
            Assert.IsNotNull(task.Result.ErrorTraceEvents.FirstOrDefault(i => i.NodeName == "ensure single"));
            Assert.IsNotNull(task.Result.StreamStatisticErrors.FirstOrDefault(i => i.NodeName == "ensure single"));
            CollectionAssert.AreEquivalent(new[] { 1 }.ToList(), outputList);
            #endregion
        }
        [TestCategory(nameof(EnsureSingleTests))]
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void FailEnsureSingleNotSorted()
        {
            #region ensure single not sorted
            var inputList = new[] { 1, 2 }.ToList();
            var outputList = new List<int>();

            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .EnsureSingle("ensure single")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();
            #endregion
        }
    }
}
