using Paillave.Etl.Core;

namespace Paillave.Etl.Pgp;

public class PgpAdapterConnectionParameters
{
}

public class PgpProviderProcessorAdapter : ProviderProcessorAdapterBase<PgpAdapterConnectionParameters, object, PgpAdapterProcessorParameters>
{
    public override string Description => "Handle zip files";
    public override string Name => "Zip";
    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, PgpAdapterConnectionParameters connectionParameters, object inputParameters)
        => null;
    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, PgpAdapterConnectionParameters connectionParameters, PgpAdapterProcessorParameters outputParameters)
        => new PgpFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}
