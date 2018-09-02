using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class CombineWithLatestSubjectTests
    {
        [TestCategory(nameof(CombineWithLatestSubjectTests))]
        [TestMethod]
        public void SimplePushValues()
        {
            var valueStack = new Stack<Tuple<int, int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();
            var obs2 = new PushSubject<int>();

            var output = obs1.CombineWithLatest<int, int, Tuple<int, int>>(obs2, (v1, v2) => new Tuple<int, int>(v1, v2));

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "no value should be in the output stream if not both value are sent into input streams");

            obs1.PushValue(2);
            Assert.AreEqual(0, valueStack.Count, "no value should be in the output stream if not both value are sent into input streams");

            obs2.PushValue(3);
            var outValue = valueStack.Peek();
            Assert.IsTrue(outValue.Item1 == 2 && outValue.Item2 == 3, "the ouput value should contains the 2 last submitted values in both streams");

            obs2.PushValue(4);
            outValue = valueStack.Peek();
            Assert.IsTrue(outValue.Item1 == 2 && outValue.Item2 == 4, "the ouput value should contains the 2 last submitted values in both streams");

            obs1.PushValue(5);
            outValue = valueStack.Peek();
            Assert.IsTrue(outValue.Item1 == 5 && outValue.Item2 == 4, "the ouput value should contains the 2 last submitted values in both streams");

            obs2.Complete();
            Assert.IsFalse(isComplete, "the output stream shouldn't complete as long as both input streams are not complete");

            obs1.PushValue(6);
            outValue = valueStack.Peek();
            Assert.IsTrue(outValue.Item1 == 6 && outValue.Item2 == 4, "the ouput value should contains the 2 last submitted values in both streams");

            obs1.Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed");
        }
    }
}
