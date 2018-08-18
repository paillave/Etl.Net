using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class ToActionStreamNode<TIn> : AwaitableStreamNodeBase<IStream<TIn>, TIn, Action<TIn>>
    {
        public ToActionStreamNode(IStream<TIn> input, string name, Action<TIn> arguments) : base(input, name, arguments)
        {
        }
        protected override void ProcessValue(TIn value)
        {
            this.Arguments(value);
        }
    }
    public class ToActionStreamNode<TIn, TOut> : AwaitableStreamNodeBase<IStream<TIn>, TIn, TOut, Func<TIn, TOut>>
    {
        public ToActionStreamNode(IStream<TIn> input, string name, Func<TIn, TOut> arguments) : base(input, name, arguments)
        {
        }
        protected override TOut ProcessValue(TIn value)
        {
            return this.Arguments(value);
        }
    }



    public class ToActionResourceStreamNode<TIn, TRes> : AwaitableStreamNodeBase<IStream<TIn>, TIn, ToActionArgs<TIn, TRes>>
    {
        public ToActionResourceStreamNode(IStream<TIn> input, string name, ToActionArgs<TIn, TRes> arguments) : base(input, name, arguments)
        {
        }
        protected override IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.CombineWithLatest(this.Arguments.ResourceStream.Observable, (i, r) =>
            {
                this.Arguments.Action(i, r);
                return i;
            });
        }
        protected override void ProcessValue(TIn value)
        {
            throw new NotSupportedException("This method should not be called");
        }
    }
    public class ToActionResourceStreamNode<TIn, TRes, TOut> : AwaitableStreamNodeBase<IStream<TIn>, TIn, TOut, ToActionArgs<TIn, TRes, TOut>>
    {
        public ToActionResourceStreamNode(IStream<TIn> input, string name, ToActionArgs<TIn, TRes, TOut> arguments) : base(input, name, arguments)
        {
        }
        protected override IPushObservable<TOut> ProcessObservable(IPushObservable<TIn> observable)
        {
            return observable.CombineWithLatest(this.Arguments.ResourceStream.Observable, this.Arguments.Action);
        }
        protected override TOut ProcessValue(TIn value)
        {
            throw new NotSupportedException("This method should not be called");
        }
    }




    public class ToActionArgs<TIn, TRes>
    {
        public IStream<TRes> ResourceStream { get; set; }
        public Action<TIn, TRes> Action { get; set; }
    }
    public class ToActionArgs<TIn, TRes, TOut>
    {
        public IStream<TRes> ResourceStream { get; set; }
        public Func<TIn, TRes, TOut> Action { get; set; }
    }
}
