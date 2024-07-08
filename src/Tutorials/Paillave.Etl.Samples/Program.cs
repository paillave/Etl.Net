using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.S3;

var executionOptions = new ExecutionOptions<string>
{
    Connectors = new FileValueConnectors()
        .Register(new FileSystemFileValueProvider("IN", "Input", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.docx"))
        .Register(new FileSystemFileValueProcessor("OUT1", "Output", Path.Combine(Environment.CurrentDirectory, "Output")))
        .Register(new FileSystemFileValueProcessor("OUT2", "Output", Path.Combine(Environment.CurrentDirectory, "Output", "B"))),
};

var result = await StreamProcessRunner.CreateAndExecuteAsync("", triggerStream =>
{
    triggerStream.FromConnector("get input", "S3IN")
        // .ToConnector("write output", "S3OUT")
        .ToConnector("write output2", "OUT1");
}, executionOptions);
