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
            var right = PushObservable.FromEnumerable(new[] { 2, 5, 5, 6 });
            var output = left.Substract(right, i => i, i => i);

            var task = output.ToListAsync();

            left.Start();
            right.Start();
            task.Wait();
            CollectionAssert.AreEquivalent(new[] { 1, 3, 4, 4, 7 }, task.Result);
        }
        [TestCategory(nameof(SubstractSubjectTests))]
        [TestMethod]
        public void QuickTest2()
        {
            var rnd = new Random();
            var leftList = Enumerable.Range(0, 1000).Select(i => rnd.Next(25)).OrderBy(i => i).ToList();
            var rightList = Enumerable.Range(0, 100).Select(i => rnd.Next(50) / 2).OrderBy(i => i).ToList();
            var expected = leftList.Where(i => !rightList.Contains(i)).OrderBy(i => i).ToList();

            var left = PushObservable.FromEnumerable(leftList);
            var right = PushObservable.FromEnumerable(rightList);
            // left.Subscribe(i => System.Diagnostics.Debug.WriteLine($"left.PushValue({i});"), () => System.Diagnostics.Debug.WriteLine($"left.Complete();"));
            // right.Subscribe(i => System.Diagnostics.Debug.WriteLine($"______right.PushValue({i});"), () => System.Diagnostics.Debug.WriteLine($"______right.Complete();"));
            var output = left.Substract(right, i => i, i => i);
            // output.Subscribe(i => System.Diagnostics.Debug.WriteLine($"===output.PushValue({i});"), () => System.Diagnostics.Debug.WriteLine($"===output.Complete();"));

            var task = output.ToListAsync();

            left.Start();
            right.Start();
            task.Wait();

            System.Diagnostics.Debug.WriteLine($"->left:{string.Join(",", leftList)}");
            System.Diagnostics.Debug.WriteLine($"->right:{string.Join(",", rightList)}");
            System.Diagnostics.Debug.WriteLine($"->expected:{string.Join(",", expected)}");
            System.Diagnostics.Debug.WriteLine($"->result:{string.Join(",", task.Result)}");
            CollectionAssert.AreEquivalent(expected, task.Result);
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
            CollectionAssert.AreEquivalent(new int[] { 1, 3 }.ToList(), outputValues);
            left.PushValue(4); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4 }.ToList(), outputValues);
            left.PushValue(4); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4 }.ToList(), outputValues);
            left.PushValue(5); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4 }.ToList(), outputValues);
            left.PushValue(6); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4 }.ToList(), outputValues);
            left.PushValue(7); //<-
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4 }.ToList(), outputValues);
            left.Complete();
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4 }.ToList(), outputValues);
            ______right.Complete();
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 4, 4, 6, 7 }.ToList(), outputValues);
        }
        [TestCategory(nameof(SubstractSubjectTests))]
        [TestMethod]
        public void SteppedTest4()
        {
            var left = new PushSubject<int>();
            var ______right = new PushSubject<int>();
            var outputValues = new List<int>();
            left.Substract(______right, i => i, i => i).Subscribe(outputValues.Add);

            left.PushValue(2);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            ______right.PushValue(0);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            left.PushValue(2);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            ______right.PushValue(2);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            left.PushValue(4);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            left.PushValue(4);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            ______right.PushValue(3);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            left.PushValue(5);
            CollectionAssert.AreEquivalent(new int[] { }.ToList(), outputValues);
            ______right.PushValue(6);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5 }.ToList(), outputValues);
            left.PushValue(5);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            ______right.PushValue(6);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            ______right.PushValue(7);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            left.PushValue(8);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            ______right.PushValue(7);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            ______right.PushValue(8);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            left.PushValue(9);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            left.PushValue(9);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            ______right.PushValue(9);
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            left.Complete();
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
            ______right.Complete();
            CollectionAssert.AreEquivalent(new int[] { 4, 4, 5, 5 }.ToList(), outputValues);
        }
        [TestCategory(nameof(SubstractSubjectTests))]
        [TestMethod]
        public void SteppedTest5()
        {
            var left = new PushSubject<int>();
            var ______right = new PushSubject<int>();
            var outputValues = new List<int>();
            left.Substract(______right, i => i, i => i).Subscribe(outputValues.Add);

            left.PushValue(1);
            ______right.PushValue(2);
            left.PushValue(2);
            ______right.PushValue(5);
            left.PushValue(2);
            left.PushValue(3);
            left.PushValue(4);
            ______right.PushValue(5);
            left.PushValue(4);
            ______right.PushValue(6);
            left.PushValue(5);
            ______right.Complete();
            left.PushValue(6);
            left.PushValue(7);
            left.PushValue(7);
            left.PushValue(8);
            left.Complete();
            CollectionAssert.AreEquivalent(new[] { 1, 3, 4, 4, 7, 7, 8 }, outputValues.ToArray());
        }
    }
}
