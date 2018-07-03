using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.SystemOld
{
    public interface IStream<T>
    {
        IObservable<T> Observable { get; }
        IEnumerable<string> SourceNodeName { get; }
        string SourceOutputName { get; }
    }
}