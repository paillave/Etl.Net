using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class ConcatenateSubjectTests
    {
        [TestCategory(nameof(ConcatenateSubjectTests))]
        [TestMethod]
        public void SimpleConcatenationWithTopStartingFirst()
        {
            var obs1 = PushObservable.FromEnumerable(new[] { 1, 2 });
            var obs2 = PushObservable.FromEnumerable(new[] { 3, 4 });

            var outputObs = obs1.Concatenate(obs2);
            var outputTask = outputObs.ToListAsync();

            obs1.Start();
            obs2.Start();

            outputTask.Wait();
            var output = outputTask.Result;
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, output);
        }
        [TestCategory(nameof(ConcatenateSubjectTests))]
        [TestMethod]
        public void SimpleConcatenationWithBottomStartingFirst()
        {
            var obs1 = PushObservable.FromEnumerable(new[] { 1, 2 });
            var obs2 = PushObservable.FromEnumerable(new[] { 3, 4 });

            var outputObs = obs1.Concatenate(obs2);
            var outputTask = outputObs.ToListAsync();

            obs2.Start();
            obs1.Start();

            outputTask.Wait();
            var output = outputTask.Result;
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, output);
        }
        [TestCategory(nameof(ConcatenateSubjectTests))]
        [TestMethod]
        public void SimpleConcatenationWithEmptyBottomTopStartingFirst()
        {
            var obs1 = PushObservable.FromEnumerable(new[] { 1, 2 });
            var obs2 = PushObservable.FromEnumerable(new int[] { });

            var outputObs = obs1.Concatenate(obs2);
            var outputTask = outputObs.ToListAsync();

            obs1.Start();
            obs2.Start();

            outputTask.Wait();
            var output = outputTask.Result;
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, output);
        }
        [TestCategory(nameof(ConcatenateSubjectTests))]
        [TestMethod]
        public void SimpleConcatenationWithEmptyBottomBottomStartingFirst()
        {
            var obs1 = PushObservable.FromEnumerable(new[] { 1, 2 });
            var obs2 = PushObservable.FromEnumerable(new int[] {  });

            var outputObs = obs1.Concatenate(obs2);
            var outputTask = outputObs.ToListAsync();

            obs2.Start();
            obs1.Start();

            outputTask.Wait();
            var output = outputTask.Result;
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, output);
        }
        [TestCategory(nameof(ConcatenateSubjectTests))]
        [TestMethod]
        public void SimpleConcatenationWithEmptyTopTopStartingFirst()
        {
            var obs1 = PushObservable.FromEnumerable(new int[] { });
            var obs2 = PushObservable.FromEnumerable(new[] { 3, 4 });

            var outputObs = obs1.Concatenate(obs2);
            var outputTask = outputObs.ToListAsync();

            obs1.Start();
            obs2.Start();

            outputTask.Wait();
            var output = outputTask.Result;
            CollectionAssert.AreEquivalent(new[] { 3, 4 }, output);
        }
        [TestCategory(nameof(ConcatenateSubjectTests))]
        [TestMethod]
        public void SimpleConcatenationWithEmptyTopBottomStartingFirst()
        {
            var obs1 = PushObservable.FromEnumerable(new int[] { });
            var obs2 = PushObservable.FromEnumerable(new[] { 3, 4 });

            var outputObs = obs1.Concatenate(obs2);
            var outputTask = outputObs.ToListAsync();

            obs2.Start();
            obs1.Start();

            outputTask.Wait();
            var output = outputTask.Result;
            CollectionAssert.AreEquivalent(new[] { 3, 4 }, output);
        }
    }
}
