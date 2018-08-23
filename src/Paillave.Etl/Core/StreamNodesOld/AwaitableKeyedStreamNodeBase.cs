using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;

namespace Paillave.Etl.Core.StreamNodesOld
{
    public abstract class AwaitableKeyedStreamNodeBase<TStream, TIn> : StreamNodeBase, IKeyedStreamNodeOutput<TIn> where TStream : IKeyedStream<TIn>
    {
        public TStream Input { get; }
        public IKeyedStream<TIn> Output { get; }
        public AwaitableKeyedStreamNodeBase(TStream input, string name)
            : base(input.ExecutionContext, name)
        {
            var processedPushObservable = this.ProcessObservable(input.Observable);
            input.ExecutionContext.AddToWaitForCompletion(name, processedPushObservable);
            this.Output = base.CreateKeyedStream<TIn>(name, processedPushObservable, input.SortCriterias);
            this.Input = input;
        }
        protected virtual void ProcessValue(TIn value) { }
        protected virtual IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.Do(ProcessValue);
        }
    }
    public abstract class AwaitableKeyedStreamNodeBase<TStream, TIn, TArgs> : AwaitableKeyedStreamNodeBase<TStream, TIn> where TStream : IKeyedStream<TIn>
    {
        public TArgs Arguments { get; }
        public AwaitableKeyedStreamNodeBase(TStream input, string name, TArgs arguments)
            : base(input, name)
        {
            this.Arguments = arguments;
        }
    }
}
