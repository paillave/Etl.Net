using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System.Diagnostics;

namespace Paillave.Etl.Core.StreamNodes
{
    public abstract class AwaitableStreamNodeBase<TStream, TIn, TArgs> : StreamNodeBase, IStreamNodeOutput<TIn> where TStream : IStream<TIn>
    {
        public TArgs Arguments { get; }
        public TStream Input { get; }
        public IStream<TIn> Output { get; }

        public AwaitableStreamNodeBase(TStream input, string name, TArgs arguments)
            : base(input.ExecutionContext, name)
        {
            this.Arguments = arguments;
            this.Input = input;
            var processedPushObservable = this.ProcessObservable(input.Observable);
            input.ExecutionContext.AddToWaitForCompletion(processedPushObservable);
            this.Output = base.CreateStream<TIn>(name, processedPushObservable);
        }
        protected virtual void ProcessValue(TIn value) { }
        protected virtual IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.Do(ProcessValue);
        }
    }
    public abstract class AwaitableStreamNodeBase<TStream, TIn, TOut, TArgs> : StreamNodeBase, IStreamNodeOutput<TOut> where TStream : IStream<TIn>
    {
        public TArgs Arguments { get; }
        public TStream Input { get; }
        public IStream<TOut> Output { get; }

        public AwaitableStreamNodeBase(TStream input, string name, TArgs arguments)
            : base(input.ExecutionContext, name)
        {
            this.Arguments = arguments;
            this.Input = input;
            var processedPushObservable = this.ProcessObservable(input.Observable);
            input.ExecutionContext.AddToWaitForCompletion(processedPushObservable);
            this.Output = base.CreateStream<TOut>(name, processedPushObservable);
        }
        protected abstract TOut ProcessValue(TIn value);
        protected virtual IPushObservable<TOut> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.Map(ProcessValue);
        }
    }
    //public abstract class AwaitableStreamNodeBase<TStream, TIn, TOut> : StreamNodeBase, IStreamNodeOutput<TOut> where TStream : IStream<TIn>
    //{
    //    public TStream Input { get; }
    //    public IStream<TIn> Output { get; }

    //    public AwaitableStreamNodeBase(TStream input, string name)
    //        : base(input.ExecutionContext, name)
    //    {
    //        this.Input = input;
    //        var processedPushObservable = this.ProcessObservable(input.Observable);
    //        input.ExecutionContext.AddToWaitForCompletion(processedPushObservable);
    //        this.Output = base.CreateStream<TOut>(name, processedPushObservable);
    //    }
    //    protected virtual TOut ProcessValue(TIn value) { }
    //    protected virtual IPushObservable<TOut> ProcessObservable(IPushObservable<TIn> observable)
    //    {
    //        return observable.Map(ProcessValue);
    //    }
    //}
    //public abstract class AwaitableStreamNodeBase<TStream, TIn, TArgs> : AwaitableStreamNodeBase<TStream, TIn> where TStream : IStream<TIn>
    //{
    //    public TArgs Arguments { get; }
    //    public AwaitableStreamNodeBase(TStream input, string name, TArgs arguments)
    //        : base(input, name)
    //    {
    //        this.Arguments = arguments;
    //    }
    //}
}
