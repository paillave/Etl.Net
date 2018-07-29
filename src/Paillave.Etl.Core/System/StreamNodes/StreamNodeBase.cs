using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.System.Streams;

namespace Paillave.Etl.Core.System
{
    public abstract class StreamNodeBase<TStream, TIn> : StreamNodeBase where TStream : IStream<TIn>
    {
        public TStream Input { get; private set; }
        public StreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath)
            : base(input.ExecutionContext, name, parentNodeNamePath)
        {
            this.Input = input;
        }
    }
    public abstract class StreamNodeBase<TStream, TIn, TArgs> : StreamNodeBase<TStream, TIn> where TStream : IStream<TIn>
    {
        public TArgs Arguments { get; private set; }
        public StreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath, TArgs arguments)
            : base(input, name, parentNodeNamePath)
        {
            this.Arguments = arguments;
        }
    }
    public abstract class StreamNodeBase : INodeContext
    {
        private IExecutionContext _executionContext;
        public StreamNodeBase(IExecutionContext executionContext, string name, IEnumerable<string> parentNodeNamePath)
        {
            this._executionContext = executionContext;
            this.TypeName = this.GetType().Name;
            this.NodeNamePath = (parentNodeNamePath ?? new string[] { }).Concat(new[] { name }).ToArray();
            this.Tracer = new Tracer(executionContext, this);
        }

        public IEnumerable<string> NodeNamePath { get; set; }
        public virtual string TypeName { get; private set; }

        protected ITracer Tracer { get; private set; }

        protected IStream<T> CreateStream<T>(string streamName, IPushObservable<T> observable)
        {
            var stream = new Stream<T>(this.Tracer, this._executionContext, streamName, observable);
            //this._executionContext.AddObservableToWait(stream.Observable);
            return stream;
        }

        protected IPushObservable<TOut> CreateObservable<TIn, TOut>(TIn input, Action<TIn, Action<TOut>> populateObserver)
        {
            return new DeferedPushObservable<TOut>(i => populateObserver(input, i), true);
            //var subject = new PushSubject<TOut>();
            //Task.Run(() => populateObserver(input, subject));
            //return subject;
        }

        protected ISortedStream<T> CreateSortedStream<T>(string streamName, IPushObservable<T> observable, IEnumerable<ISortCriteria<T>> sortCriterias)
        {
            var stream = new SortedStream<T>(this.Tracer, this._executionContext, streamName, observable, sortCriterias);
            //this._executionContext.AddObservableToWait(stream.Observable);
            return stream;
        }
        protected ISortedStream<T> CreateStream<T>(string streamName, IPushObservable<T> observable, ISortedStream<T> streamIn)
        {
            var stream = new SortedStream<T>(this.Tracer, this._executionContext, streamName, observable, streamIn.SortCriterias);
            //this._executionContext.AddObservableToWait(stream.Observable);
            return stream;
        }
        protected IKeyedStream<T> CreateKeyedStream<T>(string streamName, IPushObservable<T> observable, IEnumerable<ISortCriteria<T>> sortCriterias)
        {
            var stream = new KeyedStream<T>(this.Tracer, this._executionContext, streamName, observable, sortCriterias);
            //this._executionContext.AddObservableToWait(stream.Observable);
            return stream;
        }
        protected IKeyedStream<T> CreateStream<T>(string streamName, IPushObservable<T> observable, IKeyedStream<T> streamIn)
        {
            var stream = new KeyedStream<T>(this.Tracer, this._executionContext, streamName, observable, streamIn.SortCriterias);
            //this._executionContext.AddObservableToWait(stream.Observable);
            return stream;
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
