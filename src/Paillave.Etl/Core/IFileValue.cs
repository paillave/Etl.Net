using System;
using System.IO;

namespace Paillave.Etl.Core
{
    public interface IFileValue
    {
        string Name { get; }
        Stream Get(bool useStreamCopy = false);
        Stream GetContent();
        Stream OpenContent();
        void Delete();
        Type MetadataType { get; }
        IFileValueMetadata Metadata { get; }
        string SourceType { get; }
    }
    public interface IFileValueMetadata
    {
        string Type { get; }
        string? ConnectorCode { get; }
        string? ConnectionName { get; }
        string? ConnectorName { get; }
    }
}