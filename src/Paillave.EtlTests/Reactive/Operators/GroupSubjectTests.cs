using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class GroupSubjectTests
    {
        [TestCategory(nameof(GroupSubjectTests))]
        [TestMethod]
        public void SplitObservables()
        {
            var lastValues = new List<int>();
            bool isCompleted = false;
            // var lastValues=new List<KeyValuePair<int, int>>();
            IPushSubject<KeyValuePair<int, int>> src = new PushSubject<KeyValuePair<int, int>>();
            var resS = src.Group(i => i.Key, iS => iS.Last().Map(i => i.Value));
            resS.Subscribe(i => lastValues.Add(i), () => isCompleted = true);

            src.PushValue(new KeyValuePair<int, int>(1, 1));
            src.PushValue(new KeyValuePair<int, int>(1, 2));
            src.PushValue(new KeyValuePair<int, int>(2, 3));
            src.PushValue(new KeyValuePair<int, int>(2, 4));
            src.PushValue(new KeyValuePair<int, int>(1, 5));
            src.PushValue(new KeyValuePair<int, int>(1, 6));
            src.Complete();
            CollectionAssert.AreEquivalent(new[] { 6, 4 }, lastValues);
            Assert.IsTrue(isCompleted);
        }
        [TestCategory(nameof(GroupSubjectTests))]
        [TestMethod]
        public void SplitObservablesEndingBeforeInput()
        {
            var lastValues = new List<int>();
            bool isCompleted = false;
            // var lastValues=new List<KeyValuePair<int, int>>();
            IPushSubject<KeyValuePair<int, int>> src = new PushSubject<KeyValuePair<int, int>>();
            var resS = src.Group(i => i.Key, iS => iS.First().Map(i => i.Value));
            resS.Subscribe(i => lastValues.Add(i), () => isCompleted = true);

            src.PushValue(new KeyValuePair<int, int>(1, 1));
            src.PushValue(new KeyValuePair<int, int>(1, 2));
            src.PushValue(new KeyValuePair<int, int>(2, 3));
            src.PushValue(new KeyValuePair<int, int>(2, 4));
            src.PushValue(new KeyValuePair<int, int>(1, 5));
            src.PushValue(new KeyValuePair<int, int>(1, 6));
            CollectionAssert.AreEquivalent(new[] { 1, 3 }, lastValues);
            Assert.IsFalse(isCompleted);
            src.Complete();
            Assert.IsTrue(isCompleted);
        }
        [TestCategory(nameof(GroupSubjectTests))]
        [TestMethod]
        public void SplitObservables2()
        {
            var lastValues = new List<int>();
            bool isCompleted = false;
            // var lastValues=new List<KeyValuePair<int, int>>();
            IPushSubject<KeyValuePair<int, int>> src = new PushSubject<KeyValuePair<int, int>>();
            var resS = src.Group(i => i.Key, iS => iS.Scan((acc, val) => acc + val.Value, 0));
            resS.Subscribe(i => lastValues.Add(i), () => isCompleted = true);

            src.PushValue(new KeyValuePair<int, int>(1, 1));
            src.Complete();
            CollectionAssert.AreEquivalent(new[] { 1 }, lastValues);
            Assert.IsTrue(isCompleted);
        }
        [TestCategory(nameof(GroupSubjectTests))]
        [TestMethod]
        public void SplitObservables3()
        {
            var lastValues = new List<int>();
            bool isCompleted = false;
            // var lastValues=new List<KeyValuePair<int, int>>();
            IPushSubject<KeyValuePair<int, int>> src = new PushSubject<KeyValuePair<int, int>>();
            var resS = src.Group(i => i.Key, iS => iS.Scan((acc, val) => acc + val.Value, 0).Last());
            resS.Subscribe(i => lastValues.Add(i), () => isCompleted = true);

            src.PushValue(new KeyValuePair<int, int>(1, 1));
            src.PushValue(new KeyValuePair<int, int>(1, 2));
            src.PushValue(new KeyValuePair<int, int>(2, 3));
            src.PushValue(new KeyValuePair<int, int>(2, 4));
            src.PushValue(new KeyValuePair<int, int>(1, 5));
            src.PushValue(new KeyValuePair<int, int>(1, 6));
            src.Complete();
            CollectionAssert.AreEquivalent(new[] { 14, 7 }, lastValues);
            Assert.IsTrue(isCompleted);
        }
        [TestCategory(nameof(GroupSubjectTests))]
        [TestMethod]
        public void SplitEmptyObservables1()
        {
            var lastValues = new List<int>();
            bool isCompleted = false;
            // var lastValues=new List<KeyValuePair<int, int>>();
            IPushSubject<KeyValuePair<int, int>> src = new PushSubject<KeyValuePair<int, int>>();
            var resS = src.Group(i => i.Key, iS => iS.Map(i => i.Value).Last());
            resS.Subscribe(i => lastValues.Add(i), () => isCompleted = true);

            src.Complete();
            CollectionAssert.AreEquivalent(new int[0], lastValues);
            Assert.IsTrue(isCompleted);
        }
    }
}
