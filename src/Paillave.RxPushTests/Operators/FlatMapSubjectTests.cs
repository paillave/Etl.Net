using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPushTests.Operators
{
    [TestClass()]
    public class FlatMapSubjectTests
    {
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
    }
}
