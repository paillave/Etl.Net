using System;
using System.IO;

namespace Paillave.Etl.Core
{
    public interface IFileValue
    {
        string Name { get; }
        Stream GetContent();
        void Delete();
        Type MetadataType { get; }
        IFileValueMetadata Metadata { get; }
        string SourceType { get; }
    }
    public interface IFileValueMetadata
    {
        string Type { get; }
    }
}