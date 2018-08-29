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
    public class KeyedStream<T, TKey> : Stream<T>, IKeyedStream<T, TKey>
    {
        public KeyedStream(ITracer tracer, IExecutionContext executionContext, string sourceNodeName, IPushObservable<T> observable, SortDefinition<T, TKey> sortDefinition) : base(tracer, executionContext, sourceNodeName, observable)
        {
            if (sortDefinition == null) throw new ArgumentOutOfRangeException(nameof(sortDefinition), "key criteria list cannot be empty");
            this.SortDefinition = sortDefinition;
        }
        public SortDefinition<T, TKey> SortDefinition { get; }
    }
}
