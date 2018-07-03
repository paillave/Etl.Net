using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ObservableType = System.Reactive.Linq.Observable;

namespace Paillave.Etl.Core.SystemOld
{
    public class Stream<T> : IStream<T>
    {
        public Stream(ITracer tracer, IEnumerable<string> sourceNodeName, string sourceOutputName, IObservable<T> observable)
        {
            if (tracer != null)
            {
                this.Tracer = tracer;
                ObservableType.Merge<ProcessTrace>(
                    observable.Count().Select(count => new CounterSummaryProcessTrace(this.SourceNodeName, this.SourceOutputName, count)),
                    observable.Select((e, i) => new RowProcessTrace<T>(sourceNodeName, sourceOutputName, i + 1, e))//.Select(counter => new RowProcessTrace(sourceNodeName, sourceOutputName, counter + 1,))
                ).Subscribe(tracer.OnNextProcessTrace);
            }
            this.Observable = observable;
            this.SourceNodeName = sourceNodeName;
            this.SourceOutputName = sourceOutputName;
        }
        public ITracer Tracer { get; private set; }
        public IEnumerable<string> SourceNodeName { get; private set; }
        public string SourceOutputName { get; private set; }
        public IObservable<T> Observable { get; private set; }
    }
}
