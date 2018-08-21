using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.Etl.Helpers;
using SystemIO = System.IO;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.StreamNodes
{
    public class ToResourceStreamArgsBase<TResource>
    {
        public IStream<TResource> ResourceStream { get; set; }
    }
    public abstract class ToResourceStreamNodeBase<TIn, TResource, TArgs> : AwaitableStreamNodeBase<IStream<TIn>, TIn, TArgs>
        where TArgs : ToResourceStreamArgsBase<TResource>
    {
        public ToResourceStreamNodeBase(IStream<TIn> input, string name, TArgs arguments) : base(input, name, arguments)
        {
        }

        protected override IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.CombineWithLatest<TIn, TResource, TIn>(this.Arguments.ResourceStream.Observable.Do(PreProcess).DelayTillEndOfStream(), (i, r) => { ProcessValueToOutput(r, i); return i; }, true);
        }

        protected virtual void PreProcess(TResource outputResource) { }

        protected abstract void ProcessValueToOutput(TResource outputResource, TIn value);
    }
}
