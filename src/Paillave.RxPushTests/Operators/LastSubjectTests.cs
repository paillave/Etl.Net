using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class LastSubjectTests
    {
        [TestCategory(nameof(LastSubjectTests))]
        [TestMethod]
        public void GetSimpleLast()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.Last();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "no value should be submitted");

            obs1.PushValue(2);
            Assert.AreEqual(0, valueStack.Count, "no value should be submitted");

            obs1.Complete();
            Assert.AreEqual(1, valueStack.Count, "one value should be submitted");
            Assert.IsTrue(isComplete, "the stream should be completed");
        }

        [TestCategory(nameof(LastSubjectTests))]
        [TestMethod]
        public void GetLastForEmptyStream()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.Last();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.Complete();
            Assert.AreEqual(0, valueStack.Count, "no value should be submitted");
            Assert.IsTrue(isComplete, "the stream should be completed");
        }
    }
}
