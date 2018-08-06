using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.System.Streams;
using Paillave.RxPush.Core;

namespace Paillave.Etl.Core.StreamNodes
{
    public class ToFlatFileStreamArgs<TIn> where TIn : new()
    {
        public ColumnNameFlatFileDescriptor<TIn> Mapper { get; set; }
    }
    public class ToFlatFileStreamNode<TIn> : AwaitableStreamNodeBase<IStream<TIn>, TIn, ToFlatFileStreamArgs<TIn>> where TIn : new()
    {
        public ToFlatFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, ToFlatFileStreamArgs<TIn> arguments) : base(input, name, parentNodeNamePath, arguments)
        {
        }
        protected override IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            return base.ProcessObservable(observable);
        }
        protected override void ProcessValue(TIn value)
        {
            this.Arguments(value);
        }
    }

    //public class SkipSortedStreamNode<TIn> : StreamNodeBase<ISortedStream<TIn>, TIn, int>, ISortedStreamNodeOutput<TIn>
    //{
    //    public ISortedStream<TIn> Output { get; }
    //    public SkipSortedStreamNode(ISortedStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, int arguments) : base(input, name, parentNodeNamePath, arguments)
    //    {
    //        this.Output = base.CreateSortedStream(nameof(Output), input.Observable.Skip(arguments), input.SortCriterias);
    //    }
    //}

    //public class SkipKeyedStreamNode<TIn> : StreamNodeBase<IKeyedStream<TIn>, TIn, int>, IKeyedStreamNodeOutput<TIn>
    //{
    //    public IKeyedStream<TIn> Output { get; }
    //    public SkipKeyedStreamNode(IKeyedStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, int arguments) : base(input, name, parentNodeNamePath, arguments)
    //    {
    //        this.Output = base.CreateKeyedStream(nameof(Output), input.Observable.Skip(arguments), input.SortCriterias);
    //    }
    //}

    public static partial class StreamEx
    {
        public static IStream<TIn> ToAction<TIn>(this IStream<TIn> stream, string name, Action<TIn> action)
        {
            return new ToActionStreamNode<TIn>(stream, name, null, action).Output;
        }
        //public static ISortedStream<TIn> Skip<TIn>(this ISortedStream<TIn> stream, string name, int count)
        //{
        //    return new SkipSortedStreamNode<TIn>(stream, name, null, count).Output;
        //}
        //public static IKeyedStream<TIn> Skip<TIn>(this IKeyedStream<TIn> stream, string name, int count)
        //{
        //    return new SkipKeyedStreamNode<TIn>(stream, name, null, count).Output;
        //}
    }
}
