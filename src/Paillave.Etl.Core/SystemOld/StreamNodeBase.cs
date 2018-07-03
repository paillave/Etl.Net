using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.SystemOld
{
    public abstract class StreamNodeBase
    {
        public StreamNodeBase(ITracer tracer, string name, IEnumerable<string> parentNodeNamePath = null)
        {
            this.Tracer = tracer;
            this.NodeNamePath = (parentNodeNamePath ?? new string[] { }).Concat(new[] { name }).ToArray();
        }
        public IEnumerable<string> NodeNamePath { get; private set; }
        protected ITracer Tracer { get; private set; }
        protected abstract string TypeName { get; }
        protected IStream<T> CreateStream<T>(string streamName, IObservable<T> observable)
        {
            return new Stream<T>(this.Tracer, this.NodeNamePath, streamName, observable);
        }
        protected ISortedStream<T> CreateSortedStream<T>(string streamName, IObservable<T> observable, IEnumerable<SortCriteria<T>> sortCriterias)
        {
            return new SortedStream<T>(this.Tracer, this.NodeNamePath, streamName, observable, sortCriterias);
        }
        protected IKeyedStream<T> CreateKeyedStream<T>(string streamName, IObservable<T> observable, IEnumerable<SortCriteria<T>> sortCriterias)
        {
            return new KeyedStream<T>(this.Tracer, this.NodeNamePath, streamName, observable, sortCriterias);
        }
    }
}
