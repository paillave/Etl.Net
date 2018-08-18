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
    public class DeferedPushObservableTests
    {
        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushMultipleValues()
        {
            PushValues(10, false);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushNoValue()
        {
            PushValues(0, false);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushMultipleValuesStartOnFirstSubscription()
        {
            PushValues(10, true);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushNoValueStartOnFirstSubscription()
        {
            PushValues(0, true);
        }

        public void PushValues(int nb, bool automaticallyStartsOnHandle)
        {
            var inputValues = Enumerable.Range(0, nb).ToList();
            var outputValues = new List<int>();
            bool completed = false;
            EventWaitHandle waitHandle = null;
            if (automaticallyStartsOnHandle)
                waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = new DeferedPushObservable<int>((pushValue) =>
              {
                  foreach (var item in inputValues)
                      pushValue(item);
              }, waitHandle);

            var task = observable.ToTaskAsync();

            System.Threading.Thread.Sleep(99); //not more than 100!!!

            //for (int i = 0; i < 5000; i++)
            //{

            //};
            observable.Subscribe(outputValues.Add);

            if (!automaticallyStartsOnHandle)
                observable.Start();
            else
                waitHandle.Set();
            Assert.IsTrue(task.Wait(5000), "the stream should complete");
            for (int i = 0; i < nb; i++)
                Assert.AreEqual(i, outputValues[i], "all values should be the same");
            Assert.AreEqual(inputValues.Count, outputValues.Count, "nb items from the input source must be the same that in the output");
            Assert.IsFalse(completed, "shouldn't be completed");
        }
    }
}
