using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class FilterSubjectTests
    {
        [TestCategory(nameof(FilterSubjectTests))]
        [TestMethod]
        public void FilterValues()
        {
            var inputValues = new[] { -2, -1, 0, 1, 2 };
            var outputValues = new List<int>();
            var obs = new PushSubject<int>();
            var filtered = obs.Filter(i => i > 0);
            filtered.Subscribe(outputValues.Add);

            foreach (var item in inputValues)
                obs.PushValue(item);

            obs.Complete();

            Assert.IsTrue(filtered.ToTaskAsync().Wait(5000), "The filtering should complete");

            var expected = inputValues.Where(i => i > 0).ToList();

            for (int i = 0; i < outputValues.Count; i++)
                Assert.AreEqual(expected[i], outputValues[i], "all values should match");
            Assert.AreEqual(expected.Count, outputValues.Count, $"nb items from the output must match the input one");
        }

        [TestCategory(nameof(FilterSubjectTests))]
        [TestMethod]
        public void Synchronisation()
        {
            var outputValues = new Stack<int>();
            var errors = new Stack<Exception>();
            var completed = false;
            var obs = new PushSubject<int>();

            var mapped = obs.Filter(i => i > 0);
            mapped.Subscribe(outputValues.Push, () => completed = true, errors.Push);

            obs.PushValue(0);
            Assert.AreEqual(0, outputValues.Count, "no output should be issued");
            Assert.IsFalse(completed, "the stream should not be completed");

            obs.PushValue(10);
            Assert.AreEqual(1, outputValues.Count, "one output should be issued");
            Assert.AreEqual(10, outputValues.Peek(), "issued value should match output");
            Assert.IsFalse(completed, "the stream should not be completed");

            obs.PushValue(11);
            Assert.AreEqual(2, outputValues.Count, "two output should be issued");
            Assert.AreEqual(11, outputValues.Peek(), "issued value should match output");
            Assert.IsFalse(completed, "the stream should not be completed");

            obs.Complete();
            Assert.IsTrue(completed, "the stream should be completed");
        }

        [TestCategory(nameof(FilterSubjectTests))]
        [TestMethod]
        public void FilterWithErrors()
        {
            var outputValues = new Stack<int>();
            var errors = new Stack<Exception>();
            var completed = false;
            var obs = new PushSubject<int>();

            var mapped = obs.Filter(i => 10 / i > 0);
            mapped.Subscribe(outputValues.Push, () => completed = true, errors.Push);

            obs.PushValue(-10);
            Assert.AreEqual(0, outputValues.Count, "no output should be issued");
            Assert.IsFalse(completed, "the stream should not be completed");

            obs.PushValue(0);
            Assert.AreEqual(0, outputValues.Count, "no output should be issued");
            Assert.IsFalse(completed, "the stream should not be completed");
            Assert.IsInstanceOfType(errors.Peek(), typeof(DivideByZeroException), "a division by zero should be received in the stream");

            obs.PushValue(10);
            Assert.AreEqual(1, outputValues.Count, "one output should be issued");
            Assert.AreEqual(10, outputValues.Peek(), "issued value should match output");
            Assert.IsFalse(completed, "the stream should not be completed");

            obs.PushValue(9);
            Assert.AreEqual(2, outputValues.Count, "two output should be issued");
            Assert.AreEqual(9, outputValues.Peek(), "issued value should match output");
            Assert.IsFalse(completed, "the stream should not be completed");

            obs.PushValue(11);
            Assert.AreEqual(2, outputValues.Count, "two output should be issued");
            Assert.IsFalse(completed, "the stream should not be completed");

            obs.Complete();
            Assert.IsTrue(completed, "the stream should be completed");
        }

        //[TestCategory(nameof(FilterSubjectTests))]
        //[TestMethod]
        //public void ErrorInMap()
        //{
        //    var outputValues = new Stack<int>();
        //    var errors = new Stack<Exception>();
        //    var completed = false;
        //    var obs = new PushSubject<int>();

        //    var mapped = obs.Map(i => 10 / i);
        //    mapped.Subscribe(outputValues.Push, () => completed = true, errors.Push);

        //    for (int value = -2; value < 0; value++)
        //    {
        //        obs.PushValue(value);
        //        Assert.AreEqual(10 / value, outputValues.Peek(), "the output value should match the map");
        //        Assert.AreEqual(0, errors.Count, "no error should be submitted");
        //    }
        //    obs.PushValue(0);
        //    Assert.AreEqual(2, outputValues.Count, "no value should be added if error");
        //    Assert.IsInstanceOfType(errors.Peek(), typeof(DivideByZeroException), "a division by zero should be received in the stream");

        //    obs.PushValue(1);
        //    Assert.AreEqual(10 / 1, outputValues.Peek(), "the output value should match the map");
        //    Assert.AreEqual(1, errors.Count, "no nore error should be submitted");

        //    obs.Complete();
        //    Assert.IsTrue(completed, "the stream should be completed");
        //}
    }
}
