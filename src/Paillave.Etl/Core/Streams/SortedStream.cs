using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.TraceContents;

namespace Paillave.Etl.Core.Streams
{
    public class SortedStream<T, TKey> : Stream<T>, ISortedStream<T, TKey>
    {
        public SortedStream(ITracer tracer, IExecutionContext executionContext, string sourceNodeName, IPushObservable<T> observable, SortDefinition<T, TKey> sortDefinition)
            : base(tracer, executionContext, sourceNodeName, observable)
        {
            if (sortDefinition == null) throw new ArgumentOutOfRangeException(nameof(sortDefinition), "sorting criteria list cannot be empty");
            this.SortDefinition = sortDefinition;
        }
        public SortDefinition<T, TKey> SortDefinition { get; }
        // public override object GetMatchingStream(IPushObservable<T> observable)
        // {
        //     return new SortedStream<T, TKey>(this.Tracer, this.ExecutionContext, this.SourceNodeName, observable, this.SortDefinition);
        // }
        public override object GetMatchingStream(ITracer tracer, IExecutionContext executionContext, string name, object observable)
        {
            return new SortedStream<T, TKey>(tracer, executionContext, name, (IPushObservable<T>)observable, this.SortDefinition);
        }
    }
}
