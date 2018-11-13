using System;

namespace Paillave.Etl.TextFile.Core
{
    public class FlatFileNoFieldDeserializeException : Exception
    {
        public int SourceColumnIndex { get; }
        public string TargetPropertyName { get; }
        public FlatFileNoFieldDeserializeException(int sourceColumnIndex, string targetPropertyName, Exception innerException) : base($"could not get value to deserialize in source column {sourceColumnIndex} for target property {targetPropertyName}", innerException)
        {
            this.SourceColumnIndex = sourceColumnIndex;
            this.TargetPropertyName = targetPropertyName;
        }
    }
}