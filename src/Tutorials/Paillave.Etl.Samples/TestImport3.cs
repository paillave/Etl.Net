using Paillave.Etl.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.TextFile;

namespace Paillave.Etl.Samples;

public static class TestImport3
{
    public static void Import(ISingleStream<string[]> contextStream)
    {
        var portfolioFileStream = contextStream
            .FromConnector("Get portfolio files", "PTF")
            .CrossApplyTextFile("Parse portfolio file", o => o
                .UseMap(i => new DataAccess.SimpleTable { Name = i.ToColumn("SicavName") }).IsColumnSeparated(','));
        var sicavStream = portfolioFileStream
            .Distinct("Distinct sicav", i => i.Name, true)
            .EfCoreSave("Save sicav", o => o.WithMode(SaveMode.EntityFrameworkCore));
    }
}
