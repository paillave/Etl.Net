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
    public class Stream<T>
    {
        public Stream(ExecutionContextBase traceContext, IEnumerable<string> sourceNodeName, string sourceOutputName, IObservable<T> observable)
        {
            if (traceContext != null)
            {
                this.Context = traceContext;
                ObservableType.Merge<ProcessTrace>(
                    observable.Count().Select(count => new CounterSummaryProcessTrace(this.SourceNodeName, this.SourceOutputName, count)),
                    observable.Select((e, i) => i).Select(counter => new CounterProcessTrace(sourceNodeName, sourceOutputName, counter + 1))
                ).Subscribe(traceContext.OnNextProcessTrace);
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
