using System;
using System.Linq;

namespace Paillave.Etl.GraphApi.Provider.Writers;
internal static class EnumWriterModule
{
    internal static void RegisterWriters(ODataExpressionConverterSettings settings)
    {
        settings.RegisterMethod(typeof(Enum), nameof(Enum.HasFlag), (opj, args) => $"{opj} has {args.Single()}");

        settings.RegisterValueWriter(new EnumValueWriter());
    }
}
