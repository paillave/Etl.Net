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
    public class KeyedStream<T, TKey> : Stream<T>, IKeyedStream<T, TKey>
    {
        public KeyedStream(ITraceMapper tracer, IExecutionContext executionContext, string sourceNodeName, IPushObservable<T> observable, SortDefinition<T, TKey> sortDefinition) : base(tracer, executionContext, sourceNodeName, observable)
        {
            this.SortDefinition = sortDefinition ?? throw new ArgumentOutOfRangeException(nameof(sortDefinition), "key criteria list cannot be empty");
        }
        public SortDefinition<T, TKey> SortDefinition { get; }
        public override object GetMatchingStream(ITraceMapper tracer, IExecutionContext executionContext, string name, object observable)
        {
            return new KeyedStream<T, TKey>(tracer, executionContext, name, (IPushObservable<T>)observable, this.SortDefinition);
        }
    }
}
