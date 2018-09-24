using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Operators;
using System.Threading;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class RangeSubjectTests
    {
        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void MultipleValues()
        {
            RangeOfValuesNotAutomaticallyStartsOnHandle(-5, 10);
        }

        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void NoValue()
        {
            RangeOfValuesNotAutomaticallyStartsOnHandle(0, 0);
        }

        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void MultipleValuesStartOnFirstSubscription()
        {
            RangeOfValuesAutomaticallyStartsOnHandle(-5, 10);
        }

        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void NoValueStartOnFirstSubscription()
        {
            RangeOfValuesAutomaticallyStartsOnHandle(0, 0);
        }

        public void RangeOfValuesAutomaticallyStartsOnHandle(int start, int nb)
        {
            var expectedValues = Enumerable.Range(start, nb).ToList();
            var outputValues = new List<int>();
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var obs = PushObservable.Range(-5, nb, waitHandle);

            obs.Subscribe(outputValues.Add);

            var task = obs.ToTaskAsync();

            waitHandle.Set();

            Assert.IsTrue(task.Wait(5000), "the stream should complete");

            for (int i = 0; i < nb; i++)
                Assert.AreEqual(start + i, outputValues[i], "all values should be the same");
            Assert.AreEqual(nb, outputValues.Count, "nb items from the input source must be the same that in the output");
        }
        public void RangeOfValuesNotAutomaticallyStartsOnHandle(int start, int nb)
        {
            var expectedValues = Enumerable.Range(start, nb).ToList();
            var outputValues = new List<int>();
            var obs = PushObservable.Range(-5, nb);

            obs.Subscribe(outputValues.Add);

            var task = obs.ToTaskAsync();

            obs.Start();
            Assert.IsTrue(task.Wait(5000), "the stream should complete");

            for (int i = 0; i < nb; i++)
                Assert.AreEqual(start + i, outputValues[i], "all values should be the same");
            Assert.AreEqual(nb, outputValues.Count, "nb items from the input source must be the same that in the output");
        }
    }
}
