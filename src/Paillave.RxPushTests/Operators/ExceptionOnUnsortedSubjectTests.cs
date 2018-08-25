using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.RxPush.Operators;

namespace Paillave.RxPushTests.Operators
{
    [TestClass]
    public class ExceptionOnUnsortedSubjectTests
    {
        [TestCategory(nameof(ExceptionOnUnsortedSubjectTests))]
        [TestMethod]
        public void SortedValues()
        {
            var inputValues = new[] { -2, -1, 0, 1, 2 };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);// new PushSubject<int>();
            var filtered = obs.ExceptionOnUnsorted(i => i);
            var output = filtered.ToListAsync();
            var errorList = filtered.ExceptionsToObservable().ToListAsync();

            obs.Start();
            output.Wait();
            errorList.Wait();
            CollectionAssert.AreEquivalent(inputValues, output.Result, "the output should be the same than the input");
            Assert.AreEqual(0, errorList.Result.Count, "no exception should be issued in a sorted stream");
        }

        [TestCategory(nameof(ExceptionOnUnsortedSubjectTests))]
        [TestMethod]
        public void UnSortedValues()
        {
            var inputValues = new[] { -2, 0, -1, 1, 2 };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);// new PushSubject<int>();
            var filtered = obs.ExceptionOnUnsorted(i => i);
            var output = filtered.ToListAsync();
            var errorList = filtered.ExceptionsToObservable().ToListAsync();

            obs.Start();
            output.Wait();
            errorList.Wait();
            CollectionAssert.AreEquivalent(inputValues.Where(i => i != -1).ToList(), output.Result, "the output should contains only sorted values");
            Assert.AreEqual(1, errorList.Result.Count, "one exception should be issued");
        }

        [TestCategory(nameof(ExceptionOnUnsortedSubjectTests))]
        [TestMethod]
        public void DistinctSortedValuesForDistinct()
        {
            var inputValues = new[] { -2, -1, 0, 1, 2 };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);// new PushSubject<int>();
            var filtered = obs.ExceptionOnUnsorted(i => i, SortOrder.Ascending, true);
            var output = filtered.ToListAsync();
            var errorList = filtered.ExceptionsToObservable().ToListAsync();

            obs.Start();
            output.Wait();
            errorList.Wait();
            CollectionAssert.AreEquivalent(inputValues, output.Result, "the output should be the same than the input");
            Assert.AreEqual(0, errorList.Result.Count, "no exception should be issued in a sorted stream");
        }

        [TestCategory(nameof(ExceptionOnUnsortedSubjectTests))]
        [TestMethod]
        public void UnSortedValuesForDistinct()
        {
            var inputValues = new[] { -2, 0, -1, 1, 2 };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);// new PushSubject<int>();
            var filtered = obs.ExceptionOnUnsorted(i => i, SortOrder.Ascending, true);
            var output = filtered.ToListAsync();
            var errorList = filtered.ExceptionsToObservable().ToListAsync();

            obs.Start();
            output.Wait();
            errorList.Wait();
            CollectionAssert.AreEquivalent(inputValues.Where(i => i != -1).ToList(), output.Result, "the output should contains only sorted values");
            Assert.AreEqual(1, errorList.Result.Count, "one exception should be issued");
        }

        [TestCategory(nameof(ExceptionOnUnsortedSubjectTests))]
        [TestMethod]
        public void UndistinctSortedValuesForDistinct()
        {
            var inputValues = new[] { -2, -1, -1, 0, 1, 2 };
            var outputValues = new List<int>();
            var obs = PushObservable.FromEnumerable(inputValues);// new PushSubject<int>();
            var filtered = obs.ExceptionOnUnsorted(i => i, SortOrder.Ascending, true);
            var output = filtered.ToListAsync();
            var errorList = filtered.ExceptionsToObservable().ToListAsync();

            obs.Start();
            output.Wait();
            errorList.Wait();
            CollectionAssert.AreEquivalent(new[] { -2, -1, 0, 1, 2 }, output.Result, "the output should contains only sorted values");
            Assert.AreEqual(1, errorList.Result.Count, "one exception should be issued");
        }
    }
}
