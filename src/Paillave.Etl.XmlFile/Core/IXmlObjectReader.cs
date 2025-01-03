using System;
using System.IO;
using System.Threading;

namespace Paillave.Etl.XmlFile.Core;

public interface IXmlObjectReader
{
    void Read(Stream fileStream, CancellationToken cancellationToken);
}
