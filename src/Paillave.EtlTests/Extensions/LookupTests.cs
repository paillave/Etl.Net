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
    public class LookupTests
    {
        [TestCategory(nameof(LookupTests))]
        [TestMethod]
        public void SimpleLookup()
        {
            #region simple lookup
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = Enumerable.Range(0, 10).Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            StreamProcessRunner.CreateAndExecuteAsync(new
            {
                InputLeftList = inputLeftList,
                InputRightList = inputRightList
            }, rootStream =>
            {
                var leftStream = rootStream.CrossApplyEnumerable("input left elements", config => config.InputLeftList);
                var rightStream = rootStream.CrossApplyEnumerable("input right elements", config => config.InputRightList);
                leftStream
                    .Lookup("join left and right", rightStream, i => i.ForeignId, i => i.Id, (left, right) => $"{left.Id}-{right.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expected = Enumerable.Range(0, 100).Select(i => $"{i}-Label{i / 10}").ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }

        [TestCategory(nameof(LookupTests))]
        [TestMethod]
        public void LookupWithNoMatch()
        {
            #region lookup with no match
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = Enumerable.Range(1000, 10).Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            //TODO: Ensure there is an actual failure if the result expression fails
            StreamProcessRunner.CreateAndExecuteAsync(new
            {
                InputLeftList = inputLeftList,
                InputRightList = inputRightList
            }, rootStream =>
            {
                var leftStream = rootStream.CrossApplyEnumerable("input left elements", config => config.InputLeftList);
                var rightStream = rootStream.CrossApplyEnumerable("input right elements", config => config.InputRightList);
                leftStream
                    .Lookup("join left and right", rightStream, i => i.ForeignId, i => i.Id, (left, right) => $"{left.Id}-{right?.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expected = Enumerable.Range(0, 100).Select(i => $"{i}-").ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }

        [TestCategory(nameof(LookupTests))]
        [TestMethod]
        public void LookupWithSomeMatches()
        {
            #region lookup with couple of no match
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = new[] { 2, 5 }.Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            //TODO: Ensure there is an actual failure if the result expression fails
            StreamProcessRunner.CreateAndExecuteAsync(new
            {
                InputLeftList = inputLeftList,
                InputRightList = inputRightList
            }, rootStream =>
            {
                var leftStream = rootStream.CrossApplyEnumerable("input left elements", config => config.InputLeftList);
                var rightStream = rootStream.CrossApplyEnumerable("input right elements", config => config.InputRightList);
                leftStream
                    .Lookup("join left and right", rightStream, i => i.ForeignId, i => i.Id, (left, right) => $"{left.Id}-{right?.Label}")
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
