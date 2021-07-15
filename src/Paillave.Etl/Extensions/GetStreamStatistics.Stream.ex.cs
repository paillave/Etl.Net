using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    public static partial class GetStreamStatisticsEx
    {
        public static Task<List<StreamStatisticCounter>> GetStreamStatisticsAsync(this IStream<TraceEvent> input)
            => input
                .Where("keep stream results", i => i.Content is CounterSummaryStreamTraceContent)
                .Select("select statistic", i =>
             {
                 var content = (CounterSummaryStreamTraceContent)i.Content;
                 return new StreamStatisticCounter
                 {
                     Counter = content.Counter,
                     SourceNodeName = i.NodeName
                 };
             }).Observable.ToList().ToTaskAsync();
    }
}
