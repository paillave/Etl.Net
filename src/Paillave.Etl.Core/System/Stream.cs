using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ObservableType = System.Reactive.Linq.Observable;

namespace Paillave.Etl.Core.System
{
    public class Stream<T> : IStream<T>
    {
        public Stream(ITracer tracer, IExecutionContext executionContext, string sourceOutputName, IObservable<T> observable)
        {
            this.ExecutionContext = executionContext;

            observable = observable.Catch<T, Exception>(e =>
              {
                  tracer.Trace(new UnhandledExceptionStreamTraceContent(sourceOutputName, e));
                  return ObservableType.Empty<T>();
              });
            observable = observable.Publish().RefCount();
            executionContext.AddObservableToWait(observable);
            if (tracer != null)
            {
                ObservableType.Merge<ITraceContent>(
                    observable.Count().Select(count => new CounterSummaryStreamTraceContent(sourceOutputName, count)),
                    observable.Select((e, i) => new RowProcessStreamTraceContent(sourceOutputName, i + 1, e))
                ).Subscribe(tracer.Trace);
            }
            this.Observable = observable;
        }

        public IObservable<T> Observable { get; private set; }

        public IExecutionContext ExecutionContext { get; }
    }
}
