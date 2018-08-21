using Paillave.Etl.Core;
using System;
using System.Linq;
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
    public class ToResourceStreamArgsBase<TResource, TIn, TResKey>
    {
        public IStream<TResource> ResourceStream { get; set; }
        public Func<TResource, TResKey> GetResourceKey { get; set; }
        public Func<TIn, TResKey> GetInputResourceKey { get; set; }
    }
    public abstract class ToResourceStreamNodeBase<TIn, TResource, TArgs, TResKey> : AwaitableStreamNodeBase<IStream<TIn>, TIn, TArgs>
        where TArgs : ToResourceStreamArgsBase<TResource, TIn, TResKey>
    {
        public ToResourceStreamNodeBase(IStream<TIn> input, string name, TArgs arguments) : base(input, name, arguments)
        {
        }

        protected override IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            var dicoResourceS = this.Arguments.ResourceStream.Observable.Do(PreProcess).ToList().Map(rs => rs.ToDictionary(this.Arguments.GetResourceKey));
            return observable.CombineWithLatest<TIn, Dictionary<TResKey, TResource>, TIn>(dicoResourceS, (i, r) => { ProcessValueToOutput(r[this.Arguments.GetInputResourceKey(i)], i); return i; }, true);
        }

        protected virtual void PreProcess(TResource outputResource) { }

        protected abstract void ProcessValueToOutput(TResource outputResource, TIn value);
    }





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
            var firstResourceS = this.Arguments.ResourceStream.Observable.First().Do(PreProcess).DelayTillEndOfStream();
            return observable.CombineWithLatest<TIn, TResource, TIn>(firstResourceS, (i, r) => { ProcessValueToOutput(r, i); return i; }, true);
        }

        protected virtual void PreProcess(TResource outputResource) { }

        protected abstract void ProcessValueToOutput(TResource outputResource, TIn value);
    }
}
