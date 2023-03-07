// using System;
// using Paillave.Etl.Core;
// using Paillave.Etl.Mail;

// var connectors = new FileValueConnectors().Register(new MailFileValueProvider("IN", "Input", "MailConnection",
//     new MailAdapterConnectionParameters
//     {
//         Server = "outlook.office365.com",
//         PortNumber = 993,
//         Login = "**********",
//         Password = "*********",
//         Ssl = true,
//         Tls = true,
//     }, new MailAdapterProviderParameters
//     {
//         SetToReadIfBatchDeletion = true,
//         OnlyNotRead = true,
//         AttachmentNamePattern = "*.pdf",
//         FromContains = "tv",
//         SubjectContains = "Database",
//         Folder = "Work"
//     }));


// var res = await StreamProcessRunner.CreateAndExecuteAsync("dummy", baseStream =>
// {
//     baseStream
//         .FromConnector("get files", "IN")
//         .Do("write file name", i =>
//         {
//             Console.WriteLine(i.Name);
//             Console.WriteLine(i.Get().Length);
//         })
//         .Do("delete", i => i.Delete());
// }, new ExecutionOptions<string>
// {
//     Connectors = connectors
// });
// Console.WriteLine(res.Failed ? $"fail: {res.ErrorTraceEvent}" : "Success");
