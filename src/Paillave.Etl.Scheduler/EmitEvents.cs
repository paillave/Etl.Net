using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Scheduler;
// public class EmitEventsArgs<TIn, TOut, TKey> where TKey : IEquatable<TKey>
// {
//     public SingleStream<TIn> Stream { get; set; }
//     public TickEmitterManager<TOut, TKey> EmitterManager { get; set; }
// }
// public class EmitEventsStreamNode<TIn, TOut, TKey>
//     : StreamNodeBase<TOut, IStream<TOut>, EmitEventsArgs<TIn, TOut, TKey>>
//      where TKey : IEquatable<TKey>
// {
//     public EmitEventsStreamNode(string name, EmitEventsArgs<TIn, TOut, TKey> args)
//         : base(name, args) { }
//     public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
//     public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
//     protected override IStream<TOut> CreateOutputStream(EmitEventsArgs<TIn, TOut, TKey> args)
//         => base.CreateUnsortedStream(args.Stream.Observable./*Reactive Process*/);
// }
public static partial class CustomEx
{
    public static IStream<TOut> EmitEvents<TOut, TKey>(this SingleStream<object> stream, string name, TickEmitterManager<TOut, TKey> emitterManager) where TKey : IEquatable<TKey>
        => stream.CrossApply(name,  );
    // => new EmitEventsStreamNode<object, TOut, TKey>(name,
    //     new EmitEventsArgs<object, TOut, TKey>
    //     {
    //         Stream = stream
    //     }).Output;
}
internal class TicksProvider<TOut, TKey> : IValuesProvider<object, TOut> where TKey : IEquatable<TKey>
{
    private readonly TickEmitterManager<TOut, TKey> _emitterManager;
    public TicksProvider(TickEmitterManager<TOut, TKey> emitterManager)
        => _emitterManager = emitterManager;
    public string TypeName => nameof(TicksProvider<TOut, TKey>);
    public ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    public void PushValues(object input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
    {
        _emitterManager.
        throw new NotImplementedException();
    }
}