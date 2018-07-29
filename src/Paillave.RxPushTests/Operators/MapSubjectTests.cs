using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class MapSubjectTests
    {
        [TestCategory(nameof(MapSubjectTests))]
        [TestMethod]
        public void MapValues()
        {
            var inputValues = new[] { true, false, true, false, false };
            var outputValues = new List<bool>();
            var obs = new PushSubject<bool>();
            var mapped = obs.Map(i => !i);
            mapped.Subscribe(outputValues.Add);

            foreach (var item in inputValues)
                obs.PushValue(item);

            obs.Complete();

            Assert.IsTrue(mapped.ToTaskAsync().Wait(5000), "The mapping should complete");

            for (int i = 0; i < outputValues.Count; i++)
                Assert.AreEqual(!inputValues[i], outputValues[i], "all values should match the result of the map definition");
            Assert.AreEqual(inputValues.Length, outputValues.Count, $"nb items from the output must match the input one");
        }

        [TestCategory(nameof(MapSubjectTests))]
        [TestMethod]
        public void Synchronisation()
        {
            var outputValues = new Stack<int>();
            var errors = new Stack<Exception>();
            var completed = false;
            var obs = new PushSubject<int>();

            var mapped = obs.Map(i => 10 / i);
            mapped.Subscribe(outputValues.Push, () => completed = true, errors.Push);

            for (int value = 1; value < 5; value++)
            {
                obs.PushValue(value);
                Assert.AreEqual(10 / value, outputValues.Peek(), "the output value should match the map");
                Assert.IsFalse(completed, "the stream should not be completed");

            }

            obs.Complete();
            Assert.IsTrue(completed, "the stream should be completed");
        }

        [TestCategory(nameof(MapSubjectTests))]
        [TestMethod]
        public void ErrorInMap()
        {
            var outputValues = new Stack<int>();
            var errors = new Stack<Exception>();
            var completed = false;
            var obs = new PushSubject<int>();

            var mapped = obs.Map(i => 10 / i);
            mapped.Subscribe(outputValues.Push, () => completed = true, errors.Push);

            for (int value = -2; value < 0; value++)
            {
                obs.PushValue(value);
                Assert.AreEqual(10 / value, outputValues.Peek(), "the output value should match the map");
                Assert.AreEqual(0, errors.Count, "no error should be submitted");
            }
            obs.PushValue(0);
            Assert.AreEqual(2, outputValues.Count, "no value should be added if error");
            Assert.IsInstanceOfType(errors.Peek(), typeof(DivideByZeroException), "a division by zero should be received in the stream");

            obs.PushValue(1);
            Assert.AreEqual(10 / 1, outputValues.Peek(), "the output value should match the map");
            Assert.AreEqual(1, errors.Count, "no nore error should be submitted");

            obs.Complete();
            Assert.IsTrue(completed, "the stream should be completed");
        }
    }
}
