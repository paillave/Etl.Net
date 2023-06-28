using System;

namespace Paillave.Etl.GraphApi.Provider.Writers;
internal static class MathWriterModule
{
    internal static void RegisterWriters(ODataExpressionConverterSettings settings)
    {
        settings.RegisterMethod(typeof(Math), nameof(Math.Floor));
        settings.RegisterMethod(typeof(Math), nameof(Math.Ceiling));
        settings.RegisterMethod(typeof(Math), nameof(Math.Round));
    }
}
