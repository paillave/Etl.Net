using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class SkipUntilSubjectTests
    {
        [TestCategory(nameof(SkipUntilSubjectTests))]
        [TestMethod]
        public void SimplePushBeforeTrigger()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();
            var obs2 = new PushSubject<int>();

            var output = obs1.SkipUntil(obs2);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(0, errorStack.Count, "as the stream is not triggered, no error should be issued");

            obs2.PushValue(2);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value only by being triggered");

            obs1.PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "the ouput stream should issue values as long as the trigger stream issues a value");

            ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(1, errorStack.Count, "as the stream is not triggered, no error should be issued");
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "as the stream is triggered, the error should be issued");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be competed");
        }

        [TestCategory(nameof(SkipUntilSubjectTests))]
        [TestMethod]
        public void TriggerAfterMainStreamComplete()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();
            var obs2 = new PushSubject<int>();

            var output = obs1.SkipUntil(obs2);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be competed");

            obs2.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(0, errorStack.Count, "as the stream is completed, no error should be issued");
        }

        [TestCategory(nameof(SkipUntilSubjectTests))]
        [TestMethod]
        public void TriggerStreamCompletesBeforeMainStreamIsTriggered()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();
            var obs2 = new PushSubject<int>();

            var output = obs1.SkipUntil(obs2);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            obs2.Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed as it cannot be triggered anymore");
        }

        [TestCategory(nameof(SkipUntilSubjectTests))]
        [TestMethod]
        public void TriggerStreamCompletesAfterTriggering()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();
            var obs2 = new PushSubject<int>();

            var output = obs1.SkipUntil(obs2);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            obs2.PushValue(2);

            obs2.Complete();
            Assert.IsFalse(isComplete, "the output stream should not be completed");

            obs1.PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "the output stream should issue a value even if the trigger is completed");
        }
    }
}
