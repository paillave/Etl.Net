
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
            var inputList = Enumerable.Range(10, 10).Select(i => $".{i}").ToList();
            var outputList = new List<char>();

            StreamProcessRunner.Create<List<string>>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", i => i, true)
                    .CrossApply<string, char>("produce sub values", (input, pushValue) => input.ToList().ForEach(pushValue))
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(inputList).Wait();

            var expected = string.Join("", inputList);
            var actual = new string(outputList.ToArray());
            Assert.AreEqual(expected, actual);
            #endregion
        }
    }
}
