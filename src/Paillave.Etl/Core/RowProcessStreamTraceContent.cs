

namespace Paillave.Etl.Core;

public class RowProcessStreamTraceContent(int position, int? averageDuration, object row) : StreamTraceContentBase
{
    public override EtlTraceLevel Level => EtlTraceLevel.Verbose;

    public int Position { get; } = position;
    public int? AverageDuration { get; } = averageDuration;
    public object Row { get; } = row;

    public override string Message => $"row {Position} processing (avg:{AverageDuration ?? 0} ms)";
}
