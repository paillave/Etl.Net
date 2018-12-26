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
    public class EnsureSortedTests
    {
        [TestCategory(nameof(EnsureSortedTests))]
        [TestMethod]
        public void SuccessfulEnsureSorted()
        {
            #region ensure sorted without pb
            var inputList = new[] { 1, 2, 3, 4, 5 }.ToList();
            var outputList = new List<int>();

            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .EnsureSorted("ensure sorted", i => i)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            CollectionAssert.AreEquivalent(inputList, outputList);
            #endregion
        }
        [TestCategory(nameof(EnsureSortedTests))]
        [TestMethod]
        public void SuccessfulEnsureSortedWithDuplicate()
        {
            #region ensure sorted with duplicate
            var inputList = new[] { 1, 2, 2, 3, 4, 5 }.ToList();
            var outputList = new List<int>();

            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .EnsureSorted("ensure sorted", i => i)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            CollectionAssert.AreEquivalent(new[] { 1, 2, 2, 3, 4, 5 }.ToList(), outputList);
            #endregion
        }
        [TestCategory(nameof(EnsureSortedTests))]
        [TestMethod]
        public void FailEnsureSortedNotSortedWithNoException()
        {
            #region ensure sorted not sorted without exception
            var inputList = new[] { 2, 1, 3, 4, 5 }.ToList();
            var outputList = new List<int>();

            var task = StreamProcessRunner.CreateAndExecuteWithNoFaultAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .EnsureSorted("ensure sorted", i => i)
                    .ThroughAction("collect values", outputList.Add);
            });
            task.Wait();

            Assert.IsTrue(task.Result.Failed);
            Assert.IsNotNull(task.Result.EndOfProcessTraceEvent.NodeName == "ensure sorted");
            Assert.IsNotNull(task.Result.StreamStatisticErrors.FirstOrDefault(i => i.NodeName == "ensure sorted"));
            CollectionAssert.AreEquivalent(new[] { 2 }.ToList(), outputList);
            #endregion
        }




        [TestCategory(nameof(EnsureSortedTests))]
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void FailEnsureSortedNotSorted()
        {
            #region ensure sorted not sorted
            var inputList = new[] { 2, 1, 3, 4, 5 }.ToList();
            var outputList = new List<int>();

            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .EnsureSorted("ensure sorted", i => i)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();
            #endregion
        }
    }
}
