using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests.Extensions
{
    [TestClass()]
    public class PivotTests
    {
        [TestCategory(nameof(PivotTests))]
        [TestMethod]
        public void EmptyPivot()
        {
            var inputList = new int[] { }.Select(i => new { Key = i / 5, Value = i }).ToList();
            var outputList = new List<string>();

            #region simple pivot
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Pivot("simple count", i => i.Key, i => new
                    {
                        Id = i.Key,
                        Count = AggregationOperators.Count()
                    })
                    .Select("transform into string", SimpleSerializeToString)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expectedList = new string[] { }.ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }
        [TestCategory(nameof(PivotTests))]
        [TestMethod]
        public void SimplePivot()
        {
            var inputList = Enumerable.Range(0, 10).Select(i => new { Key = i / 5, Value = i }).ToList();
            var outputList = new List<string>();

            #region simple pivot
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Pivot("simple count", i => i.Key, i => new
                    {
                        Id = i.Key,
                        Count = AggregationOperators.Count()
                    })
                    .Select("transform into string", SimpleSerializeToString)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expectedList = new[] {
                "FirstValue:{ Key = 0, Value = 0 },Key:0,Aggregation:{ Id = 0, Count = 5 }",
                "FirstValue:{ Key = 1, Value = 5 },Key:1,Aggregation:{ Id = 0, Count = 5 }"
            }.ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }
        [TestCategory(nameof(PivotTests))]
        [TestMethod]
        public void MultiColumnsPivot()
        {
            var inputList = Enumerable.Range(0, 10).Select(i => new { Key = i / 5, Value = i, Col = i % 2 }).ToList();
            var outputList = new List<string>();

            #region multicolumns pivot
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Pivot("simple count", i => i.Key, i => new
                    {
                        Id = i.Key,
                        EvensCount = AggregationOperators.Count().For(i.Col == 0),
                        OddsCount = AggregationOperators.Count().For(i.Col == 1),
                    })
                    .Select("transform into string", SimpleSerializeToString)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expectedList = new[] {
                "FirstValue:{ Key = 0, Value = 0, Col = 0 },Key:0,Aggregation:{ Id = 0, EvensCount = 3, OddsCount = 2 }",
                "FirstValue:{ Key = 1, Value = 5, Col = 1 },Key:1,Aggregation:{ Id = 0, EvensCount = 2, OddsCount = 3 }"
            }.ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }
        [TestCategory(nameof(PivotTests))]
        [TestMethod]
        public void MultiColumnsPivotWithNoValue()
        {
            var inputList = Enumerable.Range(0, 10).Select(i => new { Key = i / 5, Value = i, Col = i % 2 }).ToList();
            var outputList = new List<string>();

            #region multicolumns pivot
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Pivot("simple count", i => i.Key, i => new
                    {
                        Id = i.Key,
                        EvensCount = AggregationOperators.Count().For(i.Col == 2),
                        OddsCount = AggregationOperators.Count().For(i.Col == 3),
                    })
                    .Select("transform into string", SimpleSerializeToString)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expectedList = new[] {
                "FirstValue:{ Key = 0, Value = 0, Col = 0 },Key:0,Aggregation:{ Id = 0, EvensCount = 0, OddsCount = 0 }",
                "FirstValue:{ Key = 1, Value = 5, Col = 1 },Key:1,Aggregation:{ Id = 0, EvensCount = 0, OddsCount = 0 }"
            }.ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }
        [TestCategory(nameof(PivotTests))]
        [TestMethod]
        public void MultiComputationsPivot()
        {
            var inputList = Enumerable.Range(0, 10).Select(i => new { Key = i / 5, Value = i }).ToList();
            var outputList = new List<string>();

            #region multicomputations pivot
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Pivot("simple count", i => i.Key, i => new
                    {
                        Id = i.Key,
                        Count = AggregationOperators.Count(),
                        Sum = AggregationOperators.Sum(i.Value),
                        First = AggregationOperators.First(i.Value),
                        Last = AggregationOperators.Last(i.Value),
                        Max = AggregationOperators.Max(i.Value),
                        Min = AggregationOperators.Min(i.Value),
                        Avg = AggregationOperators.Avg(i.Value),
                    })
                    .Select("transform into string", SimpleSerializeToString)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expectedList = new[] {
                "FirstValue:{ Key = 0, Value = 0 },Key:0,Aggregation:{ Id = 0, Count = 5, Sum = 10, First = 0, Last = 4, Max = 4, Min = 0, Avg = 2 }",
                "FirstValue:{ Key = 1, Value = 5 },Key:1,Aggregation:{ Id = 0, Count = 5, Sum = 35, First = 5, Last = 9, Max = 9, Min = 5, Avg = 7 }"
            }.ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }
        [TestCategory(nameof(PivotTests))]
        [TestMethod]
        public void MultiColumnsMultiComputationsPivot()
        {
            var inputList = Enumerable.Range(0, 10).Select(i => new { Key = i / 5, Value = i, Col = i % 2 }).ToList();
            var outputList = new List<string>();

            #region multicolumns multicomputations pivot
            StreamProcessRunner.CreateAndExecuteAsync(inputList, rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", config => config)
                    .Pivot("simple count", i => i.Key, i => new
                    {
                        Id = i.Key,
                        EvensCount = AggregationOperators.Count().For(i.Col == 0),
                        EvensSum = AggregationOperators.Sum(i.Value).For(i.Col == 0),
                        EvensFirst = AggregationOperators.First(i.Value).For(i.Col == 0),
                        EvensLast = AggregationOperators.Last(i.Value).For(i.Col == 0),
                        EvensMax = AggregationOperators.Max(i.Value).For(i.Col == 0),
                        EvensMin = AggregationOperators.Min(i.Value).For(i.Col == 0),
                        EvensAvg = AggregationOperators.Avg(i.Value).For(i.Col == 0),
                        OddsCount = AggregationOperators.Count().For(i.Col == 1),
                        OddsSum = AggregationOperators.Sum(i.Value).For(i.Col == 1),
                        OddsFirst = AggregationOperators.First(i.Value).For(i.Col == 1),
                        OddsLast = AggregationOperators.Last(i.Value).For(i.Col == 1),
                        OddsMax = AggregationOperators.Max(i.Value).For(i.Col == 1),
                        OddsMin = AggregationOperators.Min(i.Value).For(i.Col == 1),
                        OddsAvg = AggregationOperators.Avg(i.Value).For(i.Col == 1),
                    })
                    .Select("transform into string", SimpleSerializeToString)
                    .ThroughAction("collect values", outputList.Add);
            }).Wait();

            var expectedList = new[] {
                "FirstValue:{ Key = 0, Value = 0, Col = 0 },Key:0,Aggregation:{ Id = 0, EvensCount = 3, EvensSum = 6, EvensFirst = 0, EvensLast = 4, EvensMax = 4, EvensMin = 0, EvensAvg = 2, OddsCount = 2, OddsSum = 4, OddsFirst = 1, OddsLast = 3, OddsMax = 3, OddsMin = 1, OddsAvg = 2 }",
                "FirstValue:{ Key = 1, Value = 5, Col = 1 },Key:1,Aggregation:{ Id = 0, EvensCount = 2, EvensSum = 14, EvensFirst = 6, EvensLast = 8, EvensMax = 8, EvensMin = 6, EvensAvg = 7, OddsCount = 3, OddsSum = 21, OddsFirst = 5, OddsLast = 9, OddsMax = 9, OddsMin = 5, OddsAvg = 7 }"
            }.ToList();
            CollectionAssert.AreEquivalent(expectedList, outputList);
            #endregion
        }
        private string SimpleSerializeToString<T>(T input)
        {
            return string.Join(',', typeof(T).GetProperties().Select(i => $"{i.Name}:{i.GetValue(input)}").ToArray());
        }
    }
}