using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Extensions
{
    public static class KeepLastTracesPerNodeEx
    {
        public static ISingleStream<Dictionary<string, List<TraceEvent>>> KeepLastTracesPerNode(this IStream<TraceEvent> traceEventStream, int limit = 100)
        {
            return traceEventStream.Aggregate("aggregate traces per node type",
                i => new LimitedQueue<TraceEvent>(limit),
                i => i.NodeName,
                (a, e) =>
                {
                    a.Enqueue(e);
                    return a;
                })
                .Aggregate("Join all aggregations",
                    i => new Dictionary<string, List<TraceEvent>>(),
                    i => true,
                    (a, l) =>
                    {
                        a[l.Key] = l.Aggregation.ToList();
                        return a;
                    })
                .Select("create aggregation result", i => i.Aggregation)
                .First("get single aggregation result");
        }
    }
}
