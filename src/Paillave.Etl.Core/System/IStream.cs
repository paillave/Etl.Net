using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.System
{
    public interface IStream<T>: IContextual
    {
        IObservable<T> Observable { get; }
        IEnumerable<string> SourceNodeName { get; }
        string SourceOutputName { get; }
    }
}