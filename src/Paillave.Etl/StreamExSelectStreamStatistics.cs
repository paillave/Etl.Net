using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static partial class StreamExSelectStreamStatistics
    {
        public static Task<StreamStatistics> GetStreamStatisticsAsync(this IStream<TraceEvent> input)
        {
            var errorsStatistics = input
                .Where("keep errors", i => i.Content.Level == System.Diagnostics.TraceLevel.Error)
                .Select("select errors caracteristics", i => new StreamStatisticError { NodeName = i.NodeName, Text = i.ToString() })
                .Observable.ToList();
            var streamStatistics = input
                .Where("keep stream results", i => i.Content is CounterSummaryStreamTraceContent)
                .Select("select statistic", i =>
             {
                 var content = (CounterSummaryStreamTraceContent)i.Content;
                 return new StreamStatisticCounter
                 {
                     Counter = content.Counter,
                     SourceNodeName = i.NodeName
                 };
             }).Observable.ToList();
            return streamStatistics
                .CombineWithLatest(errorsStatistics, (s, e) => new StreamStatistics
                {
                    StreamStatisticErrors = e,
                    StreamStatisticCounters = s
                })
                .ToTaskAsync();
        }
    }
}
