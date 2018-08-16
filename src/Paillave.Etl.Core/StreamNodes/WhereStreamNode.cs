using Paillave.Etl.Core;
using Paillave.Etl.Core.NodeOutputs;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class WhereArgs<TIn>
    {
        public Func<TIn, bool> Predicate { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; } = false;
    }
    public class WhereStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, WhereArgs<TIn>>, IStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public WhereStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, WhereArgs<TIn> arguments)
            : base(input, name, parentNodeNamePath, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.Map(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Filter(i => i.Output).Map(i => i.Input));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), input.Observable.Filter(arguments.Predicate));
        }

        public IStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }

    public class WhereSortedStreamNode<TIn> : StreamNodeBase<ISortedStream<TIn>, TIn, WhereArgs<TIn>>, ISortedStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public WhereSortedStreamNode(ISortedStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, WhereArgs<TIn> arguments)
            : base(input, name, parentNodeNamePath, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.Map(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateSortedStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Filter(i => i.Output).Map(i => i.Input), input.SortCriterias);
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = base.CreateSortedStream(nameof(this.Output), input.Observable.Filter(arguments.Predicate), input.SortCriterias);
        }

        public ISortedStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }

    public class WhereKeyedStreamNode<TIn> : StreamNodeBase<IKeyedStream<TIn>, TIn, WhereArgs<TIn>>, IKeyedStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public WhereKeyedStreamNode(IKeyedStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, WhereArgs<TIn> arguments)
            : base(input, name, parentNodeNamePath, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.Map(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateKeyedStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Filter(i => i.Output).Map(i => i.Input), input.SortCriterias);
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = base.CreateKeyedStream(nameof(this.Output), input.Observable.Filter(arguments.Predicate), input.SortCriterias);
        }

        public IKeyedStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }

    public static partial class StreamEx
    {
        public static IKeyedStream<TIn> Where<TIn>(this IKeyedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereKeyedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }

        public static IKeyedNodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this IKeyedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereKeyedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new KeyedNodeOutputError<WhereKeyedStreamNode<TIn>, TIn, TIn>(ret);
        }

        public static ISortedStream<TIn> Where<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereSortedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }

        public static ISortedNodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereSortedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new SortedNodeOutputError<WhereSortedStreamNode<TIn>, TIn, TIn>(ret);
        }

        public static IStream<TIn> Where<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }

        public static INodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<WhereStreamNode<TIn>, TIn, TIn>(ret);
        }
    }
}
