using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class TakeUntilConditionSubjectTests
    {
        [TestCategory(nameof(TakeUntilConditionSubjectTests))]
        [TestMethod]
        public void SimplePushBeforeTriggerExcluded()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.TakeUntil(i => i == 0);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "the input value should be issued");

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "the exception should be issued");

            obs1.PushValue(0);
            Assert.AreEqual(1, valueStack.Count, "the ouput stream should not issue any other value");

            obs1.PushValue(3);
            Assert.AreEqual(1, valueStack.Count, "the ouput stream should not issue any other value");

            ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(1, errorStack.Count, "the exception should not be issued");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be competed");
        }
        [TestCategory(nameof(TakeUntilConditionSubjectTests))]
        [TestMethod]
        public void SimplePushBeforeTriggerIncluded()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.TakeUntil(i => i == 0,true);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "the input value should be issued");

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "the exception should be issued");

            obs1.PushValue(0);
            Assert.AreEqual(0, valueStack.Peek(), "the input value should be issued");

            obs1.PushValue(3);
            Assert.AreEqual(2, valueStack.Count, "the ouput stream should not issue any other value");

            ex = new Exception();
            obs1.PushException(ex);
            Assert.AreEqual(1, errorStack.Count, "the exception should not be issued");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be competed");
        }
    }
}
