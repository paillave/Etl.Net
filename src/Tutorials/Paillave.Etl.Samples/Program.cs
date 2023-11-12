using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;

var executionOptions = new ExecutionOptions<string>
{
    Connectors = new FileValueConnectors()
        .Register(new FileSystemFileValueProvider("IN", "Input", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.docx"))
        .Register(new FileSystemFileValueProcessor("OUT1", "Output", Path.Combine(Environment.CurrentDirectory, "Output", "A")))
        .Register(new FileSystemFileValueProcessor("OUT2", "Output", Path.Combine(Environment.CurrentDirectory, "Output", "B"))),
};

var result = await StreamProcessRunner.CreateAndExecuteAsync("", triggerStream =>
{
    triggerStream.FromConnector("get input", "IN")
        .ToConnector("write output", "OUT1")
        .ToConnector("write output2", "OUT2");
}, executionOptions);
