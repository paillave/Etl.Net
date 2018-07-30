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
    public abstract class AwaitableSortedStreamNodeBase<TStream, TIn> : StreamNodeBase, ISortedStreamNodeOutput<TIn> where TStream : ISortedStream<TIn>
    {
        public TStream Input { get; }
        public ISortedStream<TIn> Output { get; }
        public AwaitableSortedStreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath)
            : base(input.ExecutionContext, name, parentNodeNamePath)
        {
            var processedPushObservable = this.ProcessObservable(input.Observable);
            input.ExecutionContext.WaitCompletion(processedPushObservable);
            this.Output = base.CreateSortedStream<TIn>(name, processedPushObservable, input.SortCriterias);
            this.Input = input;
        }
        protected virtual void ProcessValue(TIn value) { }
        protected virtual IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.Do(ProcessValue);
        }
    }
    public abstract class AwaitableSortedStreamNodeBase<TStream, TIn, TArgs> : AwaitableSortedStreamNodeBase<TStream, TIn> where TStream : ISortedStream<TIn>
    {
        public TArgs Arguments { get; }
        public AwaitableSortedStreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath, TArgs arguments)
            : base(input, name, parentNodeNamePath)
        {
            this.Arguments = arguments;
        }
    }
}
