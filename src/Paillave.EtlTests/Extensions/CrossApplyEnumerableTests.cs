using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.EtlTests.Extensions
{
    [TestClass()]
    public class CrossApplyEnumerableTests
    {
        [TestCategory(nameof(CrossApplyEnumerableTests))]
        [TestMethod]
        public void ProduceList()
        {
            var inputList = Enumerable.Range(0, 10).ToList();
            var outputList = new List<int>();

            StreamProcessRunner.Create<object>(rootStream =>
            {
                rootStream
                    .CrossApplyEnumerable("list elements", _ => inputList)
                    .ThroughAction("collect values", outputList.Add);
            }).ExecuteAsync(null).Wait();

            CollectionAssert.AreEquivalent(inputList, outputList);
        }
    }
}
