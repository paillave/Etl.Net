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
    public class LeftJoinSubjectTests
    {
        [TestCategory(nameof(LeftJoinSubjectTests))]
        [TestMethod]
        public void SimpleJoin()
        {
            var valueStack = new Stack<Tuple<int, int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var leftS = new PushSubject<int>();
            var rightS = new PushSubject<int>();

            var output = leftS.LeftJoin(rightS, i => i, i => i, null, (l, r) => new Tuple<int, int>(l, r));

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            leftS.PushValue(1);

            Assert.AreEqual(0, valueStack.Count, "no value should be in the output stream yet");

            rightS.PushValue(1);

            var outElt = valueStack.Peek();
            Assert.AreEqual(1, outElt.Item1, "a joined output should be issued");
            Assert.IsTrue(outElt.Item1 == outElt.Item2, "joined values should match");

            leftS.PushValue(1);

            outElt = valueStack.Peek();
            Assert.AreEqual(2, valueStack.Count, "a joined output should be issued");
            Assert.AreEqual(1, outElt.Item1, "a joined output should be issued");
            Assert.IsTrue(outElt.Item1 == outElt.Item2, "joined values should match");

            rightS.PushValue(2);
            Assert.AreEqual(2, valueStack.Count, "no more output should be issued");

            rightS.PushValue(3);
            Assert.AreEqual(2, valueStack.Count, "no more output should be issued");

            leftS.PushValue(2);

            outElt = valueStack.Peek();
            Assert.AreEqual(3, valueStack.Count, "a joined output should be issued");
            Assert.AreEqual(2, outElt.Item1, "a joined output should be issued");
            Assert.IsTrue(outElt.Item1 == outElt.Item2, "joined values should match");

            leftS.PushValue(2);

            outElt = valueStack.Peek();
            Assert.AreEqual(4, valueStack.Count, "a joined output should be issued");
            Assert.AreEqual(2, outElt.Item1, "a joined output should be issued");
            Assert.IsTrue(outElt.Item1 == outElt.Item2, "joined values should match");

            leftS.PushValue(3);

            outElt = valueStack.Peek();
            Assert.AreEqual(5, valueStack.Count, "a joined output should be issued");
            Assert.AreEqual(3, outElt.Item1, "a joined output should be issued");
            Assert.IsTrue(outElt.Item1 == outElt.Item2, "joined values should match");

            leftS.Complete();
            Assert.IsTrue(isComplete, "output stream should be completed");
        }

        [TestCategory(nameof(LeftJoinSubjectTests))]
        [TestMethod]
        public void SimpleJoin2()
        {
            var valueStack = new Stack<Tuple<int, int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var leftS = new PushSubject<int>();
            var rightS = new PushSubject<int>();

            var output = leftS.LeftJoin(rightS, i => i, i => i, null, (l, r) => new Tuple<int, int>(l, r));

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            leftS.PushValue(1);
            leftS.Complete();
            Assert.AreEqual(0, valueStack.Count, "no value should be in the output stream yet");
            Assert.IsFalse(isComplete, "as long as nothing is issued on the left whereas the right is not complete and the left has still no match, the stream should not complete");

            rightS.PushValue(1);

            var outElt = valueStack.Peek();
            Assert.AreEqual(1, outElt.Item1, "a joined output should be issued");
            Assert.IsTrue(outElt.Item1 == outElt.Item2, "joined values should match");
            Assert.IsTrue(isComplete, "output stream should be completed");
        }

        [TestCategory(nameof(LeftJoinSubjectTests))]
        [TestMethod]
        public void RightSubmitsNextBeforeLeftGetsMatch()
        {
            var valueStack = new Stack<Tuple<int, int>>();
            var errorStack = new Stack<Exception>();
            //bool isComplete = false;
            var leftS = new PushSubject<int>();
            var rightS = new PushSubject<int>();

            var output = leftS.LeftJoin(rightS, i => i, i => i, null, (l, r) => new Tuple<int, int>(l, r));

            output.Subscribe(valueStack.Push, () => { }, errorStack.Push);

            leftS.PushValue(1);

            Assert.AreEqual(0, valueStack.Count, "no value should be submitted to the ouput stream");

            rightS.PushValue(2);

            var outValue = valueStack.Peek();
            Assert.AreEqual(1, valueStack.Count, "a value should be submitted to the ouput stream");
            Assert.AreEqual(1, outValue.Item1, "the out value should match the unmatched input");
            Assert.AreEqual(0, outValue.Item2, "the unmatched output should have no linked references");
        }

        [TestCategory(nameof(LeftJoinSubjectTests))]
        [TestMethod]
        public void RightCompletesBeforeLeftGetsMatches()
        {
            var valueStack = new Stack<Tuple<int, int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var leftS = new PushSubject<int>();
            var rightS = new PushSubject<int>();

            var output = leftS.LeftJoin(rightS, i => i, i => i, null, (l, r) => new Tuple<int, int>(l, r));

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            leftS.PushValue(1);

            Assert.AreEqual(0, valueStack.Count, "no value should be submitted to the ouput stream");

            rightS.Complete();

            var outValue = valueStack.Peek();
            Assert.AreEqual(1, valueStack.Count, "a value should be submitted to the ouput stream");
            Assert.AreEqual(1, outValue.Item1, "the out value should match the unmatched input");
            Assert.AreEqual(0, outValue.Item2, "the unmatched output should have no linked references");
            Assert.IsFalse(isComplete, "the output stream should not be completed");
        }

        [TestCategory(nameof(LeftJoinSubjectTests))]
        [TestMethod]
        public void SeveralLeftWaitMatches()
        {
            var valueStack = new Stack<Tuple<int, int>>();
            var errorStack = new Stack<Exception>();
            bool isComplete = false;
            var leftS = new PushSubject<int>();
            var rightS = new PushSubject<int>();

            var output = leftS.LeftJoin(rightS, i => i, i => i, null, (l, r) => new Tuple<int, int>(l, r));

            output.Subscribe(valueStack.Push, () => isComplete = true, errorStack.Push);

            leftS.PushValue(1);
            leftS.PushValue(1);

            Assert.AreEqual(0, valueStack.Count, "no value should be submitted to the ouput stream");

            rightS.PushValue(1);

            var outValue = valueStack.Pop();
            Assert.AreEqual(1, outValue.Item1, "the out value should match the unmatched input");
            outValue = valueStack.Pop();
            Assert.AreEqual(1, outValue.Item1, "the out value should match the unmatched input");
            Assert.IsFalse(isComplete, "the output stream should not be completed");
        }
    }
}
