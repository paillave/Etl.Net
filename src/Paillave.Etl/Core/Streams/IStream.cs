using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface IStream<T> : IStream
    {
        IPushObservable<T> Observable { get; }
        // object GetMatchingStream(IPushObservable<T> observable); //normally it would be better to find a way to return the current type, whatever the current class that inherits... but as .net can't handle, return object
    }
    public interface IStream
    {
        IExecutionContext ExecutionContext { get; }
        string SourceNodeName { get; }
        object GetMatchingStream(ITracer tracer, IExecutionContext executionContext, string name, object observable); //normally it would be better to find a way to return the current type, whatever the current class that inherits... but as .net can't handle, return object
    }
}
