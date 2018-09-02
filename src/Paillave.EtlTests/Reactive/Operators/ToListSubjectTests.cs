using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class ToListSubjectTests
    {
        [TestCategory(nameof(ToListSubjectTests))]
        [TestMethod]
        public void GetSimpleList()
        {
            var valueStack = new Stack<List<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.ToList();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count, "no value should be submitted");

            obs1.PushValue(2);
            Assert.AreEqual(0, valueStack.Count, "no value should be submitted");

            obs1.Complete();
            Assert.AreEqual(1, valueStack.Count, "one value should be submitted");
            var lst = valueStack.Peek();
            Assert.AreEqual(1, lst[0], "values should be accumulated");
            Assert.AreEqual(2, lst[1], "values should be accumulated");
            Assert.IsTrue(lst.Count == 2, "all values must be exactly in the list");
            Assert.IsTrue(isComplete, "the stream should be completed");
        }

        [TestCategory(nameof(ToListSubjectTests))]
        [TestMethod]
        public void GetListForEmptyStream()
        {
            var valueStack = new Stack<List<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.ToList();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.Complete();
            Assert.AreEqual(1, valueStack.Count, "no value should be submitted");
            var lst = valueStack.Peek();
            Assert.IsTrue(lst.Count == 0, "all values must be exactly in the list");
            Assert.IsTrue(isComplete, "the stream should be completed");
        }
    }
}
