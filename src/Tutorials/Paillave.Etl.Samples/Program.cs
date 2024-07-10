using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.GraphApi;
var graphApiConnectionParameters = new GraphApiAdapterConnectionParameters
{
    ClientId = "c458617d-4a95-464f-bba5-b29e97578e61",
    ClientSecret = "T2U8Q~3AQc-Ul6IY.Fs8YBFL~n3AWXdp3AfCRcPR",
    TenantId = "50e16dbd-f56c-4e6e-a5ed-802e570a08c2",
    UserId = "b15b5ca6-43fd-48ef-bc2b-a56ef4ddd605"
};
var executionOptions = new ExecutionOptions<string>
{
    Connectors = new FileValueConnectors()
        .Register(new GraphApiMailFileValueProvider("GRAPHIN", "GraphIn", "GraphIn", graphApiConnectionParameters,
        new GraphApiMailAdapterProviderParameters
        {
            AttachmentNamePattern = "*",
            SetToReadIfBatchDeletion = true,
            Folder="Boîte de réception"
        }))
        .Register(new GraphApiMailFileValueProcessor("GRAPHOUT", "GraphOut", "GraphOut", graphApiConnectionParameters,
        new GraphApiAdapterProcessorParameters
        {
            Body = "Body",
            Subject = "Subject",
            From = "stephane.royer@fundprocess.lu",
            To = "stephane.royer@fundprocess.lu"
        }))
        .Register(new FileSystemFileValueProvider("IN", "Input", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*"))
        .Register(new FileSystemFileValueProcessor("OUT1", "Output", Path.Combine(Environment.CurrentDirectory, "Output")))
        .Register(new FileSystemFileValueProcessor("OUT2", "Output", Path.Combine(Environment.CurrentDirectory, "Output", "B"))),
};

var result = await StreamProcessRunner.CreateAndExecuteAsync("", triggerStream =>
{
    triggerStream.FromConnector("get input", "IN")
        // .ToConnector("write output", "S3OUT")
        .ToConnector("write output2", "GRAPHOUT");
}, executionOptions);
