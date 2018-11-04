using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class FilterSectionSubjectTests
    {
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToIgnore1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3 };
            var expectedValues = new[] { 2 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToIgnore2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6 };
            var expectedValues = new[] { 3, 4 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToIgnore1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3 };
            var expectedValues = new[] { 1, 3 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToIgnore2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6 };
            var expectedValues = new[] { 1, 2, 5, 6 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToKeep1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3 };
            var expectedValues = new[] { 0, 2, 0 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToKeep2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6 };
            var expectedValues = new[] { 0, 3, 4, 0 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToKeep1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3 };
            var expectedValues = new[] { 1, 0, 0, 3 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToKeep2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6 };
            var expectedValues = new[] { 1, 2, 0, 0, 5, 6 };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState).ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }


















        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToIgnoreWithSectionIndex1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3 };
            var expectedValues = new[] { "0-2" };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToIgnoreWithSectionIndex2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6 };
            var expectedValues = new[] { "0-3", "0-4" };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToIgnoreWithSectionIndex1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3 };
            var expectedValues = new[] { "0-1", "1-3" };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToIgnoreWithSectionIndex2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6 };
            var expectedValues = new[] { "0-1", "0-2", "1-5", "1-6", };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchIgnoreCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToKeepWithSectionIndex1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3, 0, 4 };
            var expectedValues = new[] { "0-0", "0-2", "0-0", "1-0", "1-4" };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionWithOneTriggerToKeepWithSectionIndex2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6, 0, 7, 8 };
            var expectedValues = new[] { "0-0", "0-3", "0-4", "0-0", "1-0", "1-7", "1-8" };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToKeepWithSectionIndex1()
        {
            var inputValues = new[] { 1, 0, 2, 0, 3, 0, 4 };
            var expectedValues = new[] { "0-1", "0-0", "1-0", "1-3", "1-0" };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
        [TestCategory(nameof(FilterSectionSubjectTests))]
        [TestMethod]
        public void FilterSectionStartKeepWithOneTriggerToKeepWithSectionIndex2()
        {
            var inputValues = new[] { 1, 2, 0, 3, 4, 0, 5, 6, 0, 7, 8 };
            var expectedValues = new[] { "0-1", "0-2", "0-0", "1-0", "1-5", "1-6", "1-0" };
            var src = PushObservable.FromEnumerable(inputValues);
            var returnedTask = src.FilterSection(KeepingState.Keep, i => i == 0 ? SwitchBehavior.SwitchKeepCurrent : SwitchBehavior.KeepState, (elt, sectionIndex) => $"{sectionIndex}-{elt}").ToListAsync();
            src.Start();

            var returnedValues = returnedTask.Result;

            CollectionAssert.AreEquivalent(expectedValues, returnedValues);
        }
    }
}
