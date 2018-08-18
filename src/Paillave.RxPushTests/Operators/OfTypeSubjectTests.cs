using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class OfTypeSubjectTests
    {
        interface IInterface1 { }
        class MyClass1 : IInterface1 { }
        class MyClass2 : IInterface1 { }
        [TestCategory(nameof(OfTypeSubjectTests))]
        [TestMethod]
        public void FilterValues()
        {
            var inputValues = new IInterface1[] { new MyClass1(), new MyClass2(), new MyClass1(), new MyClass2() };
            var outputValues = new List<MyClass1>();
            var obs = new PushSubject<IInterface1>();
            var filtered = obs.OfType<MyClass1>();
            filtered.Subscribe(outputValues.Add);

            foreach (var item in inputValues)
                obs.PushValue(item);

            obs.Complete();

            Assert.IsTrue(filtered.ToTaskAsync().Wait(5000), "The filtering should complete");

            var expected = inputValues.Where(i => i is MyClass1).ToList();

            for (int i = 0; i < outputValues.Count; i++)
                Assert.AreSame(expected[i], outputValues[i], "all values should match");
            Assert.AreEqual(expected.Count, outputValues.Count, $"nb items from the output must match the input one");
        }

        //[TestCategory(nameof(OfTypeSubjectTests))]
        //[TestMethod]
        //public void Synchronisation()
        //{
        //    var outputValues = new Stack<int>();
        //    var errors = new Stack<Exception>();
        //    var completed = false;
        //    var obs = new PushSubject<int>();

        //    var mapped = obs.Filter(i => i > 0);
        //    mapped.Subscribe(outputValues.Push, () => completed = true, errors.Push);

        //    obs.PushValue(0);
        //    Assert.AreEqual(0, outputValues.Count, "no output should be issued");
        //    Assert.IsFalse(completed, "the stream should not be completed");

        //    obs.PushValue(10);
        //    Assert.AreEqual(1, outputValues.Count, "one output should be issued");
        //    Assert.AreEqual(10, outputValues.Peek(), "issued value should match output");
        //    Assert.IsFalse(completed, "the stream should not be completed");

        //    obs.PushValue(11);
        //    Assert.AreEqual(2, outputValues.Count, "two output should be issued");
        //    Assert.AreEqual(11, outputValues.Peek(), "issued value should match output");
        //    Assert.IsFalse(completed, "the stream should not be completed");

        //    obs.Complete();
        //    Assert.IsTrue(completed, "the stream should be completed");
        //}

        //[TestCategory(nameof(OfTypeSubjectTests))]
        //[TestMethod]
        //public void FilterWithErrors()
        //{
        //    var outputValues = new Stack<int>();
        //    var errors = new Stack<Exception>();
        //    var completed = false;
        //    var obs = new PushSubject<int>();

        //    var mapped = obs.Filter(i => 10 / i > 0);
        //    mapped.Subscribe(outputValues.Push, () => completed = true, errors.Push);

        //    obs.PushValue(-10);
        //    Assert.AreEqual(0, outputValues.Count, "no output should be issued");
        //    Assert.IsFalse(completed, "the stream should not be completed");

        //    obs.PushValue(0);
        //    Assert.AreEqual(0, outputValues.Count, "no output should be issued");
        //    Assert.IsFalse(completed, "the stream should not be completed");
        //    Assert.IsInstanceOfType(errors.Peek(), typeof(DivideByZeroException), "a division by zero should be received in the stream");

        //    obs.PushValue(10);
        //    Assert.AreEqual(1, outputValues.Count, "one output should be issued");
        //    Assert.AreEqual(10, outputValues.Peek(), "issued value should match output");
        //    Assert.IsFalse(completed, "the stream should not be completed");

        //    obs.PushValue(9);
        //    Assert.AreEqual(2, outputValues.Count, "two output should be issued");
        //    Assert.AreEqual(9, outputValues.Peek(), "issued value should match output");
        //    Assert.IsFalse(completed, "the stream should not be completed");

        //    obs.PushValue(11);
        //    Assert.AreEqual(2, outputValues.Count, "two output should be issued");
        //    Assert.IsFalse(completed, "the stream should not be completed");

        //    obs.Complete();
        //    Assert.IsTrue(completed, "the stream should be completed");
        //}
    }
}
