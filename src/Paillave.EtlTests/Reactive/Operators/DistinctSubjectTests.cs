using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class DistinctSubjectTests
    {
        [TestCategory(nameof(DistinctSubjectTests))]
        [TestMethod]
        public void SimpleDistinct()
        {
            var valueStack = new Stack<int>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;

            var obs = new PushSubject<int>();

            var output = obs.Distinct();

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(1);
            Assert.AreEqual(1, valueStack.Peek(), "the input value should be issued");

            obs.PushValue(2);
            Assert.AreEqual(2, valueStack.Peek(), "the input value should be issued");

            obs.PushValue(1);
            Assert.AreEqual(2, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.PushValue(3);
            Assert.AreEqual(3, valueStack.Peek(), "the input value should be issued");

            obs.PushValue(1);
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.PushValue(2);
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.Complete();
            Assert.IsTrue(isComplete, "the stream should be completed");
        }

        [TestCategory(nameof(DistinctSubjectTests))]
        [TestMethod]
        public void ComplexDistinct()
        {
            var valueStack = new Stack<Tuple<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;

            var obs = new PushSubject<Tuple<int>>();

            var output = obs.Distinct((l, r) => l.Item1 == r.Item1);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(1, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(2));
            Assert.AreEqual(2, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(2, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.PushValue(new Tuple<int>(3));
            Assert.AreEqual(3, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.PushValue(new Tuple<int>(2));
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.Complete();
            Assert.IsTrue(isComplete, "the stream should be completed");
        }
        [TestCategory(nameof(DistinctSubjectTests))]
        [TestMethod]
        public void ComplexDistinct1()
        {
            var valueStack = new Stack<Tuple<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;

            var obs = new PushSubject<Tuple<int>>();

            var output = obs.Distinct(l => l.Item1);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(1, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(2));
            Assert.AreEqual(2, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(2, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.PushValue(new Tuple<int>(3));
            Assert.AreEqual(3, valueStack.Peek().Item1, "the input value should be issued");

            obs.PushValue(new Tuple<int>(1));
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.PushValue(new Tuple<int>(2));
            Assert.AreEqual(3, valueStack.Count, "the input value should not be issued as it has been submitted first");

            obs.Complete();
            Assert.IsTrue(isComplete, "the stream should be completed");
        }
    }
}
