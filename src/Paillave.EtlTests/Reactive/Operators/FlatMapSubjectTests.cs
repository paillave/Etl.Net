using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass()]
    public class FlatMapSubjectTests
    {
        [TestCategory(nameof(FlatMapSubjectTests))]
        [TestMethod]
        public void FlatMapWithDeferable()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs = new PushSubject<int>();
            var sobs = new[] { PushObservable.FromEnumerable(new[] { 1, 2 }), PushObservable.FromEnumerable(new[] { 3, 4 }) };

            var output = obs.FlatMap(i => sobs[i]);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(0);
            System.Threading.Thread.Sleep(50); //the time for the 2 values to be issued
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, valueStack, "output values should be automatically issued");

            obs.PushValue(1);
            System.Threading.Thread.Sleep(50); //the time for the 2 values to be issued
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, valueStack, "output values should be automatically issued");

            obs.Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed");
        }

        [TestCategory(nameof(FlatMapSubjectTests))]
        [TestMethod]
        public void SimpleFlatMap()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs = new PushSubject<int>();
            var sobs = new[] { new PushSubject<int>(), new PushSubject<int>() };

            var output = obs.FlatMap(i => sobs[i]);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(0);
            Assert.AreEqual(0, valueStack.Count, "no value should be in the output stream");

            sobs[0].PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[1].PushValue(2);
            Assert.AreEqual(1, valueStack.Count, "values from the input streams that are not used should not be in the output stream");

            var ex = new Exception();
            sobs[0].PushException(ex);
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "errors from the input streams should be in the output stream");

            obs.PushValue(1);
            Assert.AreEqual(1, valueStack.Count, "no more values from the input streams should be in the output stream");

            sobs[0].PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[1].PushValue(4);
            Assert.AreEqual(4, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[0].Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed");

            sobs[1].Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed");

            obs.Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed");
        }

        [TestCategory(nameof(FlatMapSubjectTests))]
        [TestMethod]
        public void PushValuesAfterComplete()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs = new PushSubject<int>();
            var sobs = new[] { new PushSubject<int>(), new PushSubject<int>() };

            var output = obs.FlatMap(i => sobs[i]);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(0);
            Assert.AreEqual(0, valueStack.Count, "no value should be in the output stream");

            sobs[0].PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[1].PushValue(2);
            Assert.AreEqual(1, valueStack.Count, "values from the input streams that are not used should not be in the output stream");

            var ex = new Exception();
            sobs[0].PushException(ex);
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "errors from the input streams should be in the output stream");

            obs.PushValue(1);
            Assert.AreEqual(1, valueStack.Count, "no more values from the input streams should be in the output stream");

            sobs[0].PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[1].PushValue(4);
            Assert.AreEqual(4, valueStack.Peek(), "values from the input streams should be in the output stream");

            obs.Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed unless its sub stream are completed");

            sobs[0].PushValue(5);
            Assert.AreEqual(5, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[0].Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed unless its sub stream are completed");

            sobs[1].PushValue(6);
            Assert.AreEqual(6, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[1].Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed if the source is complete and its sub stream are completed");
        }

        [TestCategory(nameof(FlatMapSubjectTests))]
        [TestMethod]
        public void Test3()
        {
            for (int counter = 0; counter < 5000; counter++)
            {
                Stopwatch stopwatch = new Stopwatch();
                Debug.WriteLine($"START {counter}: {DateTime.Now}");
                stopwatch.Start();
                var initObs = PushObservable.FromSingle("aze");
                var task = initObs.FlatMap(i => new DeferredPushObservable<string>(push => push(i))).ToTaskAsync();
                initObs.Start();
                task.Wait(5000);
                Assert.IsTrue(task.IsCompletedSuccessfully);
                stopwatch.Stop();
                Debug.WriteLine($"STOP {counter}: {stopwatch.Elapsed}");
            }
        }
    }
}
