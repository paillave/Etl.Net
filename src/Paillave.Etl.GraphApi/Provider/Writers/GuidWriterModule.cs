namespace Paillave.Etl.GraphApi.Provider.Writers;
internal static class GuidWriterModule
{
    internal static void RegisterWriters(ODataExpressionConverterSettings settings)
    {
        settings.RegisterValueWriter(new GuidValueWriter());
    }
}