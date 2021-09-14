---
sidebar_position: 8
---

# Make a custom operator

![CustomOperator](/img/xamarin-extends-platforms-toolbox-devices.svg)

Making a custom operator involves making a `CustomStreamNode`. A stream node is an operator.

## Simplest operator

First let's make the class that will host arguments for the operator. This class must contain all the input streams and all the necessary parameters for the operator.

```cs
public class CustomArgs
{
    public Stream<string> Stream { get; set; }
}
```

Then we must make a stream node. This will be the implementation of the operator. This is done by implementing a concrete version of `StreamNodeBase`.
It has 3 type parameters:

1. The type of output events payloads
2. The type of the output stream
3. The type that hosts parameters, including input streams

```cs
public class CustomStreamNode : StreamNodeBase<string, IStream<string>, CustomArgs>
{
    public CustomStreamNode(string name, CustomArgs args) : base(name, args) { }
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override IStream<string> CreateOutputStream(CustomArgs args)
        => base.CreateUnsortedStream(args.Stream.Observable.Do(Console.WriteLine));
}
```

This operator processes `string` events from the only input stream by displaying it on console.

:::info

How to make something more evolved than a simple `Do`?

The `Do` is one of the many reactive operators that are available. These operators are mostly the very same than the one that are mentioned in [ReactiveX](http://reactivex.io/). To get more examples, check the ones that are implemented in the the source code of this project.

:::

Now, an extension shall be done for an easier use of the operator:

```cs
public static partial class CustomEx
{
    public static IStream<string> Custom(this Stream<string> stream, string name)
        => new CustomStreamNode(name, new CustomArgs { Stream = stream }).Output;
}
```

This operator makes a simple action without actually modifying the stream it self. The problem is that if we give a sorted/single/keyed stream on the input, we get a stream that is not marked as is anymore. For this we must permit to have a strongly typed stream on t he input, ensuring it is still a stream of strings. Then we must have the output stream that is the same type of stream than the input one. The method `CreateMatchingStream` takes an observable to make a stream out of it that will match the given stream.

```cs title="CustomStreamNode.cs" {7,9,11,13,16,17,21,22}
using System;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Demo
{
    public class CustomArgs<TStream> where TStream : IStream<string>
    {
        public TStream Stream { get; set; }
    }
    public class CustomStreamNode<TStream> : StreamNodeBase<string, TStream, CustomArgs<TStream>> where TStream : IStream<string>
    {
        public CustomStreamNode(string name, CustomArgs<TStream> args) : base(name, args) { }
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
}
```

## Operator that issues a different stream

Here, we will remake the `Select` operator that we will call `SimpleSelect`.

The operator needs the input stream, and how to transform every occurrence. We will make the arguments class to contain this:

```cs
public class CustomSelectArgs<TIn, TOut>
{
    public Stream<TIn> Stream { get; set; }
    public Func<TIn, TOut> Selector { get; set; }
}
```

Now, we will make the operator by knowing that, as a matter of a fact, whatever the order of the stream, as a transformation of payload is done, we can't possibly know if the stream will still be sorted or keyed. This is why we will leave the output stream as an `IStream` and we will issue it with a `CreateUnsortedStream`.

```cs {6,7}
public class CustomSelectStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, CustomSelectArgs<TIn, TOut>>
{
    public CustomSelectStreamNode(string name, CustomSelectArgs<TIn, TOut> args) : base(name, args) { }
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override IStream<TOut> CreateOutputStream(CustomSelectArgs<TIn, TOut> args)
        => base.CreateUnsortedStream(args.Stream.Observable.Map(args.Selector));
}
```

Then, we make a function to use the operator easily:

```cs
public static partial class CustomEx
{
    public static IStream<TOut> CustomSelect<TIn, TOut>(this Stream<TIn> stream, string name, Func<TIn, TOut> selector)
        => new CustomSelectStreamNode<TIn, TOut>(name, new CustomSelectArgs<TIn, TOut> { Stream = stream, Selector = selector }).Output;
}
```

Here is the full source code for the `CustomSelect` operator:

```cs title="CustomSelect.cs"
using System;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Demo
{
    public class CustomSelectArgs<TIn, TOut>
    {
        public Stream<TIn> Stream { get; set; }
        public Func<TIn, TOut> Selector { get; set; }
    }
    public class CustomSelectStreamNode<TIn, TOut>
        : StreamNodeBase<TOut, IStream<TOut>, CustomSelectArgs<TIn, TOut>>
    {
        public CustomSelectStreamNode(string name, CustomSelectArgs<TIn, TOut> args)
            : base(name, args) { }
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
}
```
