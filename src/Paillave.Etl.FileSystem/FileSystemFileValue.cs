using Paillave.Etl.Core;
using System.IO;

namespace Paillave.Etl.FileSystem
{
    public class FileSystemFileValue(FileInfo fileInfo) : FileValueBase
    {
        public override string Name => fileInfo.Name;
        protected override void DeleteFile()
        {
            fileInfo.Delete();
        }
        public override Stream GetContent()
        {
            var stream = new MemoryStream();
            using (var fileStream = fileInfo.OpenRead())
                fileStream.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        public override StreamWithResource OpenContent() => new StreamWithResource(fileInfo.OpenRead());
    }
}
