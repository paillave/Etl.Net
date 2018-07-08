using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
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
                var errorManagedResult = input.Observable.Select(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Where(i => i.Output).Select(i => i.Input));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), input.Observable.Where(arguments.Predicate));
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
                var errorManagedResult = input.Observable.Select(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateSortedStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Where(i => i.Output).Select(i => i.Input), input.SortCriterias);
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = base.CreateSortedStream(nameof(this.Output), input.Observable.Where(arguments.Predicate), input.SortCriterias);
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
                var errorManagedResult = input.Observable.Select(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateKeyedStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Where(i => i.Output).Select(i => i.Input), input.SortCriterias);
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = base.CreateKeyedStream(nameof(this.Output), input.Observable.Where(arguments.Predicate), input.SortCriterias);
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

        public static KeyedNodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this IKeyedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereKeyedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new KeyedNodeOutputError<TIn, TIn>(ret.Output, ret.Error);
        }

        public static ISortedStream<TIn> Where<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereSortedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }

        public static SortedNodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereSortedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new SortedNodeOutputError<TIn, TIn>(ret.Output, ret.Error);
        }

        public static IStream<TIn> Where<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }

        public static NodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<TIn, TIn>(ret.Output, ret.Error);
        }
    }
}
