using Paillave.Etl.Core;

namespace Paillave.Etl.GraphApi;

public class GraphApiOneDriveProviderProcessorAdapter : ProviderProcessorAdapterBase<GraphApiAdapterConnectionParameters, GraphApiOneDriveAdapterProviderParameters, GraphApiOneDriveAdapterProcessorParameters>
{
    public override string Description => "Get and save files on OneDrive or SharePoint via GraphAPI";
    public override string Name => "GraphApiOneDrive";

    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName,
        GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProviderParameters inputParameters)
        => new GraphApiOneDriveFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);

    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName,
        GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProcessorParameters outputParameters)
        => new GraphApiOneDriveFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}
