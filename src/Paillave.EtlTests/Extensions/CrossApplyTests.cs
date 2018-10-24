using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Core;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests.Extensions
{
    [TestClass()]
    public class CrossApplyTests
    {

        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValues()
        {
            #region produce sub values without pre/post process
            var inputList = Enumerable.Range(10, 10).Select(i => $"{i}").ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<List<string>>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", i => i, true)
                    .CrossApply<string, string>("produce sub values", (input, pushValue) => input.Select(i => $"{i}").ToList().ForEach(pushValue))
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(inputList).Wait();

            var expected = inputList.SelectMany(i => i.Select(j => $"{j}")).ToList();
            var actual = outputList;
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }

        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValuesWithToApply()
        {
            #region produce sub values without pre/post process with to apply
            var inputList = Enumerable.Range(10, 100).Select(i => $"{i}").ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<Tuple<List<string>, int>>(rootStream =>
            {
                var toApplyStream = rootStream.Select("get multiplier", i => i.Item2);
                rootStream
                    .CrossApplyEnumerable("list elements", i => i.Item1, true)
                    .CrossApply<string, int, string>("produce sub values", toApplyStream, (input, toApply, pushValue) => input.Select(i => $"{i}*{toApply}").ToList().ForEach(pushValue))
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(new Tuple<List<string>, int>(inputList, 2)).Wait();

            var expected = inputList.SelectMany(i => i.Select(j => $"{j}*2")).ToList();
            var actual = outputList;
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }

        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValuesParallelized()
        {
            #region parallel produce sub values without pre/post process
            var inputList = Enumerable.Range(10, 10).Select(i => Enumerable.Range(i, 4).Select(j => $"{j}").ToList()).ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<List<List<string>>>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable<List<List<string>>, List<string>>("list elements", i => i)
                    .CrossApply<List<string>, string>("produce sub values", (input, pushValue) => input.ForEach(pushValue))
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(inputList).Wait();

            var expected = inputList.SelectMany(i => i).OrderBy(i => i).ToList();
            var actual = outputList.OrderBy(i => i).ToList();
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }

        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValuesWithToApplyParallelized()
        {
            #region produce sub values without pre/post process with to apply parallelized
            var inputList = Enumerable.Range(10, 100).Select(i => $"{i}").ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<Tuple<List<string>, int>>(rootStream =>
            {
                var toApplyStream = rootStream.Select("get multiplier", i => i.Item2);
                rootStream
                    .CrossApplyEnumerable("list elements", i => i.Item1)
                    .CrossApply<string, int, string>("produce sub values", toApplyStream, (input, toApply, pushValue) => input.Select(i => $"{i}*{toApply}").ToList().ForEach(pushValue))
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(new Tuple<List<string>, int>(inputList, 2)).Wait();

            var expected = inputList.SelectMany(i => i.Select(j => $"{j}*2")).OrderBy(i => i).ToList();
            var actual = outputList.OrderBy(i => i).ToList();
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }












        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValuesPrePost()
        {
            #region produce sub values without pre/post process PrePost
            var inputList = Enumerable.Range(10, 10).Select(i => $"{i}").ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<List<string>>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", i => i, true)
                    .CrossApply<string, int, int, string>("produce sub values", (input, pushValue) => input.ToString().Select(i => int.Parse(i.ToString())).ToList().ForEach(pushValue), i => int.Parse(i), (i, j) => $"{i}-{j}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(inputList).Wait();

            var expected = inputList.SelectMany(i => i.Select(j => $"{j}-{i}")).ToList();
            var actual = outputList;
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }

        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValuesWithToApplyPrePost()
        {
            #region produce sub values without pre/post process with to apply PrePost
            var inputList = Enumerable.Range(10, 100).Select(i => $"{i}").ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<Tuple<List<string>, int>>(rootStream =>
            {
                var toApplyStream = rootStream.Select("get multiplier", i => i.Item2);
                rootStream
                    .CrossApplyEnumerable("list elements", i => i.Item1, true)
                    .CrossApply<string,int,int, int, string>("produce sub values", toApplyStream, (input, toApply, pushValue) => input.ToString().Select(i => int.Parse(i.ToString())*toApply).ToList().ForEach(pushValue), (i,j)=>int.Parse(i)+2, (val,initVal,toApply)=> $"{val/toApply}-{initVal}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(new Tuple<List<string>, int>(inputList, 2)).Wait();

            var expected = inputList.SelectMany(i => (int.Parse(i)+2).ToString().Select(j => int.Parse(j.ToString())*2).Select(j=>$"{j/2}-{i}").ToList()).ToList();
            var actual = outputList;
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }

        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValuesPrePostParallelized()
        {
            #region produce sub values without pre/post process PrePost Parallelized
            var inputList = Enumerable.Range(10, 10).Select(i => $"{i}").ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<List<string>>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", i => i)
                    .CrossApply<string, int, int, string>("produce sub values", (input, pushValue) => input.ToString().Select(i => int.Parse(i.ToString())).ToList().ForEach(pushValue), i => int.Parse(i), (i, j) => $"{i}-{j}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(inputList).Wait();

            var expected = inputList.SelectMany(i => i.Select(j => $"{j}-{i}")).OrderBy(i=>i).ToList();
            var actual = outputList.OrderBy(i=>i).ToList();
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }

        [TestCategory(nameof(CrossApplyTests))]
        [TestMethod]
        public void ProduceSubValuesWithToApplyPrePostParallelized()
        {
            #region produce sub values without pre/post process with to apply PrePost Parallelized
            var inputList = Enumerable.Range(10, 100).Select(i => $"{i}").ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<Tuple<List<string>, int>>(rootStream =>
            {
                var toApplyStream = rootStream.Select("get multiplier", i => i.Item2);
                rootStream
                    .CrossApplyEnumerable("list elements", i => i.Item1)
                    .CrossApply<string,int,int, int, string>("produce sub values", toApplyStream, (input, toApply, pushValue) => input.ToString().Select(i => int.Parse(i.ToString())*toApply).ToList().ForEach(pushValue), (i,j)=>int.Parse(i)+2, (val,initVal,toApply)=> $"{val/toApply}-{initVal}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(new Tuple<List<string>, int>(inputList, 2)).Wait();

            var expected = inputList.SelectMany(i => (int.Parse(i)+2).ToString().Select(j => int.Parse(j.ToString())*2).Select(j=>$"{j/2}-{i}").ToList()).OrderBy(i=>i).ToList();
            var actual = outputList.OrderBy(i=>i).ToList();
            CollectionAssert.AreEquivalent(expected, actual);
            #endregion
        }
    }
}
