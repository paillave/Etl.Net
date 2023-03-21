using System;
using System.IO;
using System.Text;
using Paillave.Etl.Core;
using Paillave.Etl.XmlFile;


var testXmlContent = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<CompactData
	xmlns=""http://www.SDMX.org/resources/SDMXML/schemas/v2_0/message""
	xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.SDMX.org/resources/SDMXML/schemas/v2_0/message https://stats.ecb.europa.eu/stats/vocabulary/sdmx/2.0/SDMXMessage.xsd"">
	<Header>
		<ID>EXR-HIST_2023-02-17</ID>
		<Test>false</Test>
		<Name xml:lang=""en"">Euro foreign exchange reference rates</Name>
		<Prepared>2023-02-17T15:55:10</Prepared>
		<Sender id=""4F0"">
			<Name xml:lang=""en"">European Central Bank</Name>
			<Contact>
				<Department xml:lang=""en"">DG Statistics</Department>
				<URI>mailto:statistics@ecb.europa.eu</URI>
			</Contact>
		</Sender>
		<DataSetAgency>ECB</DataSetAgency>
		<DataSetID>ECB_EXR_WEB</DataSetID>
		<Extracted>2023-02-17T15:55:10</Extracted>
	</Header>
	<DataSet
		xmlns=""http://www.ecb.europa.eu/vocabulary/stats/exr/1"" xsi:schemaLocation=""http://www.ecb.europa.eu/vocabulary/stats/exr/1 https://stats.ecb.europa.eu/stats/vocabulary/exr/1/2006-09-04/sdmx-compact.xsd"">
		<Group CURRENCY=""USD"" CURRENCY_DENOM=""EUR"" EXR_TYPE=""SP00"" EXR_SUFFIX=""A"" DECIMALS=""4"" UNIT=""USD"" UNIT_MULT=""0"" TITLE_COMPL=""ECB reference exchange rate, US dollar/Euro, 2:15 pm (C.E.T.)"" />
		<Series FREQ=""D"" CURRENCY=""USD"" CURRENCY_DENOM=""EUR"" EXR_TYPE=""SP00"" EXR_SUFFIX=""A"" TIME_FORMAT=""P1D"" COLLECTION=""A"">
			<Obs TIME_PERIOD=""1999-01-04"" OBS_VALUE=""1.1789"" OBS_STATUS=""A"" OBS_CONF=""F"" />
			<Obs TIME_PERIOD=""1999-01-05"" OBS_VALUE=""1.1790"" OBS_STATUS=""A"" OBS_CONF=""F"" />
			<Obs TIME_PERIOD=""1999-01-06"" OBS_VALUE=""1.1743"" OBS_STATUS=""A"" OBS_CONF=""F"" />
			<Obs TIME_PERIOD=""1999-01-07"" OBS_VALUE=""1.1632"" OBS_STATUS=""A"" OBS_CONF=""F"" />
			<Obs TIME_PERIOD=""2023-02-16"" OBS_VALUE=""1.0700"" OBS_STATUS=""A"" OBS_CONF=""F"" />
			<Obs TIME_PERIOD=""2023-02-17"" OBS_VALUE=""1.0625"" OBS_STATUS=""A"" OBS_CONF=""F"" />
		</Series>
	</DataSet>
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