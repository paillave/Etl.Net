using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class SkipUntilConditionSubjectTests
    {
        [TestCategory(nameof(SkipUntilConditionSubjectTests))]
        [TestMethod]
        public void SimplePushBeforeTrigger()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.SkipUntil(i => i == 0);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(0, errorStack.Count, "as the stream is not triggered, no error should be issued");

            obs1.PushValue(0);
            Assert.AreEqual(0, valueStack.Peek(), "the ouput stream should issue values as long as the trigger stream issues a value");

            obs1.PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "the ouput stream should issue values as long as the trigger stream issues a value");

            ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(1, errorStack.Count, "as the stream is not triggered, no error should be issued");
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "as the stream is triggered, the error should be issued");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be competed");
        }

        [TestCategory(nameof(SkipUntilConditionSubjectTests))]
        [TestMethod]
        public void SimplePushBeforeTriggerExclude()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.SkipUntil(i => i == 0, false);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(0, errorStack.Count, "as the stream is not triggered, no error should be issued");

            obs1.PushValue(0);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            obs1.PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "the ouput stream should issue values as long as the trigger stream issues a value");

            ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(1, errorStack.Count, "as the stream is not triggered, no error should be issued");
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "as the stream is triggered, the error should be issued");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be competed");
        }

        [TestCategory(nameof(SkipUntilConditionSubjectTests))]
        [TestMethod]
        public void TriggerAfterMainStreamComplete()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.SkipUntil(i => i == 0);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "the output stream should not issue any value as long as it's not triggered");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be competed");

            obs1.PushValue(0);

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(0, errorStack.Count, "as the stream is completed, no error should be issued");
        }
    }
}
