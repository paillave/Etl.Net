using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ObservableType = System.Reactive.Linq.Observable;

namespace Paillave.Etl.Core.System
{
    public class Stream<T> : IStream<T>
    {
        public Stream(ExecutionContextBase context, IEnumerable<string> sourceNodeName, string sourceOutputName, IObservable<T> observable)
        {
            if (context != null)
            {
                this.Context = context;
                ObservableType.Merge<ProcessTrace>(
                    observable.Count().Select(count => new CounterSummaryProcessTrace(this.SourceNodeName, this.SourceOutputName, count)),
                    observable.Select((e, i) => new RowProcessTrace<T>(sourceNodeName, sourceOutputName, i + 1, e))//.Select(counter => new RowProcessTrace(sourceNodeName, sourceOutputName, counter + 1,))
                ).Subscribe(context.OnNextProcessTrace);
            }
            this.Observable = observable;
            this.SourceNodeName = sourceNodeName;
            this.SourceOutputName = sourceOutputName;
        }
        public ExecutionContextBase Context { get; private set; }
        public IEnumerable<string> SourceNodeName { get; private set; }
        public string SourceOutputName { get; private set; }
        public IObservable<T> Observable { get; private set; }
    }
}
