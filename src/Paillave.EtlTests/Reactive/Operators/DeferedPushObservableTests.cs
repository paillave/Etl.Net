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
        public void Push10000Values()
        {
            PushValuesNotAutomaticStart(10000);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void Push100Values()
        {
            PushValuesNotAutomaticStart(100);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void Push10Values()
        {
            PushValuesNotAutomaticStart(10);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void Push0Value()
        {
            PushValuesNotAutomaticStart(0);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void Push10000ValuesStartWithEvent()
        {
            PushValuesStartWithEvent(10000);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void Push100ValuesStartWithEvent()
        {
            PushValuesStartWithEvent(100);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void Push10ValuesStartWithEvent()
        {
            PushValuesStartWithEvent(10);
        }

        [TestCategory(nameof(DeferedPushObservableTests))]
        [TestMethod]
        public void Push0ValueStartWithEvent()
        {
            PushValuesStartWithEvent(0);
        }

        public void PushValuesStartWithEvent(int nb)
        {
            var inputValues = Enumerable.Range(0, nb).ToList();
            var outputValues = new List<int>();
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = PushObservable.FromEnumerable(inputValues, waitHandle);
            var task = observable.ToListAsync();
            observable.Subscribe(outputValues.Add);
            waitHandle.Set();
            Assert.IsTrue(task.Wait(5000), "the stream should complete");
            CollectionAssert.AreEquivalent(inputValues, outputValues);
            CollectionAssert.AreEquivalent(inputValues, task.Result);
        }

        public void PushValuesNotAutomaticStart(int nb)
        {
            var inputValues = Enumerable.Range(0, nb).ToList();
            var outputValues = new List<int>();
            var observable = PushObservable.FromEnumerable(inputValues);
            var task = observable.ToListAsync();
            observable.Subscribe(outputValues.Add);
            observable.Start();
            Assert.IsTrue(task.Wait(5000), "the stream should complete");
            CollectionAssert.AreEquivalent(inputValues, outputValues);
            CollectionAssert.AreEquivalent(inputValues, task.Result);
        }
    }
}
