using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class ExceptionsToObservableSubjectTests
    {
        [TestCategory(nameof(ExceptionsToObservableSubjectTests))]
        [TestMethod]
        public void SimpleErrors()
        {
            var valueStack = new Stack<Exception>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.ExceptionsToObservable();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.IsTrue(Object.ReferenceEquals(ex, valueStack.Peek()), "exception message should be in the output stream");
            Assert.AreEqual(0, errorStack.Count, "no exception should be in the error output stream");

            ex = new Exception();
            obs1.PushException(ex);
            Assert.IsTrue(Object.ReferenceEquals(ex, valueStack.Peek()), "exception message should be in the output stream");
            Assert.AreEqual(0, errorStack.Count, "no exception should be in the error output stream");

            obs1.Complete();
            Assert.AreEqual(2, valueStack.Count, "no more exception should be in the error output stream");
            Assert.IsTrue(isComplete, "the output stream should be complete");
        }
    }
}
