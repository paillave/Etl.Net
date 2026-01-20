

namespace Paillave.Etl.Core;

public class CounterSummaryStreamTraceContent(int counter) : StreamTraceContentBase
{
    public override EtlTraceLevel Level => EtlTraceLevel.Info;

    public int Counter { get; } = counter;

    public override string Message => $"{this.Counter} row(s) issued";
}
