using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
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

        public void RangeOfValues(int start, int nb, bool startOnFirstSubscription)
        {
            var expectedValues = Enumerable.Range(start, nb).ToList();
            var outputValues = new List<int>();
            var obs = PushObservable.Range(-5, nb, startOnFirstSubscription);

            obs.Subscribe(outputValues.Add);

            var task = obs.ToTaskAsync();

            if (!startOnFirstSubscription) obs.Start();

            Assert.IsTrue(task.Wait(5000), "the stream should complete");

            for (int i = 0; i < nb; i++)
                Assert.AreEqual(start + i, outputValues[i], "all values should be the same");
            Assert.AreEqual(nb, outputValues.Count, "nb items from the input source must be the same that in the output");
        }
    }
}
