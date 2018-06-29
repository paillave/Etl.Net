using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.System
{
    public interface IKeyedStream<T> : ISortedStream<T>
    {
    }
}