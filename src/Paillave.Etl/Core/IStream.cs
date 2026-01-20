using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core;

public interface IStream<out T> : IStream
{
    IPushObservable<T> Observable { get; }
    // object GetMatchingStream(IPushObservable<T> observable); //normally it would be better to find a way to return the current type, whatever the current class that inherits... but as .net can't handle, return object
}
public interface IStream
{

    INodeContext SourceNode { get; }

    // TODO: the following is an absolute dreadful solution about which I MUST find a proper alternative
    object GetMatchingStream<TOut>(INodeContext sourceNode, object observable); //normally it would be better to find a way to return the current type, whatever the current class that inherits... but as .net can't handle, return object
}
