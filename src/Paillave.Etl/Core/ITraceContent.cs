namespace Paillave.Etl.Core;

public interface ITraceContent
{
    string Type { get; }
    EtlTraceLevel Level { get; }
    string Message { get; }
}

public enum EtlTraceLevel
{
    //
    // Summary:
    //     Output no tracing and debugging messages.
    Off = 0,
    //
    // Summary:
    //     Output error-handling messages.
    Error = 1,
    //
    // Summary:
    //     Output warnings and error-handling messages.
    Warning = 2,
    //
    // Summary:
    //     Output informational messages, warnings, and error-handling messages.
    Info = 3,
    //
    // Summary:
    //     Output all debugging and tracing messages.
    Verbose = 4
}
