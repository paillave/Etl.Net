using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class UnpivotArgs<TIn, TUnpivoted, TOut>
    {
        public IStream<TIn> InputStream { get; set; }
        public IEnumerable<Func<TIn, TUnpivoted>> FieldsToUnpivot { get; set; }
        public Func<TIn, TUnpivoted, TOut> ResultSelector { get; set; }
    }

    public class UnpivotStreamNode<TIn, TUnpivoted, TOut> : StreamNodeBase<TOut, IStream<TOut>, UnpivotArgs<TIn, TUnpivoted, TOut>>
    {
        public UnpivotStreamNode(string name, UnpivotArgs<TIn, TUnpivoted, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(UnpivotArgs<TIn, TUnpivoted, TOut> args)
        {
            return base.CreateUnsortedStream(args.InputStream.Observable.FlatMap(i => PushObservable.FromEnumerable(args.FieldsToUnpivot.Select(inp => args.ResultSelector(i, inp(i))))));
        }
    }
}
