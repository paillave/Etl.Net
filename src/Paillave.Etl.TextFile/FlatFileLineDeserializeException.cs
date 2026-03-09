using System;

namespace Paillave.Etl.TextFile;

public class FlatFileLineDeserializeException : Exception
{
    public int LineIndex { get; }
    public string SourceName { get; }
    public FlatFileLineDeserializeException(string sourceName, int lineIndex, Exception innerException) : base($"could not deserialize file \"{sourceName}\" at line #{lineIndex}: {innerException.Message}", innerException)
        => (LineIndex, SourceName) = (lineIndex, sourceName);
}