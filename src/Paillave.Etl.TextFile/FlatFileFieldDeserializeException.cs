using System;

namespace Paillave.Etl.TextFile;

public class FlatFileFieldDeserializeException(int sourceColumnIndex, string targetPropertyName, string valueToParse, Exception innerException) : Exception($"could not deserialize value \"{valueToParse}\" in source column #{sourceColumnIndex} for target property \"{targetPropertyName}\"", innerException)
{
    public int SourceColumnIndex { get; } = sourceColumnIndex;
    public string TargetPropertyName { get; } = targetPropertyName;
    public string ValueToParse { get; set; } = valueToParse;
}