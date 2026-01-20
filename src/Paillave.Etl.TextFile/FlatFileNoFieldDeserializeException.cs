using System;

namespace Paillave.Etl.TextFile;

public class FlatFileNoFieldDeserializeException(int sourceColumnIndex, string targetPropertyName, Exception innerException) : Exception($"could not get value to deserialize in source column {sourceColumnIndex} for target property {targetPropertyName}", innerException)
{
    public int SourceColumnIndex { get; } = sourceColumnIndex;
    public string TargetPropertyName { get; } = targetPropertyName;
}