using System.Collections.Generic;
using System.IO;

namespace Paillave.Etl.Core;

public abstract class FileValueBase : IFileValue
{
    private readonly object _lock = new();
    private bool _isDeleted = false;
    public abstract string Name { get; }
    public object? Metadata { get; set; }
    public Dictionary<string, IEnumerable<Destination>>? Destinations { get; set; }

    public void Delete()
    {
        lock (_lock)
        {
            if (!_isDeleted)
            {
                this.DeleteFile();
                _isDeleted = true;
            }
        }
    }

    protected abstract void DeleteFile();
    public abstract Stream GetContent();
    public abstract StreamWithResource OpenContent();

    public StreamWithResource Get(bool useStreamCopy = true)
    {
        if (!useStreamCopy)
            return OpenContent();

        var stream = new StreamWithResource(GetContent());
        stream.Position = 0;
        return stream;
    }

    // public abstract string GetSerialization();
}
