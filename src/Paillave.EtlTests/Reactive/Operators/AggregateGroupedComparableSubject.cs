using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass()]
    public class AggregateGroupedComparableSubject
    {
        [TestCategory(nameof(AggregateGroupedComparableSubject))]
        [TestMethod]
        public void NoElements()
        {
            var inputValues = new int[] { };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);
            var task = obs.AggregateGrouped((i) => new List<int>(), SortDefinition.Create((int i) => i % 3), (List<int> a, int i) => a.Union(new[] { i }).ToList(), (i, a) => new { Key = i, Value = a }).ToListAsync();
            obs.Start();
            task.Wait();
            Assert.AreEqual(0, task.Result.Count, "the output stream should be empty");
        }
        [TestCategory(nameof(AggregateGroupedComparableSubject))]
        [TestMethod]
        public void OneElements()
        {
            var inputValues = new int[] { 0 };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);
            var task = obs.AggregateGrouped((i) => new List<int>(), SortDefinition.Create((int i) => i % 3), (List<int> a, int i) => a.Union(new[] { i }).ToList(), (i, a) => new { Key = i, Value = a }).ToListAsync();
            obs.Start();
            task.Wait();
            Assert.AreEqual(1, task.Result.Count, "the output stream should have one element");
            CollectionAssert.AreEquivalent(new[] { 0 }, task.Result[0].Value);
            Assert.AreEqual(0, task.Result[0].Key);
        }
        [TestCategory(nameof(AggregateGroupedComparableSubject))]
        [TestMethod]
        public void SeveralSortedElements()
        {
            var inputValues = new int[] { 0, 3, 1, 4, 2, 5 };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);
            var task = obs.AggregateGrouped(
                (i) => new List<int>(),
                SortDefinition.Create((int i) => i % 3),
                (List<int> a, int i) => a.Union(new[] { i }).ToList(),
                (i, a) => new { Key = i, Value = a }
            ).ToListAsync();
            obs.Start();
            task.Wait();
            Assert.AreEqual(3, task.Result.Count, "the output stream should have one element");

            CollectionAssert.AreEquivalent(new[] { 0, 3 }, task.Result[0].Value);
            Assert.AreEqual(0, task.Result[0].Key);

            CollectionAssert.AreEquivalent(new[] { 1, 4 }, task.Result[1].Value);
            Assert.AreEqual(1, task.Result[1].Key);

            CollectionAssert.AreEquivalent(new[] { 2, 5 }, task.Result[2].Value);
            Assert.AreEqual(2, task.Result[2].Key);
        }
    }
}
