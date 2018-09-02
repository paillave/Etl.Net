using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class ToTaskAsyncTests
    {
        [TestCategory(nameof(ToTaskAsyncTests))]
        [TestMethod]
        public void WaitEnd()
        {
            bool completed = false;
            var tmp = new PushSubject<int>();
            tmp.Subscribe((_) => { }, () => completed = true);
            var returnedTask = tmp.ToTaskAsync();
            Assert.IsFalse(returnedTask.IsCompleted, "The task shouldn't be completed");
            tmp.Complete();
            Assert.IsTrue(returnedTask.Wait(5000), "The task should complete");
            Assert.IsTrue(completed, "complete should be triggered");
        }
        [TestCategory(nameof(ToTaskAsyncTests))]
        [TestMethod]
        public void WaitEndWitoutComplete()
        {
            bool completed = false;
            var tmp = new PushSubject<int>();
            tmp.Subscribe((_) => { }, () => completed = true);
            var returnedTask = tmp.ToTaskAsync();
            Assert.IsFalse(returnedTask.IsCompleted, "The task shouldn't be completed");
            Assert.IsFalse(returnedTask.Wait(1000), "The task should not complete");
            Assert.IsFalse(completed, "complete should not be triggered");
        }
        [TestCategory(nameof(ToTaskAsyncTests))]
        [TestMethod]
        public void WaitEndOfList()
        {
            var inputValues = Enumerable.Range(0, 1000000).ToList();
            var outputValues = new List<int>();
            var tmp = PushObservable.FromEnumerable(inputValues);
            var obs = tmp.Filter(i => i >= 0); ;

            var output = obs.ToListAsync();

            tmp.Start();
            output.Wait();

            CollectionAssert.AreEquivalent(inputValues, output.Result, "the output should contains only sorted values");
        }
    }
}
