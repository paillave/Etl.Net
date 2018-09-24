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
    public class DeferedPushObservableTests
    {
        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushMultipleValues()
        {
            PushValuesNotAutomaticStart(10);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushNoValue()
        {
            PushValuesNotAutomaticStart(0);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushMultipleValuesStartOnFirstSubscription()
        {
            PushValuesAutomaticStart(10);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void PushNoValueStartOnFirstSubscription()
        {
            PushValuesAutomaticStart(0);
        }

        public void PushValuesAutomaticStart(int nb)
        {
            var inputValues = Enumerable.Range(0, nb).ToList();
            var outputValues = new List<int>();
            bool completed = false;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = new EventDeferedPushObservable<int>((pushValue) =>
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

            waitHandle.Set();
            Assert.IsTrue(task.Wait(5000), "the stream should complete");
            for (int i = 0; i < nb; i++)
                Assert.AreEqual(i, outputValues[i], "all values should be the same");
            Assert.AreEqual(inputValues.Count, outputValues.Count, "nb items from the input source must be the same that in the output");
            Assert.IsFalse(completed, "shouldn't be completed");
        }

        public void PushValuesNotAutomaticStart(int nb)
        {
            var inputValues = Enumerable.Range(0, nb).ToList();
            var outputValues = new List<int>();
            bool completed = false;
            var observable = new DeferedPushObservable<int>((pushValue) =>
              {
                  foreach (var item in inputValues)
                      pushValue(item);
              });

            var task = observable.ToTaskAsync();

            System.Threading.Thread.Sleep(99); //not more than 100!!!

            //for (int i = 0; i < 5000; i++)
            //{

            //};
            observable.Subscribe(outputValues.Add);

            observable.Start();
            Assert.IsTrue(task.Wait(5000), "the stream should complete");
            for (int i = 0; i < nb; i++)
                Assert.AreEqual(i, outputValues[i], "all values should be the same");
            Assert.AreEqual(inputValues.Count, outputValues.Count, "nb items from the input source must be the same that in the output");
            Assert.IsFalse(completed, "shouldn't be completed");
        }
    }
}
