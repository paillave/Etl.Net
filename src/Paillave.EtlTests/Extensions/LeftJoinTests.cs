using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests.Extensions
{
    [TestClass()]
    public class LeftJoinTests
    {
        [TestCategory(nameof(LeftJoinTests))]
        [TestMethod]
        public void SimpleLeftJoin()
        {
            #region simple left join
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = Enumerable.Range(0, 10).Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            StreamProcessRunner.CreateAndExecuteAsync(new
            {
                InputLeftList = inputLeftList,
                InputRightList = inputRightList
            }, rootStream =>
            {
                var leftStream = rootStream
                    .CrossApplyEnumerable("input left elements", config => config.InputLeftList)
                    .EnsureSorted("ensure left is sorted", i => i.ForeignId);
                var rightStream = rootStream
                    .CrossApplyEnumerable("input right elements", config => config.InputRightList)
                    .EnsureKeyed("ensure right is keyed", i => i.Id);
                leftStream
                    .LeftJoin("join left and right", rightStream, (left, right) => $"{left.Id}-{right.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expected = Enumerable.Range(0, 100).Select(i => $"{i}-Label{i / 10}").ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }

        [TestCategory(nameof(LeftJoinTests))]
        [TestMethod]
        public void LeftJoinWithNoMatch()
        {
            #region left join with no matches
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = Enumerable.Range(1000, 10).Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            StreamProcessRunner.CreateAndExecuteAsync(new
            {
                InputLeftList = inputLeftList,
                InputRightList = inputRightList
            }, rootStream =>
            {
                var leftStream = rootStream
                    .CrossApplyEnumerable("input left elements", config => config.InputLeftList)
                    .EnsureSorted("ensure left is sorted", i => i.ForeignId);
                var rightStream = rootStream
                    .CrossApplyEnumerable("input right elements", config => config.InputRightList)
                    .EnsureKeyed("ensure right is keyed", i => i.Id);
                leftStream
                    .LeftJoin("join left and right", rightStream, (left, right) => $"{left.Id}-{right?.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expected = Enumerable.Range(0, 100).Select(i => $"{i}-").ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }

        [TestCategory(nameof(LeftJoinTests))]
        [TestMethod]
        public void LeftJoinWithSomeMatches()
        {
            #region simple left join with couple of matches
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = new[] { 2, 5 }.Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            StreamProcessRunner.CreateAndExecuteAsync(new
            {
                InputLeftList = inputLeftList,
                InputRightList = inputRightList
            }, rootStream =>
            {
                var leftStream = rootStream
                    .CrossApplyEnumerable("input left elements", config => config.InputLeftList)
                    .EnsureSorted("ensure left is sorted", i => i.ForeignId);
                var rightStream = rootStream
                    .CrossApplyEnumerable("input right elements", config => config.InputRightList)
                    .EnsureKeyed("ensure right is keyed", i => i.Id);
                leftStream
                    .LeftJoin("join left and right", rightStream, (left, right) => $"{left.Id}-{right?.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expected = Enumerable.Range(0, 100).Select(i =>
            {
                if (new[] { 2, 5 }.Contains(i / 10))
                    return $"{i}-Label{i / 10}";
                else
                    return $"{i}-";
            }).ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }
    }
}
