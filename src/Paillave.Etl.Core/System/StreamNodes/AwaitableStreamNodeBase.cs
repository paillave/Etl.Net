using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.System.Streams;
using Paillave.RxPush.Operators;

namespace Paillave.Etl.Core.System
{
    public abstract class AwaitableStreamNodeBase<TStream, TIn> : StreamNodeBase, IStreamNodeOutput<TIn> where TStream : IStream<TIn>
    {
        public TStream Input { get; }
        public IStream<TIn> Output { get; }
        public AwaitableStreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath)
            : base(input.ExecutionContext, name, parentNodeNamePath)
        {
            var processedPushObservable = this.ProcessObservable(input.Observable);
            input.ExecutionContext.WaitCompletion(processedPushObservable);
            this.Output = base.CreateStream<TIn>(name, processedPushObservable);
            this.Input = input;
        }
        protected virtual void ProcessValue(TIn value) { }
        protected virtual IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.Do(ProcessValue);
        }
    }
    public abstract class AwaitableStreamNodeBase<TStream, TIn, TArgs> : AwaitableStreamNodeBase<TStream, TIn> where TStream : IStream<TIn>
    {
        public TArgs Arguments { get; }
        public AwaitableStreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath, TArgs arguments)
            : base(input, name, parentNodeNamePath)
        {
            this.Arguments = arguments;
        }
    }
}
