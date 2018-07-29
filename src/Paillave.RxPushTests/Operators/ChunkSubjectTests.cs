using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPushTests.Operators
{
    [TestClass()]
    public class ChunkSubjectTests
    {
        [TestCategory(nameof(ChunkSubjectTests))]
        [TestMethod]
        public void NoElements()
        {
            var valueStack = new Stack<IEnumerable<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.Chunk(3);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.Complete();

            Assert.AreEqual(0, valueStack.Count(), "the output stream should be empty");
            Assert.IsTrue(isComplete, "the stream should be finished if every input stream is complete");
        }

        [TestCategory(nameof(ChunkSubjectTests))]
        [TestMethod]
        public void ExactChunkAmountOfElements()
        {
            var valueStack = new Stack<IEnumerable<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.Chunk(3);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count(), "the output stream should be empty");
            obs1.PushValue(2);
            Assert.AreEqual(0, valueStack.Count(), "the output stream should be empty");
            obs1.PushValue(3);
            Assert.AreEqual(1, valueStack.Count(), "the output stream should return a chunk of values");
            var value = valueStack.Peek().ToList();
            Assert.AreEqual(3, value.Count, "the chunk of values should be the size of the chunk");

            Assert.AreEqual(1, value[0], "All values must match");
            Assert.AreEqual(2, value[1], "All values must match");
            Assert.AreEqual(3, value[2], "All values must match");

            var ex = new Exception();
            obs1.PushException(ex);
            Assert.IsTrue(Object.ReferenceEquals(ex, errorStack.Peek()), "input errors should go in the output stream");

            obs1.Complete();

            Assert.AreEqual(1, valueStack.Count(), "the output stream should have returned one chunk");
            Assert.IsTrue(isComplete, "the stream should be finished if every input stream is complete");
        }

        [TestCategory(nameof(ChunkSubjectTests))]
        [TestMethod]
        public void NotExactChunkAmountOfElements()
        {
            var valueStack = new Stack<IEnumerable<int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var obs1 = new PushSubject<int>();

            var output = obs1.Chunk(3);

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            obs1.PushValue(1);
            Assert.AreEqual(0, valueStack.Count(), "the output stream should be empty");
            obs1.PushValue(2);
            Assert.AreEqual(0, valueStack.Count(), "the output stream should be empty");
            obs1.PushValue(3);
            Assert.AreEqual(1, valueStack.Count(), "the output stream should return a chunk of values");
            var value = valueStack.Peek().ToList();
            Assert.AreEqual(3, value.Count, "the chunk of values should be the size of the chunk");

            Assert.AreEqual(1, value[0], "All values must match");
            Assert.AreEqual(2, value[1], "All values must match");
            Assert.AreEqual(3, value[2], "All values must match");

            obs1.PushValue(4);
            Assert.AreEqual(1, valueStack.Count(), "the output stream should not have more chunk of values");

            obs1.Complete();

            Assert.AreEqual(2, valueStack.Count(), "the output stream should have returned a second chunk");

            value = valueStack.Peek().ToList();
            Assert.AreEqual(1, value.Count, "the chunk of values should be the size of the remaining items in the stream");

            Assert.AreEqual(4, value[0], "All values must match");

            Assert.IsTrue(isComplete, "the stream should be finished if every input stream is complete");
        }
    }
}
