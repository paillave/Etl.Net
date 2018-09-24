using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl.Reactive.Core;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Operators;
using System.Threading;

namespace Paillave.EtlTests.Reactive.Operators
{
    [TestClass]
    public class ToTaskTests
    {
        [TestCategory(nameof(ToTaskTests))]
        [TestMethod]
        public void SinglePushTriggeredWithEventWait()
        {
            var outputValue = 0;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = PushObservable.FromSingle(1, waitHandle);
            var task = observable.ToTaskAsync();
            observable.Subscribe(i => outputValue = i);
            waitHandle.Set();
            var res = task.Result;
            Assert.AreEqual(1, outputValue);
            Assert.AreEqual(1, res);
        }

        [TestCategory(nameof(ToTaskTests))]
        [TestMethod]
        public void SinglePushTriggeredWithStart()
        {
            var outputValue = 0;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = PushObservable.FromSingle(1);
            var task = observable.ToTaskAsync();
            observable.Subscribe(i => outputValue = i);
            observable.Start();
            var res = task.Result;
            Assert.AreEqual(1, outputValue);
            Assert.AreEqual(1, res);
        }

        /// <summary>
        /// FAILS ALWAYS
        /// </summary>
        [TestCategory(nameof(ToTaskTests))]
        [TestMethod]
        public void ListPushTriggeredWithEventWait()
        {
            var outputValue = 0;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = PushObservable.FromEnumerable(new[] { 1 }, waitHandle);
            var task = observable.ToTaskAsync();
            observable.Subscribe(i => outputValue = i);
            waitHandle.Set();
            var res = task.Result;
            Assert.AreEqual(1, outputValue);
            Assert.AreEqual(1, res);
        }


        /// <summary>
        /// FAILS NEARLY ALWAYS
        /// </summary>
        [TestCategory(nameof(ToTaskTests))]
        [TestMethod]
        public void ListPushTriggeredWithStart()
        {
            var outputValue = 0;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = PushObservable.FromEnumerable(new[] { 1 });
            var task = observable.ToTaskAsync();
            observable.Subscribe(i => outputValue = i);
            observable.Start();
            var res = task.Result;
            Assert.AreEqual(1, outputValue);
            Assert.AreEqual(1, res);
        }
        [TestCategory(nameof(ToTaskTests))]
        [TestMethod]
        public void ListToListPushTriggeredWithEventWait()
        {
            var outputValue = 0;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = PushObservable.FromEnumerable(new[] { 1 }, waitHandle);
            var task = observable.ToListAsync();
            observable.Subscribe(i => outputValue = i);
            waitHandle.Set();
            var res = task.Result;
            Assert.AreEqual(1, outputValue);
            CollectionAssert.AreEquivalent(new[] { 1 }, res);
        }

        [TestCategory(nameof(ToTaskTests))]
        [TestMethod]
        public void ListToListPushTriggeredWithStart()
        {
            var outputValue = 0;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var observable = PushObservable.FromEnumerable(new[] { 1 });
            var task = observable.ToListAsync();
            observable.Subscribe(i => outputValue = i);
            observable.Start();
            var res = task.Result;
            Assert.AreEqual(1, outputValue);
            CollectionAssert.AreEquivalent(new[] { 1 }, res);
        }
    }
}
