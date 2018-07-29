using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class ScanSubjectTests
    {
        [TestCategory(nameof(ScanSubjectTests))]
        [TestMethod]
        public void ScanValues()
        {
            var valueStack = new Stack<List<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;

            var obs = new PushSubject<int>();

            var output = obs.Scan((acc, val) => { acc = acc.ToList(); acc.Add(val); return acc; }, new List<int>());

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            for (int size = 0; size < 4; size++)
            {
                obs.PushValue(size);
                Assert.IsTrue(AreListEquals(valueStack.Peek(), size), "the accumulation should be done");

                var ex = new Exception();
                obs.PushException(ex);
                Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "input errors should go in the output stream");
            }
            obs.Complete();
            Assert.IsTrue(isComplete, "the stream must be completed");
        }
        private bool AreListEquals(List<int> lst1, int size)
        {
            if (lst1.Count != size + 1) return false;
            for (int i = 0; i < size; i++)
                if (lst1[i] != i) return false;
            return true;
        }
    }
}
