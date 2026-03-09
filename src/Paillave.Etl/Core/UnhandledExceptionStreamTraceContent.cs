using System;


namespace Paillave.Etl.Core;

public class UnhandledExceptionStreamTraceContent : StreamTraceContentBase
{
    public UnhandledExceptionStreamTraceContent(Exception ex) => (Exception) = (ex);
    public override EtlTraceLevel Level => EtlTraceLevel.Error;
    public Exception Exception { get; }
    public override string Message => $"Unhandled exception: {this.Exception.GetFullMessage()}";
}
