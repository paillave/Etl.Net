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
        public void SimpleLeftJoin()
        {
            #region simple left join
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = Enumerable.Range(0, 10).Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            StreamProcessRunner.Create<object>(rootStream =>
            {
                var leftStream = rootStream.CrossApplyEnumerable("input left elements", _ => inputLeftList);
                var rightStream = rootStream.CrossApplyEnumerable("input right elements", _ => inputRightList);
                leftStream
                    .Lookup("join left and right", rightStream, i => i.ForeignId, i => i.Id, (left, right) => $"{left.Id}-{right.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

            var expected = Enumerable.Range(0, 100).Select(i => $"{i}-Label{i / 10}").ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }

        [TestCategory(nameof(LookupTests))]
        [TestMethod]
        public void LeftJoinWithNoMatch()
        {
            #region simple left join
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = Enumerable.Range(1000, 10).Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            //TODO: Ensure there is an actual failure if the result expression fails
            StreamProcessRunner.Create<object>(rootStream =>
            {
                var leftStream = rootStream.CrossApplyEnumerable("input left elements", _ => inputLeftList);
                var rightStream = rootStream.CrossApplyEnumerable("input right elements", _ => inputRightList);
                leftStream
                    .Lookup("join left and right", rightStream, i => i.ForeignId, i => i.Id, (left, right) => $"{left.Id}-{right?.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

            var expected = Enumerable.Range(0, 100).Select(i => $"{i}-").ToList();
            CollectionAssert.AreEquivalent(expected, outputList);
            #endregion
        }

        [TestCategory(nameof(LookupTests))]
        [TestMethod]
        public void LeftJoinWithSomeMatches()
        {
            #region simple left join
            var inputLeftList = Enumerable.Range(0, 100).Select(i => new { Id = i, ForeignId = i / 10 }).ToList();
            var inputRightList = new[] { 2, 5 }.Select(i => new { Id = i, Label = $"Label{i}" }).ToList();
            var outputList = new List<string>();

            //TODO: Ensure there is an actual failure if the result expression fails
            StreamProcessRunner.Create<object>(rootStream =>
            {
                var leftStream = rootStream.CrossApplyEnumerable("input left elements", _ => inputLeftList);
                var rightStream = rootStream.CrossApplyEnumerable("input right elements", _ => inputRightList);
                leftStream
                    .Lookup("join left and right", rightStream, i => i.ForeignId, i => i.Id, (left, right) => $"{left.Id}-{right?.Label}")
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

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
