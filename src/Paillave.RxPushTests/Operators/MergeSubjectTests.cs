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
    public class MergeSubjectTests
    {
        [TestCategory(nameof(MergeSubjectTests))]
        [TestMethod]
        public void SimpleMerge()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();
            var obs2 = new PushSubject<int>();

            var output = obs1.Merge(obs2);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "input from one stream should be found on the merged stream");

            obs2.PushValue(2);
            Assert.AreEqual(2, valueStack.Peek(), "input from one stream should be found on the merged stream");

            obs2.PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "input from one stream should be found on the merged stream");

            obs1.PushValue(4);
            Assert.AreEqual(4, valueStack.Peek(), "input from one stream should be found on the merged stream");

            var ex1 = new Exception();
            obs1.PushException(ex1);
            Assert.IsTrue(Object.ReferenceEquals(ex1, errorStack.Peek()), "input exception from one stream should be found on the merged stream");

            var ex2 = new Exception();
            obs2.PushException(ex2);
            Assert.IsTrue(Object.ReferenceEquals(ex2, errorStack.Peek()), "input exception from one stream should be found on the merged stream");

            Assert.IsFalse(isComplete, "the stream should not be finished if not every input stream is complete");

            obs1.Complete();
            Assert.IsFalse(isComplete, "the stream should not be finished if not every input stream is complete");

            obs2.Complete();
            Assert.IsTrue(isComplete, "the stream should be finished if every input stream is complete");
        }
    }
}
