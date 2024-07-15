using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core
{
    public class GroupByArgs<TIn, TKey, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<IStream<TIn>, TIn, IStream<TOut>> SubProcess { get; set; }
    }
    public class GroupByStreamNode<TIn, TKey, TOut>(string name, GroupByArgs<TIn, TKey, TOut> args) : StreamNodeBase<TOut, IStream<TOut>, GroupByArgs<TIn, TKey, TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<TOut> CreateOutputStream(GroupByArgs<TIn, TKey, TOut> args)
        {
            if (this.ExecutionContext is GetDefinitionExecutionContext)
            {
                var inputStream = new SingleStream<TIn>(new ChildNodeWrapper(this), PushObservable.FromSingle(default(TIn), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream, default);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.Group(args.GetKey, (iS, firstElement) => args.SubProcess(new Stream<TIn>(new ChildNodeWrapper(this), iS), firstElement).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }

    public class GroupBySortedArgs<TIn, TKey, TOut>
    {
        public ISortedStream<TIn, TKey> Stream { get; set; }
        public Func<IStream<TIn>, TIn, IStream<TOut>> SubProcess { get; set; }
    }
    public class GroupBySortedStreamNode<TIn, TKey, TOut>(string name, GroupBySortedArgs<TIn, TKey, TOut> args) : StreamNodeBase<TOut, IStream<TOut>, GroupBySortedArgs<TIn, TKey, TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(GroupBySortedArgs<TIn, TKey, TOut> args)
        {
            if (this.ExecutionContext is GetDefinitionExecutionContext)
            {
                var inputStream = new SingleStream<TIn>(new ChildNodeWrapper(this), PushObservable.FromSingle(default(TIn), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream, default);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.SortedGroup(args.Stream.SortDefinition.GetKey, (iS, firstElement) => args.SubProcess(new Stream<TIn>(new ChildNodeWrapper(this), iS), firstElement).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }










    public class GroupByCorrelatedArgs<TIn, TKey, TOut>
    {
        public IStream<Correlated<TIn>> Stream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<IStream<Correlated<TIn>>, TIn, IStream<Correlated<TOut>>> SubProcess { get; set; }
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
                var inputStream = new SingleStream<Correlated<TIn>>(new ChildNodeWrapper(this), PushObservable.FromSingle(default(Correlated<TIn>), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream, default);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.Group(i => args.GetKey(i.Row), (iS, firstElement) => args.SubProcess(new Stream<Correlated<TIn>>(new ChildNodeWrapper(this), iS), firstElement.Row).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }




    public class GroupByCorrelatedSortedArgs<TIn, TKey, TOut>
    {
        public ISortedStream<Correlated<TIn>, TKey> Stream { get; set; }
        public Func<IStream<Correlated<TIn>>, TIn, IStream<Correlated<TOut>>> SubProcess { get; set; }
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
                var inputStream = new SingleStream<Correlated<TIn>>(new ChildNodeWrapper(this), PushObservable.FromSingle(default(Correlated<TIn>), null, args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream, default);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            var outputObservable = args.Stream.Observable.SortedGroup(args.Stream.SortDefinition.GetKey, (iS, firstElement) => args.SubProcess(new Stream<Correlated<TIn>>(new ChildNodeWrapper(this), iS), firstElement.Row).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
