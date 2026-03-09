using System;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace BlogTutorial;

// public class CustomArgs<TIn, TStream> where TStream : IStream<TIn>
// {
//     public TStream Stream { get; set; }
//     public Action<TIn> Processor { get; set; }
// }
// public class CustomStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, CustomArgs<TIn, TStream>> where TStream : IStream<TIn>
// {
//     public CustomStreamNode(string name, CustomArgs<TIn, TStream> args) : base(name, args) { }
//     public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
//     public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
//     protected override TStream CreateOutputStream(CustomArgs<TIn, TStream> args)
//         => base.CreateMatchingStream(args.Stream.Observable.Do(args.Processor), args.Stream);
// }
// public static partial class CustomEx
// {
//     public static TStream Custom<TIn, TStream>(this TStream stream, string name, Action<TIn> processor) where TStream : IStream<TIn>
//         => new CustomStreamNode<TIn, TStream>(name, new CustomArgs<TIn, TStream> { Stream = stream, Processor = processor }).Output;
// }



public class CustomArgs<TStream> where TStream : IStream<string>
{
    public TStream Stream { get; set; }
}
public class CustomStreamNode<TStream>(string name, CustomArgs<TStream> args) : StreamNodeBase<string, TStream, CustomArgs<TStream>>(name, args) where TStream : IStream<string>
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override TStream CreateOutputStream(CustomArgs<TStream> args)
        => base.CreateMatchingStream(args.Stream.Observable.Do(Console.WriteLine), args.Stream);
}
public static partial class CustomEx
{
    public static TStream Custom<TStream>(this TStream stream, string name) where TStream : IStream<string>
        => new CustomStreamNode<TStream>(name, new CustomArgs<TStream> { Stream = stream }).Output;
}






// base.ExecutionContext.DependencyResolver.Resolve()

public class CustomArgs
{
    public Stream<string> Stream { get; set; }
}
public class CustomStreamNode(string name, CustomArgs args) : StreamNodeBase<string, IStream<string>, CustomArgs>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override IStream<string> CreateOutputStream(CustomArgs args)
        => base.CreateUnsortedStream(args.Stream.Observable.Do(Console.WriteLine));
}


public static partial class CustomEx
{
    public static IStream<string> Custom(this Stream<string> stream, string name)
        => new CustomStreamNode(name, new CustomArgs { Stream = stream }).Output;
}















public class CustomSelectArgs<TIn, TOut>
{
    public Stream<TIn> Stream { get; set; }
    public Func<TIn, TOut> Selector { get; set; }
}
public class CustomSelectStreamNode<TIn, TOut>(string name, CustomSelectArgs<TIn, TOut> args)
    : StreamNodeBase<TOut, IStream<TOut>, CustomSelectArgs<TIn, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override IStream<TOut> CreateOutputStream(CustomSelectArgs<TIn, TOut> args)
        => base.CreateUnsortedStream(args.Stream.Observable.Map(args.Selector));
}
public static partial class CustomEx
{
    public static IStream<TOut> CustomSelect<TIn, TOut>(this Stream<TIn> stream, string name, Func<TIn, TOut> selector)
        => new CustomSelectStreamNode<TIn, TOut>(name,
            new CustomSelectArgs<TIn, TOut>
            {
                Stream = stream,
                Selector = selector
            }).Output;
}