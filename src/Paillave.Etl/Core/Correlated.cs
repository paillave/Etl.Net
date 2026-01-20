using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core;

public interface ICorrelated
{
    object Row { get; }
    HashSet<Guid> CorrelationKeys { get; set; }
}

public class Correlated<TRow> : ICorrelated
{
    public TRow Row { get; set; }
    public HashSet<Guid> CorrelationKeys { get; set; }
    object ICorrelated.Row => this.Row;
}
