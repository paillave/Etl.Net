using System;

namespace Paillave.Etl.TextFile.Core
{
    public class FlatFileLineDeserializeException : Exception
    {
        public int LineIndex { get; }
        public FlatFileLineDeserializeException(int lineIndex, Exception innerException) : base($"could not deserialize line #{lineIndex}", innerException)
        {
            this.LineIndex = lineIndex;
        }
    }
}