using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface IStream<T> : IStream
    {
        IPushObservable<T> Observable { get; }
    }
    public interface IStream
    {
        IExecutionContext ExecutionContext { get; }
        string Name { get; }
        string SourceNodeName { get; }
    }
}