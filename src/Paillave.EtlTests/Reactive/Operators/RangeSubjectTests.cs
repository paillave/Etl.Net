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
            RangeOfValues(-5, 10, false);
        }

        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void NoValue()
        {
            RangeOfValues(0, 0, false);
        }

        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void MultipleValuesStartOnFirstSubscription()
        {
            RangeOfValues(-5, 10, true);
        }

        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void NoValueStartOnFirstSubscription()
        {
            RangeOfValues(0, 0, true);
        }

        public void RangeOfValues(int start, int nb, bool automaticallyStartsOnHandle)
        {
            var expectedValues = Enumerable.Range(start, nb).ToList();
            var outputValues = new List<int>();
            EventWaitHandle waitHandle = null;
            if (automaticallyStartsOnHandle)
                waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var obs = PushObservable.Range(-5, nb, waitHandle);

            obs.Subscribe(outputValues.Add);

            var task = obs.ToTaskAsync();

            if (!automaticallyStartsOnHandle)
                obs.Start();
            else
                waitHandle.Set();

            Assert.IsTrue(task.Wait(5000), "the stream should complete");

            for (int i = 0; i < nb; i++)
                Assert.AreEqual(start + i, outputValues[i], "all values should be the same");
            Assert.AreEqual(nb, outputValues.Count, "nb items from the input source must be the same that in the output");
        }
    }
}
