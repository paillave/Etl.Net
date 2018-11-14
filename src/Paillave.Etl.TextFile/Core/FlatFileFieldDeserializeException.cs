using System;

namespace Paillave.Etl.TextFile.Core
{
    public class FlatFileFieldDeserializeException : Exception
    {
        public int SourceColumnIndex { get; }
        public string TargetPropertyName { get; }
        public string ValueToParse { get; set; }
        public FlatFileFieldDeserializeException(int sourceColumnIndex, string targetPropertyName, string valueToParse, Exception innerException) : base($"could not deserialize value \"{valueToParse}\" in source column #{sourceColumnIndex} for target property \"{targetPropertyName}\"", innerException)
        {
            this.SourceColumnIndex = sourceColumnIndex;
            this.TargetPropertyName = targetPropertyName;
            this.ValueToParse = valueToParse;
        }
    }
}