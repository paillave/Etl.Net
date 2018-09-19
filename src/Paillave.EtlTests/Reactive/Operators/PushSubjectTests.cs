using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class PushSubjectTests
    {
        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void PushSimpleError()
        {
            var errors = new List<Exception>();
            var values = new List<int>();
            bool completed = false;
            var tmp = new PushSubject<int>();
            tmp.Subscribe(values.Add, () => completed = true, errors.Add);
            var exception = new Exception();
            tmp.PushException(exception);
            Assert.AreSame(exception, errors[0], "the exception should be retrieved");
            Assert.IsFalse(completed, "shouldn't be completed");
            tmp.PushValue(1);
            Assert.AreEqual(1, values[0], "pushed value should still go in the stream after exception");
        }

        [TestCategory(nameof(RangeSubjectTests))]
        [TestMethod]
        public void PushTwoErrors()
        {
            var errors = new List<Exception>();
            var values = new List<int>();
            bool completed = false;
            var tmp = new PushSubject<int>();
            tmp.Subscribe(values.Add, () => completed = true, errors.Add);
            var exception = new Exception();
            tmp.PushException(exception);
            Assert.AreSame(exception, errors[0], "the exception should be retrieved");

            exception = new Exception();
            tmp.PushException(exception);
            Assert.AreSame(exception, errors[1], "multiple exceptions can be sent in the stream");

            Assert.IsFalse(completed, "shouldn't be completed");
            tmp.PushValue(1);
            Assert.AreEqual(1, values[0], "pushed value should still go in the stream after exception");
        }

        [TestCategory(nameof(PushSubjectTests))]
        [TestMethod()]
        public void PushSimpleValue()
        {
            var values = new List<int>();
            bool completed = false;
            var tmp = new PushSubject<int>();
            tmp.Subscribe(values.Add, () => completed = true);
            tmp.PushValue(1);
            Assert.AreEqual(1, values[0], "pushed value doesn't match");
            Assert.IsFalse(completed, "shouldn't be completed");
        }

        [TestCategory(nameof(PushSubjectTests))]
        [TestMethod()]
        public void PushValueAfterComplete()
        {
            var values = new List<int>();
            var tmp = new PushSubject<int>();
            tmp.Subscribe(values.Add);
            tmp.Complete();
            tmp.PushValue(1);
            Assert.AreEqual(0, values.Count, "pushed value should not be streamed");
        }

        [TestCategory(nameof(PushSubjectTests))]
        [TestMethod()]
        public void PushErrorAfterComplete()
        {
            var errors = new List<Exception>();
            var tmp = new PushSubject<int>();
            tmp.Subscribe(_ => { }, () => { }, errors.Add);
            tmp.Complete();
            tmp.PushException(new Exception());
            Assert.AreEqual(0, errors.Count, "pushed errors should not be streamed");
        }

        [TestCategory(nameof(PushSubjectTests))]
        [TestMethod()]
        public void PushMulipleValues()
        {
            int nb = 10;
            var inputValues = Enumerable.Range(0, nb).ToList();
            var outputValues = new List<int>();
            bool completed = false;
            var tmp = new PushSubject<int>();
            tmp.Subscribe(outputValues.Add, () => completed = true);
            foreach (var item in inputValues)
                tmp.PushValue(item);
            for (int i = 0; i < nb; i++)
                Assert.AreEqual(inputValues[i], outputValues[i], "all values should be the same");
            Assert.AreEqual(inputValues.Count, outputValues.Count, "nb items from the input source must be the same that in the output");
            Assert.IsFalse(completed, "shouldn't be completed");
        }

        [TestCategory(nameof(PushSubjectTests))]
        [TestMethod()]
        public void PushCompleted()
        {
            bool valueSubmitted = false;
            bool completed = false;
            var tmp = new PushSubject<int>();
            tmp.Subscribe(i => valueSubmitted = true, () => completed = true);
            tmp.Complete();
            Assert.IsTrue(completed, "stream should be completed");
            tmp.PushValue(1);
            Assert.IsFalse(valueSubmitted, "no more value should be submitted to the output stream");
        }
    }
}
