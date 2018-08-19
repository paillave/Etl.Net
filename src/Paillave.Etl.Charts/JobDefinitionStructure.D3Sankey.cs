using Newtonsoft.Json;
using Paillave.Etl.Charts;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public static partial class JobDefinitionStructureEx
    {
        public static D3SankeyDescription GetD3SankeyStructure(this JobDefinitionStructure jobDefinitionStructure)
        {
            var nameToIdDictionary = jobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.Name, Idx }).ToDictionary(i => i.Name, i => i.Idx);
            return new D3SankeyDescription
            {
                links = jobDefinitionStructure.StreamToNodeLinks.Select(link => new D3SankeyStatisticsLink
                {
                    source = nameToIdDictionary[link.SourceNodeName],
                    target = nameToIdDictionary[link.TargetNodeName],
                    value = 1
                }
                ).ToList(),
                nodes = jobDefinitionStructure.Nodes.Select(i => new D3SankeyStatisticsNode
                {
                    id = nameToIdDictionary[i.Name],
                    name = i.Name,
                    color = null
                }).ToList()
            };
        }
        public static string GetJsonD3SankeyStructure(this JobDefinitionStructure jobDefinitionStructure)
        {
            return JsonConvert.SerializeObject(jobDefinitionStructure.GetD3SankeyStructure());
        }
        public static string GetHtmlD3SankeyStructure(this JobDefinitionStructure jobDefinitionStructure)
        {
            var json = jobDefinitionStructure.GetJsonD3SankeyStructure();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.JobDefinitionStructure.D3Sankey.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<SANKEY_STATISTICS>>'", json);
            return html;
        }
        public static void OpenD3SankeyStructure(this JobDefinitionStructure jobDefinitionStructure)
        {
            Tools.OpenFile(jobDefinitionStructure.GetHtmlD3SankeyStructure(), "html");
        }
    }
}
