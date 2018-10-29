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
    public class EnsureKeyedTests
    {
        [TestCategory(nameof(EnsureKeyedTests))]
        [TestMethod]
        public void SuccessfulEnsureKeyed()
        {
            #region ensure keyed without pb
            var inputList = new[] { 1, 2, 3, 4, 5 }.ToList();
            var outputList = new List<int>();

            StreamProcessRunner.Create<object>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", _ => inputList)
                    .EnsureKeyed("ensure keyed", i => i)
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

            CollectionAssert.AreEquivalent(inputList, outputList);
            #endregion
        }
        [TestCategory(nameof(EnsureKeyedTests))]
        [TestMethod]
        public void FailEnsureKeyedWithDuplicate()
        {
            #region ensure keyed with duplicate
            var inputList = new[] { 1, 2, 2, 3, 4, 5 }.ToList();
            var outputList = new List<int>();

            var task = StreamProcessRunner.Create<object>(rootStream =>
             {
                 rootStream
                     .CrossApplyEnumerable("list elements", _ => inputList)
                     .EnsureKeyed("ensure keyed", i => i)
                     .ThroughAction("collect values", outputList.Add);
             }).ExecuteAsync(null);
            task.Wait();

            Assert.IsNotNull(task.Result.StreamStatisticErrors.FirstOrDefault(i => i.NodeName == "ensure keyed"));
            CollectionAssert.AreEquivalent(new[] { 1, 2 }.ToList(), outputList);
            #endregion
        }
        [TestCategory(nameof(EnsureKeyedTests))]
        [TestMethod]
        public void FailEnsureKeyedNotSorted()
        {
            #region ensure keyed not sorted
            var inputList = new[] { 2, 1, 3, 4, 5 }.ToList();
            var outputList = new List<int>();

            var task = StreamProcessRunner.Create<object>(rootStream =>
             {
                 rootStream
                     .CrossApplyEnumerable("list elements", _ => inputList)
                     .EnsureKeyed("ensure keyed", i => i)
                     .ThroughAction("collect values", outputList.Add);
             }).ExecuteAsync(null);
            task.Wait();

            Assert.IsNotNull(task.Result.StreamStatisticErrors.FirstOrDefault(i => i.NodeName == "ensure keyed"));
            CollectionAssert.AreEquivalent(new[] { 2 }.ToList(), outputList);
            #endregion
        }
    }
}
