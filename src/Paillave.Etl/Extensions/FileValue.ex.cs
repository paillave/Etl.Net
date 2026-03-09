using System;

namespace Paillave.Etl.Core;

public static partial class FileValueEx
{
    public static IStream<TIn> DeleteSourceFile<TIn>(this IStream<TIn> stream, string name, bool noParallelisation = false) where TIn : IFileValue
        => stream.Do(name, i => i.Delete());
}
