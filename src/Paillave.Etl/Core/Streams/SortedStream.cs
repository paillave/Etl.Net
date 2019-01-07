using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Core.TraceContents;

namespace Paillave.Etl.Core.Streams
{
    public class SortedStream<T, TKey> : Stream<T>, ISortedStream<T, TKey>
    {
        public SortedStream(ITraceMapper tracer, IExecutionContext executionContext, string sourceNodeName, IPushObservable<T> observable, SortDefinition<T, TKey> sortDefinition)
            : base(tracer, executionContext, sourceNodeName, observable)
        {
            if (sortDefinition == null) throw new ArgumentOutOfRangeException(nameof(sortDefinition), "sorting criteria list cannot be empty");
            this.SortDefinition = sortDefinition;
        }
        public SortDefinition<T, TKey> SortDefinition { get; }

        public override object GetMatchingStream(ITraceMapper tracer, IExecutionContext executionContext, string name, object observable)
        {
            return new SortedStream<T, TKey>(tracer, executionContext, name, (IPushObservable<T>)observable, this.SortDefinition);
        }
    }
}
