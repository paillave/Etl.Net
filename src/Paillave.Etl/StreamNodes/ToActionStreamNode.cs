using Paillave.Etl.Core.Streams;
using System;
using Paillave.RxPush.Operators;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Core;

namespace Paillave.Etl.StreamNodes
{
    public class ToActionArgs<TIn, TStream> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public Action<TIn> ProcessRow { get; set; }
    }
    public class ToActionStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToActionArgs<TIn, TStream>> where TStream : IStream<TIn>
    {
        public ToActionStreamNode(string name, ToActionArgs<TIn, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ToActionArgs<TIn, TStream> args)
        {
            return base.CreateMatchingStream(args.Stream.Observable.Do(args.ProcessRow), args.Stream);
        }
    }
    public class ToActionArgs<TIn, TStream, TResource> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public IStream<TResource> ResourceStream { get; set; }
        public Action<TIn, TResource> ProcessRow { get; set; }
        public Action<TResource> PreProcess { get; set; }
    }
    public class ToActionStreamNode<TIn, TStream, TResource> : StreamNodeBase<TIn, TStream, ToActionArgs<TIn, TStream, TResource>> where TStream : IStream<TIn>
    {
        public override bool IsAwaitable => true;
        public ToActionStreamNode(string name, ToActionArgs<TIn, TStream, TResource> args) : base(name, args)
        {
        }
        protected override TStream CreateOutputStream(ToActionArgs<TIn, TStream, TResource> args)
        {
            var firstStreamWriter = args.ResourceStream.Observable.First();
            if (args.PreProcess != null) firstStreamWriter = firstStreamWriter.Do(i => args.PreProcess(i));
            firstStreamWriter = firstStreamWriter.DelayTillEndOfStream();
            var obs = args.Stream.Observable
                .CombineWithLatest(firstStreamWriter, (i, r) => { args.ProcessRow(i, r); return i; }, true);
            return CreateMatchingStream(obs, args.Stream);
        }
    }
}
