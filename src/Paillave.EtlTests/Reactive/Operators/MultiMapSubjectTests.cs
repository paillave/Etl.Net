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
    [TestClass()]
    public class MultiMapSubjectTests
    {
        [TestCategory(nameof(MultiMapSubjectTests))]
        [TestMethod]
        public void SimpleMultiMap()
        {
            var valueStack = new List<string>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs = new PushSubject<int>();

            var output = obs.MultiMap<int, string>((i, push) =>
            {
                for (int val = 0; val < i; val++) push(val.ToString());
            });

            output.Subscribe(valueStack.Add, () => isComplete = true, errorStack.Push);

            obs.PushValue(0);
            Assert.AreEqual(0, valueStack.Count, "no value should be in the output stream");

            obs.PushValue(1);
            CollectionAssert.AreEquivalent(new[] { "0" }.ToList(), valueStack);

            obs.PushValue(4);
            CollectionAssert.AreEquivalent(new[] { "0", "0", "1", "2", "3" }.ToList(), valueStack);

            obs.Complete();
            Assert.IsTrue(isComplete, "the output stream should be completed");
        }
    }
}
