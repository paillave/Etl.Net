using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System.Diagnostics;
using System.Linq;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class LoadFlatMapTests
    {
        [TestCategory(nameof(LoadFlatMapTests))]
        [TestMethod]
        public void Test3()
        {
            for (int counter = 0; counter < 1000000; counter++)
            {
                Stopwatch stopwatch = new Stopwatch();
                Debug.WriteLine($"START {counter}: {DateTime.Now}");
                stopwatch.Start();
                var initObs = PushObservable.FromSingle("aze");
                var task = initObs.FlatMap(i => new DeferredPushObservable<string>(push => push(i))).ToTaskAsync();
                initObs.Start();
                task.Wait(5000);
                Assert.IsTrue(task.IsCompletedSuccessfully);
                stopwatch.Stop();
                Debug.WriteLine($"STOP {counter}: {stopwatch.Elapsed}");
            }
        }
    }
}
