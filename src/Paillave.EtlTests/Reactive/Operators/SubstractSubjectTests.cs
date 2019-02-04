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
    public class SubstractSubjectTests
    {
        [TestCategory(nameof(SubstractSubjectTests))]
        [TestMethod]
        public void QuickTest()
        {
            var left = PushObservable.FromEnumerable(new[] { 1, 2, 2, 3, 4, 4, 5, 6, 7 });
            var right = PushObservable.FromEnumerable(new[] { 2, 5 });
            left.Subscribe(i => System.Diagnostics.Debug.WriteLine($"left:{i}"));
            right.Subscribe(i => System.Diagnostics.Debug.WriteLine($"_____right:{i}"));
            var output = left.Substract(right, i => i, i => i);

            var task = output.ToListAsync();

            left.Start();
            right.Start();
            task.Wait();
            CollectionAssert.AreEquivalent(new[] { 1, 3, 4, 4, 6, 7 }, task.Result);
        }
        [TestCategory(nameof(SubstractSubjectTests))]
        [TestMethod]
        public void SteppedTest1()
        {
            var left = new PushSubject<int>();
            var right = new PushSubject<int>();
            var outputValues = new List<int>();
            left.Substract(right, i => i, i => i).Subscribe(outputValues.Add);

            left.PushValue(1);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            right.PushValue(2);
            CollectionAssert.AreEquivalent(new int[] { 1 }.ToList(), outputValues);
        }
        [TestCategory(nameof(SubstractSubjectTests))]
        [TestMethod]
        public void SteppedTest2()
        {
            var left = new PushSubject<int>();
            var ______right = new PushSubject<int>();
            var outputValues = new List<int>();
            left.Substract(______right, i => i, i => i).Subscribe(outputValues.Add);

            left.PushValue(1); //<-
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            left.PushValue(2); //<-
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            left.PushValue(2); //<-
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            left.PushValue(3); //<-
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            ______right.PushValue(2); //->
            CollectionAssert.AreEquivalent(new int[] { 1 }.ToList(), outputValues);
            ______right.PushValue(5); //->
            CollectionAssert.AreEquivalent(new int[] { 1, 3 }.ToList(), outputValues);
            left.PushValue(4); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4 }.ToList(), outputValues);
            left.PushValue(5); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4 }.ToList(), outputValues);
            left.PushValue(6); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4 }.ToList(), outputValues);
            left.Complete();
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4 }.ToList(), outputValues);
            ______right.Complete();
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 6 }.ToList(), outputValues);
        }
        [TestCategory(nameof(SubstractSubjectTests))]
        [TestMethod]
        public void SteppedTest3()
        {
            var left = new PushSubject<int>();
            var ______right = new PushSubject<int>();
            var outputValues = new List<int>();
            left.Substract(______right, i => i, i => i).Subscribe(outputValues.Add);

            left.PushValue(1);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            ______right.PushValue(2);
            CollectionAssert.AreEquivalent(new int[] { 1 }.ToList(), outputValues);
            left.PushValue(2);
            CollectionAssert.AreEquivalent(new int[] { 1 }.ToList(), outputValues);
            ______right.PushValue(5);
            CollectionAssert.AreEquivalent(new int[] { 1 }.ToList(), outputValues);
            left.PushValue(2);
            CollectionAssert.AreEquivalent(new int[] { 1 }.ToList(), outputValues);
            left.PushValue(3); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3}.ToList(), outputValues);
            left.PushValue(4); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4 }.ToList(), outputValues);
            left.PushValue(4); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4 }.ToList(), outputValues);
            left.PushValue(5); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4 }.ToList(), outputValues);
            left.PushValue(6); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4, 6 }.ToList(), outputValues);
            left.PushValue(7); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4, 6, 7 }.ToList(), outputValues);
            left.Complete();
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4, 6, 7 }.ToList(), outputValues);
            ______right.Complete();
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4, 6, 7 }.ToList(), outputValues);
        }
    }
}
