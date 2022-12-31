// using System;
// using System.IO;
// using System.Text;
// using Paillave.Etl.Core;
// using Paillave.Etl.XmlFile;


// var testXmlContent = @"<root>
//     <elt1 v1=""qwe"">
//         <v2>asd</v2>
//         <cols>
//             <col>
//                 <item>hello</item>
//             </col>
//             <col>
//                 <item>world</item>
//             </col>
//          </cols>
//     </elt1>
// </root>";

// var res = await StreamProcessRunner.CreateAndExecuteAsync("dummy", DefineProcess);
// Console.WriteLine(res.Failed ? $"fail: {res.ErrorTraceEvent}" : "Success");

// void DefineProcess(ISingleStream<string> contextStream)
// {
//     var xmlNodes = contextStream
//         .Select("create in memory file with content for test", _ => FileValue.Create(new MemoryStream(Encoding.UTF8.GetBytes(testXmlContent)), "example.xml", "testContent"))
//         .CrossApplyXmlFile("parse xml", o => o
//             .AddNodeDefinition("elt1", "/root/elt1", i => new Elt1Node
//             {
//                 V1 = i.ToXPathQuery<string>("/root/elt1/@v1"),
//                 V2 = i.ToXPathQuery<string>("/root/elt1/v2"),
//             })
//             .AddNodeDefinition("elt2", "/root/elt1/cols/col", i => new ColNode
//             {
//                 V1 = i.ToXPathQuery<string>("/root/elt1/@v1"),
//                 Item = i.ToXPathQuery<string>("/root/elt1/cols/col/item"),
//             }));
//     var parentStream = xmlNodes.XmlNodeOfType<Elt1Node>("only Etl1").Do("write elt1", i => Console.WriteLine($"Node type 1 : {i.V1} - {i.V2}"));
//     var childStream = xmlNodes.XmlNodeOfType<ColNode>("only Etl2").Do("write elt2", i => Console.WriteLine($"Node type 2 : {i.V1} - {i.Item}"));
//     // you can join both streams using V1 key. Later on, CrossApplyXmlFile will issue a list of parents node in the proper descendence order with the current node.
// }

// class Elt1Node
// {
//     public string V1 { get; set; }
//     public string V2 { get; set; }
// }
// class ColNode
// {
//     public string V1 { get; set; }
//     public string Item { get; set; }
// }