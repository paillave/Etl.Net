using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class DoSubjectTests
    {
        [TestCategory(nameof(DoSubjectTests))]
        [TestMethod]
        public void DoWithoutError()
        {
            var valueStack = new Stack<int>();
            var doStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.Do(doStack.Push);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "value should be submitted");
            Assert.AreEqual(1, doStack.Peek(), "value should be submitted");
            Assert.IsFalse(isComplete, "the stream should not be completed");

            obs1.PushValue(2);
            Assert.AreEqual(2, valueStack.Peek(), "value should be submitted");
            Assert.AreEqual(2, doStack.Peek(), "value should be submitted");
            Assert.IsFalse(isComplete, "the stream should not be completed");

            Assert.AreEqual(2, valueStack.Count, "no value should be submitted");
            Assert.AreEqual(2, doStack.Count, "no value should be submitted");
        }

        [TestCategory(nameof(DoSubjectTests))]
        [TestMethod]
        public void DoWithError()
        {
            var valueStack = new Stack<int>();
            var doStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.Do(i => doStack.Push(1 / i));

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(0);
            Assert.AreEqual(0, valueStack.Peek(), "value should be submitted");
            Assert.AreEqual(0, doStack.Count, "no value should be sent");
            Assert.IsFalse(isComplete, "the stream should not be completed");

            obs1.PushValue(2);
            Assert.AreEqual(2, valueStack.Peek(), "value should be submitted");
            Assert.AreEqual(1/2, doStack.Peek(), "value should be submitted");
            Assert.IsFalse(isComplete, "the stream should not be completed");

            Assert.AreEqual(2, valueStack.Count, "no value should be submitted");
            Assert.AreEqual(1, doStack.Count, "no value should be submitted");
        }
    }
}
