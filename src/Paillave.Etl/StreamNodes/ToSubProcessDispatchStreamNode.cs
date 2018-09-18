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
    // public class ToSubProcessDispatchArgs<TIn, TKey, TOut>
    // {
    //     public IStream<TIn> Stream { get; set; }
    //     public Func<TIn, TKey> GetKey { get; set; }
    //     public Func<IStream<TIn>, IStream<TOut>> SubProcess { get; set; }
    // }
    // public class ToSubProcessDispatchStreamNode<TIn, TKey, TOut> : StreamNodeBase<TOut, IStream<TOut>, ToSubProcessDispatchArgs<TIn, TKey, TOut>>
    // {
    //     private class GroupingElement
    //     {
    //         public GroupingElement()
    //         {
    //             this.SourceObservable = new PushSubject<TIn>();

    //         }
    //         public PushSubject<TIn> SourceObservable { get; }
    //         public IStream<TIn> SourceStream { get; }

    //     }
    //     private object _syncObject = new object();
    //     private IDictionary<TKey, IStream<TIn>> _streamsDictionary = new Dictionary<TKey, IStream<TIn>>();
    //     private IStream<TIn> GetStream(TIn input)
    //     {
    //         TKey key = this.Args.GetKey(input);
    //         IStream<TIn> matchingStream;
    //         if (!_streamsDictionary.TryGetValue(key, out matchingStream))
    //             matchingStream = new Stream<TOut>(this.Tracer, this.ExecutionContext, this.NodeName, observable);
    //     }
    //     public override bool IsAwaitable => true;
    //     public ToSubProcessDispatchStreamNode(string name, ToSubProcessDispatchArgs<TIn, TKey, TOut> args) : base(name, args)
    //     {
    //     }

    //     protected override IStream<TOut> CreateOutputStream(ToSubProcessDispatchArgs<TIn, TKey, TOut> args)
    //     {
    //         var outputObservable = args.Stream.Observable.FlatMap(i =>
    //             {
    //                 EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
    //                 var inputStream = new Stream<TIn>(this.Tracer, this.ExecutionContext, this.NodeName, PushObservable.FromSingle(i, waitHandle));
    //                 var outputStream = args.SubProcess(inputStream);
    //                 var intermediateOutputObservable = outputStream.Observable;
    //                 this.ExecutionContext.AddToWaitForCompletion(this.NodeName, intermediateOutputObservable);
    //                 return new DeferedWrapperPushObservable<TOut>(outputStream.Observable, () => waitHandle.Set());
    //             });
    //         return base.CreateUnsortedStream(outputObservable);
    //     }
    // }
}
