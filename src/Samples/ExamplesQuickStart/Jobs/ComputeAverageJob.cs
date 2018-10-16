using Paillave.Etl;
using Paillave.Etl.Extensions;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.StreamNodes;
using System.Linq;

namespace ExamplesQuickStart.Jobs
{
    public class ComputeAverageJob : IStreamProcessDefinition<object>
    {
        public string Name => "import file";
        public void DefineProcess(ISingleStream<object> rootStream)
        {
            rootStream
                .CrossApplyEnumerable("create some values", (input) => Enumerable.Range(0, 10))
                .Aggregate("aggregate values for average computation",
                    i => new { sum = 0, nb = 0 },
                    i => i % 3,
                    (previousAggr, value) => new { sum = previousAggr.sum + value, nb = previousAggr.nb + 1 })
                .Select("compute average", i => new { i.Key, Avg = i.Aggregation.sum / i.Aggregation.nb });
        }
    }
}
