using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class SelectSectionArgs<TIn>
    {
        public IStream<TIn> Stream { get; set; }
        public KeepingState? InitialState { get; set; } = null;
        public Func<TIn, SwitchBehavior> SwitchToKeep { get; set; } = null;
        public Func<TIn, SwitchBehavior> SwitchToIgnore { get; set; } = null;
    }
    public class SelectSectionStreamNode<TIn> : StreamNodeBase<TIn, IStream<TIn>, SelectSectionArgs<TIn>>
    {
        public SelectSectionStreamNode(string name, SelectSectionArgs<TIn> args) : base(name, args)
        {
        }

        protected override IStream<TIn> CreateOutputStream(SelectSectionArgs<TIn> args)
        {
            IPushObservable<TIn> obs;
            if (args.InitialState == null)
            {
                if (args.SwitchToIgnore == null)
                    obs = args.Stream.Observable.FilterSection(args.SwitchToKeep);
                else
                    obs = args.Stream.Observable.FilterSection(args.SwitchToKeep, args.SwitchToIgnore);
            }
            else
            {
                if (args.SwitchToIgnore == null)
                    obs = args.Stream.Observable.FilterSection(args.InitialState.Value, args.SwitchToKeep);
                else
                    obs = args.Stream.Observable.FilterSection(args.InitialState.Value, args.SwitchToKeep, args.SwitchToIgnore);
            }
            return base.CreateUnsortedStream(obs);
        }
    }
}
