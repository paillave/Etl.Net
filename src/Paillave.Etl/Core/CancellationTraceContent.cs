
namespace Paillave.Etl.Core;

public class CancellationTraceContent(CancellationCause cause) : StreamTraceContentBase
{
    private readonly CancellationCause _cause = cause;

    public override EtlTraceLevel Level => EtlTraceLevel.Warning;
    private string TextCancellationCause
        => _cause == CancellationCause.CancelledBecauseOfError ? "an error occurred" : "cancellation requested from outside";

    public override string Message => $"Process cancelled: {TextCancellationCause}";
}
public enum CancellationCause
{
    CancelledFromOutside = 1,
    CancelledBecauseOfError = 2
}




