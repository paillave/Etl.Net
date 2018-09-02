using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class FirstSubjectTests
    {
        [TestCategory(nameof(FirstSubjectTests))]
        [TestMethod]
        public void GetSimpleFirst()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.First();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "value should be submitted");
            Assert.IsTrue(isComplete, "the stream should be completed");

            obs1.PushValue(2);
            Assert.AreEqual(1, valueStack.Count, "no value should be submitted");
        }

        [TestCategory(nameof(FirstSubjectTests))]
        [TestMethod]
        public void GetFirstForEmptyStream()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.First();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.Complete();
            Assert.AreEqual(0, valueStack.Count, "no value should be submitted");
            Assert.IsTrue(isComplete, "the stream should be completed");
        }
    }
}
