using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass()]
    public class SwitchMapSubjectTests
    {
        [TestCategory(nameof(SwitchMapSubjectTests))]
        [TestMethod]
        public void SwitchFlatMap()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs = new PushSubject<int>();
            var sobs = new[] { new PushSubject<int>(), new PushSubject<int>() };

            var output = obs.SwitchMap(i => sobs[i]);

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
            Assert.AreNotEqual(3, valueStack.Peek(), "values from the first input streams should not be in the output stream");

            sobs[1].PushValue(4);
            Assert.AreEqual(4, valueStack.Peek(), "values from the input streams should be in the output stream");

            sobs[0].Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed");

            sobs[1].Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed");

            obs.Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed");
        }

        [TestCategory(nameof(SwitchMapSubjectTests))]
        [TestMethod]
        public void PushValuesAfterComplete()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs = new PushSubject<int>();
            var sobs = new[] { new PushSubject<int>(), new PushSubject<int>() };

            var output = obs.SwitchMap(i => sobs[i]);

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
            Assert.AreNotEqual(3, valueStack.Peek(), "values from the first input stream should not be in the output stream anymore");

            sobs[1].PushValue(4);
            Assert.AreEqual(4, valueStack.Peek(), "values from the input streams should be in the output stream");

            obs.Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed unless its current sub stream is completed");

            sobs[0].Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed unless its current sub stream is completed");

            sobs[1].PushValue(6);
            Assert.AreEqual(6, valueStack.Peek(), "values from the input streams should not be in the output stream");

            sobs[1].Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed as its current sub stream is completed");
        }
    }
}
