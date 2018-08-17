using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;
using System.Threading;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class TakeSubjectTests
    {
        [TestCategory(nameof(TakeSubjectTests))]
        [TestMethod]
        public void TopMultipleValue()
        {
            RangeOfValues(-5, 10, 3);
        }

        [TestCategory(nameof(TakeSubjectTests))]
        [TestMethod]
        public void TopNoValues()
        {
            RangeOfValues(-5, 10, 0);
        }

        [TestCategory(nameof(TakeSubjectTests))]
        [TestMethod]
        public void CompletesFromTopReached()
        {
            var sub = new PushSubject<int>();
            bool complete = false;
            sub.Take(1).Subscribe(_ => { }, () => complete = true);
            sub.PushValue(1);
            Assert.IsTrue(complete, "stream must be completed from the moment the top is reached");
        }

        public void RangeOfValues(int start, int nb, int top)
        {
            var outputValues = new List<int>();
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var obs = PushObservable.Range(start, nb, waitHandle).Take(top);

            obs.Subscribe(outputValues.Add);

            var task = obs.ToTaskAsync();
            waitHandle.Set();

            task.Wait(5000);

            for (int i = 0; i < top; i++)
                Assert.AreEqual(start + i, outputValues[i], "all values should be the same");
            Assert.AreEqual(top, outputValues.Count, $"nb items from the output must be {top}");
        }
    }
}
