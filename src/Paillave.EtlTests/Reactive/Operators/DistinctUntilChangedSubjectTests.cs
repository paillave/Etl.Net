using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class DistinctUntilChangedSubjectTests
    {
        [TestCategory(nameof(DistinctUntilChangedSubjectTests))]
        [TestMethod]
        public void SimpleDistinct()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;

            var obs = new PushSubject<int>();

            var output = obs.DistinctUntilChanged();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "the input value should be issued");

            obs.PushValue(2);
            Assert.AreEqual(2, valueStack.Peek(), "the input value should be issued");

            obs.PushValue(1);
            Assert.AreEqual(3, valueStack.Count, "the input value can be issued as long as the previous value is different");
            Assert.AreEqual(1, valueStack.Peek(), "the input value should be issued");

            obs.PushValue(1);
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued issued again as it is the same");

            obs.PushValue(2);
            Assert.AreEqual(4, valueStack.Count, "the input value can to be issued again as it is not the same than the previous one");
            Assert.AreEqual(2, valueStack.Peek(), "the input value should be issued");

            obs.Complete();
            Assert.IsTrue(isComplete, "the stream should be completed");
        }

        [TestCategory(nameof(DistinctUntilChangedSubjectTests))]
        [TestMethod]
        public void ComplexDistinct()
        {
            var valueStack = new Stack<Tuple<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;

            var obs = new PushSubject<Tuple<int>>();

            var output = obs.DistinctUntilChanged((l, r) => l.Item1 == r.Item1);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(1, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(2));
            Assert.AreEqual(2, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(3, valueStack.Count, "the input value can be issued as long as the previous value is different");
            Assert.AreEqual(1, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued issued again as it is the same");

            obs.PushValue(new Tuple<int>(2));
            Assert.AreEqual(4, valueStack.Count, "the input value can to be issued again as it is not the same than the previous one");
            Assert.AreEqual(2, valueStack.Peek().Item1, "the input value should be issued");

            obs.Complete();
            Assert.IsTrue(isComplete, "the stream should be completed");
        }
    }
}
