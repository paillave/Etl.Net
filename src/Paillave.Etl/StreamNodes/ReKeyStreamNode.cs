using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Reflection;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.StreamNodes
{
    public class ReKeyArgs<TIn, TOut, TMultiKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TMultiKey, TOut> ResultSelector { get; set; }
        public Func<TIn, TMultiKey> GetKeys { get; set; }
    }
    public class ReKeyStreamNode<TIn, TOut, TMultiKey> : StreamNodeBase<TOut, IStream<TOut>, ReKeyArgs<TIn, TOut, TMultiKey>>
    {
        public ReKeyStreamNode(string name, ReKeyArgs<TIn, TOut, TMultiKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<TOut> CreateOutputStream(ReKeyArgs<TIn, TOut, TMultiKey> args)
        {
            var keyProcessor = GroupProcessor.Create(args.GetKeys);
            var observableOut = args.InputStream.Observable.Do(keyProcessor.ProcessRow).Last().MultiMap<TIn, TOut>((i, pushValue) =>
            {
                var groups = keyProcessor.GetGroups();
                foreach (var elt in groups.SelectMany(g => g.Value.Select(v => new { Row = v, g.Key })))
                {
                    pushValue(args.ResultSelector(elt.Row, elt.Key));
                }
            });
            return CreateUnsortedStream(observableOut);
        }
    }
}
