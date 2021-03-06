﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Operators;
using System.Threading;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class SkipSubjectTests
    {
        [TestCategory(nameof(SkipSubjectTests))]
        [TestMethod]
        public void SkipMultipleValue()
        {
            RangeOfValues(-5, 10, 3);
        }

        [TestCategory(nameof(SkipSubjectTests))]
        [TestMethod]
        public void SkipNoValues()
        {
            RangeOfValues(-5, 10, 0);
        }

        public void RangeOfValues(int start, int nb, int skip)
        {
            var outputValues = new List<int>();
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            var obs = PushObservable.Range(start, nb, waitHandle).Skip(skip);

            obs.Subscribe(outputValues.Add);

            var task = obs.ToTaskAsync();
            waitHandle.Set();
            task.Wait(5000);

            for (int i = 0; i < (nb - skip); i++)
                Assert.AreEqual(start + i + skip, outputValues[i], "all values should be the same");
            Assert.AreEqual(nb - skip, outputValues.Count, $"nb items from the output must be the same that in the input except the {skip}th ones");
        }
    }
}
