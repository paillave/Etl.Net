using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class SkipSubjectTests
    {
        [TestCategory(nameof(SkipSubjectTests))]
        [TestMethod]
        public void SkipMultipleValue()
        {
            RangeOfValues(-5, 10, 3);
        }

        [TestCategory(nameof(SkipSubjectTests))]
        [TestMethod]
        public void SkipNoValues()
        {
            RangeOfValues(-5, 10, 0);
        }

        public void RangeOfValues(int start, int nb, int skip)
        {
            var outputValues = new List<int>();
            var obs = PushObservable.Range(start, nb, true).Skip(skip);

            obs.Subscribe(outputValues.Add);

            var task = obs.ToTaskAsync();

            task.Wait(5000);

            for (int i = 0; i < (nb - skip); i++)
                Assert.AreEqual(start + i + skip, outputValues[i], "all values should be the same");
            Assert.AreEqual(nb - skip, outputValues.Count, $"nb items from the output must be the same that in the input except the {skip}th ones");
        }
    }
}
