using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.StreamNodes
{
    public class GroupByArgs<TIn, TKey, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<IStream<TIn>, IStream<TOut>> SubProcess { get; set; }
    }
    public class GroupByStreamNode<TIn, TKey, TOut> : StreamNodeBase<TOut, IStream<TOut>, GroupByArgs<TIn, TKey, TOut>>
    {
        public GroupByStreamNode(string name, GroupByArgs<TIn, TKey, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<TOut> CreateOutputStream(GroupByArgs<TIn, TKey, TOut> args)
        {
            if (this.ExecutionContext is GetDefinitionExecutionContext)
            {
                var inputStream = new SingleStream<TIn>(new SubNodeWrapper(this), PushObservable.FromSingle(default(TIn), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.Group(args.GetKey, iS => args.SubProcess(new Stream<TIn>(new SubNodeWrapper(this), iS)).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }

    public class GroupBySortedArgs<TIn, TKey, TOut>
    {
        public ISortedStream<TIn, TKey> Stream { get; set; }
        public Func<IStream<TIn>, IStream<TOut>> SubProcess { get; set; }
    }
    public class GroupBySortedStreamNode<TIn, TKey, TOut> : StreamNodeBase<TOut, IStream<TOut>, GroupBySortedArgs<TIn, TKey, TOut>>
    {
        public GroupBySortedStreamNode(string name, GroupBySortedArgs<TIn, TKey, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(GroupBySortedArgs<TIn, TKey, TOut> args)
        {
            if (this.ExecutionContext is GetDefinitionExecutionContext)
            {
                var inputStream = new SingleStream<TIn>(new SubNodeWrapper(this), PushObservable.FromSingle(default(TIn), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.SortedGroup(args.Stream.SortDefinition.GetKey, iS => args.SubProcess(new Stream<TIn>(new SubNodeWrapper(this), iS)).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }










    public class GroupByCorrelatedArgs<TIn, TKey, TOut>
    {
        public IStream<Correlated<TIn>> Stream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<IStream<Correlated<TIn>>, IStream<Correlated<TOut>>> SubProcess { get; set; }
    }
    public class GroupByCorrelatedStreamNode<TIn, TKey, TOut> : StreamNodeBase<Correlated<TOut>, IStream<Correlated<TOut>>, GroupByCorrelatedArgs<TIn, TKey, TOut>>
    {
        public GroupByCorrelatedStreamNode(string name, GroupByCorrelatedArgs<TIn, TKey, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<Correlated<TOut>> CreateOutputStream(GroupByCorrelatedArgs<TIn, TKey, TOut> args)
        {
            if (this.ExecutionContext is GetDefinitionExecutionContext)
            {
                var inputStream = new SingleStream<Correlated<TIn>>(new SubNodeWrapper(this), PushObservable.FromSingle(default(Correlated<TIn>), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.Group(i => args.GetKey(i.Row), iS => args.SubProcess(new Stream<Correlated<TIn>>(new SubNodeWrapper(this), iS)).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }




    public class GroupByCorrelatedSortedArgs<TIn, TKey, TOut>
    {
        public ISortedStream<Correlated<TIn>, TKey> Stream { get; set; }
        public Func<IStream<Correlated<TIn>>, IStream<Correlated<TOut>>> SubProcess { get; set; }
    }
    public class GroupByCorrelatedSortedStreamNode<TIn, TKey, TOut> : StreamNodeBase<Correlated<TOut>, IStream<Correlated<TOut>>, GroupByCorrelatedSortedArgs<TIn, TKey, TOut>>
    {
        public GroupByCorrelatedSortedStreamNode(string name, GroupByCorrelatedSortedArgs<TIn, TKey, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<Correlated<TOut>> CreateOutputStream(GroupByCorrelatedSortedArgs<TIn, TKey, TOut> args)
        {
            if (this.ExecutionContext is GetDefinitionExecutionContext)
            {
                var inputStream = new SingleStream<Correlated<TIn>>(new SubNodeWrapper(this), PushObservable.FromSingle(default(Correlated<TIn>), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.SortedGroup(args.Stream.SortDefinition.GetKey, iS => args.SubProcess(new Stream<Correlated<TIn>>(new SubNodeWrapper(this), iS)).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
