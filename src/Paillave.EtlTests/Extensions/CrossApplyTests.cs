
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
        #region produce sub values without pre/post process
        private class TestValuesProvider : ValuesProviderBase<string, char>
        {
            public TestValuesProvider(bool noParallelisation) : base(noParallelisation)
            {
            }
            protected override void PushValues(string input, Action<char> pushValue)
            {
                input.ToList().ForEach(pushValue);
            }
        }

        // [TestCategory(nameof(CrossApplyTests))]
        // [TestMethod]
        // public void ProduceSubValues()
        // {
        //     var inputList = Enumerable.Range(10, 10).Select(i => $".{i}").ToList();
        //     var outputList = new List<char>();

        //     StreamProcessRunner.Create<List<string>>(rootStream =>
        //     {
        //         rootStream
        //             .CrossApplyEnumerable("list elements", i => i, true)
        //             .CrossApply("produce sub values", new TestValuesProvider(true))
        //             .ThroughAction("collect values", outputList.Add);
        //     }).ExecuteAsync(inputList).Wait();

        //     var expected = string.Join("", inputList);
        //     var actual = new string(outputList.ToArray());
        //     Assert.AreEqual(expected, actual);
        // }
        #endregion
    }
}
