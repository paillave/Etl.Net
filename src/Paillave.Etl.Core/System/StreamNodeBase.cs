using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.System
{
    public abstract class StreamNodeBase : INodeContext
    {
        private IExecutionContext _executionContext;
        public void Initialize(IExecutionContext executionContext, string name, IEnumerable<string> parentNodeNamePath = null)
        {
            this._executionContext = executionContext;
            this.TypeName = this.GetType().Name;
            this.NodeNamePath = (parentNodeNamePath ?? new string[] { }).Concat(new[] { name }).ToArray();
            this.Tracer = new Tracer(executionContext, this);
        }

        public IEnumerable<string> NodeNamePath { get; set; }
        public virtual string TypeName { get; private set; }

        protected ITracer Tracer { get; private set; }

        protected IStream<T> CreateStream<T>(string streamName, IObservable<T> observable)
        {
            return new Stream<T>(this.Tracer, this._executionContext, streamName, observable);
        }
        protected ISortedStream<T> CreateSortedStream<T>(string streamName, IObservable<T> observable, IEnumerable<SortCriteria<T>> sortCriterias)
        {
            return new SortedStream<T>(this.Tracer, this._executionContext, streamName, observable, sortCriterias);
        }
        protected IKeyedStream<T> CreateKeyedStream<T>(string streamName, IObservable<T> observable, IEnumerable<SortCriteria<T>> sortCriterias)
        {
            return new KeyedStream<T>(this.Tracer, this._executionContext, streamName, observable, sortCriterias);
        }
        protected Func<TIn, ErrorManagementItem<TIn, TOut>> ErrorManagementWrapFunction<TIn, TOut>(Func<TIn, TOut> call)
        {
            return (TIn input) =>
            {
                try
                {
                    return new ErrorManagementItem<TIn, TOut>(input, call(input));
                }
                catch (Exception ex)
                {
                    return new ErrorManagementItem<TIn, TOut>(input, ex);
                }
            };
        }
        protected Func<TIn1, TIn2, ErrorManagementItem<TIn1, TIn2, TOut>> ErrorManagementWrapFunction<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> call)
        {
            return (TIn1 input1, TIn2 input2) =>
            {
                try
                {
                    return new ErrorManagementItem<TIn1, TIn2, TOut>(input1, input2, call(input1, input2));
                }
                catch (Exception ex)
                {
                    return new ErrorManagementItem<TIn1, TIn2, TOut>(input1, input2, ex);
                }
            };
        }
    }
}
