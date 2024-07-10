using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.GraphApi;
var graphApiConnectionParameters = new GraphApiAdapterConnectionParameters
{
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
