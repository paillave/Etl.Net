using System;
using System.IO;
using System.Text;
using Paillave.Etl.Core;
using Paillave.Etl.XmlFile;


var testXmlContent = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
</CompactData>";
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
processRunner.DebugNodeStream += (sender, e) =>
{

};
var res = await processRunner.ExecuteAsync("dummy");
Console.WriteLine(res.Failed ? $"fail: {res.ErrorTraceEvent}" : "Success");





// class fileDefinition : XmlFileDefinition
// {
//     public fileDefinition()
//     {
//         this.AddNodeDefinition(XmlNodeDefinition.Create("obs", "/CompactData/DataSet/Series/Obs", i => new Obs
//         {
//             Currency = i.ToXPathQuery<string>("/CompactData/DataSet/Group/@CURRENCY"),
//             TimePeriod = i.ToXPathQuery<DateTime>("/CompactData/DataSet/Series/Obs/@TIME_PERIOD"),
//             ObsValue = i.ToXPathQuery<double>("/CompactData/DataSet/Series/Obs/@OBS_VALUE"),
//             FileName = i.ToSourceName(),
//         }));
//     }
// }
// var nodesStream = FileStream.CrossApplyXmlFile($"{TaskName}: parse input file", new fileDefinition()); var fileStream = nodesStream.XmlNodeOfType<Obs>($"{TaskName}: list only Obs");    //.SetForCorrelation($"{TaskName}: correlate");










void DefineProcess(ISingleStream<string> contextStream)
{
    contextStream
        .Select("create in memory file with content for test", _ => FileValue.Create(new MemoryStream(Encoding.UTF8.GetBytes(testXmlContent)), "example.xml", "testContent"))
        .CrossApplyXmlFile("parse xml", o => o
            .AddNodeDefinition("elt1", "/CompactData/DataSet/Series/Obs", i => new Obs
            {
                Currency = i.ToXPathQuery<string>("/CompactData/DataSet/Group/@CURRENCY"),
                TimePeriod = i.ToXPathQuery<DateTime>("/CompactData/DataSet/Series/Obs/@TIME_PERIOD"),
                ObsValue = i.ToXPathQuery<double>("/CompactData/DataSet/Series/Obs/@OBS_VALUE"),
                FileName = i.ToSourceName(),
            }))
        .XmlNodeOfType<Obs>("only obs");
}


class Obs
{
    public string Currency { get; set; }
    public DateTime TimePeriod { get; set; }
    public double ObsValue { get; set; }
    public string FileName { get; set; }
}

// var xmlNodes = contextStream
//     .Select("create in memory file with content for test", _ => FileValue.Create(new MemoryStream(Encoding.UTF8.GetBytes(testXmlContent)), "example.xml", "testContent"))
//     .CrossApplyXmlFile("parse xml", o => o
//         .AddNodeDefinition("elt1", "/root/elt1", i => new Elt1Node
//         {
//             V1 = i.ToXPathQuery<string>("/root/elt1/@v1"),
//             V2 = i.ToXPathQuery<string>("/root/elt1/v2"),
//         })
//         .AddNodeDefinition("elt2", "/root/elt1/cols/col", i => new ColNode
//         {
//             V1 = i.ToXPathQuery<string>("/root/elt1/@v1"),
//             Item = i.ToXPathQuery<string>("/root/elt1/cols/col/item"),
//         }));
// var parentStream = xmlNodes.XmlNodeOfType<Elt1Node>("only Etl1").Do("write elt1", i => Console.WriteLine($"Node type 1 : {i.V1} - {i.V2}"));
// var childStream = xmlNodes.XmlNodeOfType<ColNode>("only Etl2").Do("write elt2", i => Console.WriteLine($"Node type 2 : {i.V1} - {i.Item}"));
// you can join both streams using V1 key. Later on, CrossApplyXmlFile will issue a list of parents node in the proper descendence order with the current node.
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