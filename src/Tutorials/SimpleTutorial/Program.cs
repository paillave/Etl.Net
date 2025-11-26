using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.GraphApi;

var connectors = new FileValueConnectors().Register(new GraphApiMailFileValueProvider("IN", "Input", "MailConnection",
    new GraphApiAdapterConnectionParameters
    {
    }, new GraphApiMailAdapterProviderParameters
    {
        SetToReadIfBatchDeletion = true,
        OnlyNotRead = true,
        AttachmentNamePattern = "*.pdf",
        // FromContains = "tv",
        // SubjectContains = "Database",
        Folder = "Boîte de réception"
    }));

var res = await StreamProcessRunner.CreateAndExecuteAsync("dummy", baseStream =>
{
    baseStream
        .FromConnector("get files", "IN")
        .Do("write file name", i =>
        {
            Console.WriteLine(i.Name);
            using var fs = new FileStream($"att/{i.Name}", FileMode.Create);
            i.Get().CopyTo(fs);
            // Console.WriteLine(i.Get().Length);
        })
        .Do("delete", i => i.Delete())
        ;
}, new ExecutionOptions<string>
{
    Services = new ServiceCollection()
        .AddSingleton<IFileValueConnectors>(connectors)
        .BuildServiceProvider(),
});
Console.WriteLine(res.Failed ? $"fail: {res.ErrorTraceEvent}" : "Success");
