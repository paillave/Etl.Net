using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface IStream<T> : IStream
    {
        IPushObservable<T> Observable { get; }
        // void Initialize(ITracer tracer, IExecutionContext executionContext, string sourceNodeName, string name, IPushObservable<T> observable);
    }
    public interface IStream
    {
        IExecutionContext ExecutionContext { get; }
        string SourceNodeName { get; }
    }
}
