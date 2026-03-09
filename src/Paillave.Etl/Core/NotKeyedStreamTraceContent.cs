

namespace Paillave.Etl.Core;

public class NotKeyedStreamTraceContent(int lineNumber) : StreamTraceContentBase
{
    public int LineNumber { get; } = lineNumber;
    public override EtlTraceLevel Level => EtlTraceLevel.Error;
    public override string Message => $"The stream is not keyed at line {LineNumber} whereas it should be";
}
