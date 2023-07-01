using System;

namespace Paillave.Etl.GraphApi.Provider.Writers;
internal static class DateWriterModule
{
    internal static void RegisterWriters(ODataExpressionConverterSettings settings)
    {
        settings.RegisterMember<DateTime>(nameof(DateTime.Date));
        settings.RegisterMember<DateTime>(nameof(DateTime.Day));
        settings.RegisterMember<DateTime>(nameof(DateTime.Year));
        settings.RegisterMember<DateTime>(nameof(DateTime.Hour));
        settings.RegisterMember<DateTime>(nameof(DateTime.MaxValue), "maxdatetime");
        settings.RegisterMember<DateTime>(nameof(DateTime.Minute));
        settings.RegisterMember<DateTime>(nameof(DateTime.MinValue), "mindatetime");
        settings.RegisterMember<DateTime>(nameof(DateTime.Month));
        settings.RegisterMember<DateTime>(nameof(DateTime.Now));
        settings.RegisterMember<DateTime>(nameof(DateTime.Second));
    }
}
