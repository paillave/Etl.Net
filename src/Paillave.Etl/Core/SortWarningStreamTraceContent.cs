

namespace Paillave.Etl.Core;

public class SortWarningStreamTraceContent : StreamTraceContentBase
{
    public SortWarningStreamTraceContent() : base()
    {
    }
    public override EtlTraceLevel Level => EtlTraceLevel.Warning;

    public override string Message => $"lots of rows are sorted and therefore set in memory";
}
