using Paillave.Etl.Core;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using System.Reflection;
using Paillave.RxPush.Core;
using System.Threading;
using Paillave.Etl.Helpers;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.StreamNodes
{
    public class CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
    {
        public Func<TIn, TValueIn> InputValueSelector { get; set; }
        public IValuesProvider<TValueIn, TValueOut> ValuesProvider { get; set; }
        public Func<TValueOut, TOut> OutputValueSelector { get; set; }
    }
    public class CrossApplyStreamNode<TIn, TValueIn, TValueOut, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>>, IStreamNodeOutput<TOut>
    {
        public CrossApplyStreamNode(IStream<TIn> input, string name, CrossApplyArgs<TIn, TValueIn, TValueOut, TOut> args) : base(input, name, args)
        {
            this.Output = base.CreateStream(nameof(this.Output), input.Observable.FlatMap(i => args.ValuesProvider.PushValues(args.InputValueSelector(i))).Map(args.OutputValueSelector));
        }

        public IStream<TOut> Output { get; }
    }
    public class CrossApplyResourceArgs<TIn, TRes, TValueIn, TValueOut, TOut>
    {
        public IStream<TRes> ResourceStream { get; set; }
        public Func<TIn, TValueIn> InputValueSelector { get; set; }
        public IValuesProvider<TValueIn, TRes, TValueOut> ValuesProvider { get; set; }
        public Func<TValueOut, TOut> OutputValueSelector { get; set; }
    }
    public class CrossApplyResourceStreamNode<TIn, TRes, TValueIn, TValueOut, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyResourceArgs<TIn, TRes, TValueIn, TValueOut, TOut>>, IStreamNodeOutput<TOut>
    {
        public CrossApplyResourceStreamNode(IStream<TIn> input, string name, CrossApplyResourceArgs<TIn, TRes, TValueIn, TValueOut, TOut> args) : base(input, name, args)
        {
            var ob = input.Observable.CombineWithLatest(args.ResourceStream.Observable, (i, r) => new { Input = i, Resource = r });
            this.Output = base.CreateStream(nameof(this.Output), ob.FlatMap(i => args.ValuesProvider.PushValues(i.Resource, args.InputValueSelector(i.Input))).Map(args.OutputValueSelector));
        }

        public IStream<TOut> Output { get; }
    }
}