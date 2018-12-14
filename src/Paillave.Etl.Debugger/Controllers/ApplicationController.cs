using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Debugger.Coordinator;
using Paillave.Etl.Debugger.Hubs;

namespace Paillave.Etl.Debugger.Controllers
{
    [Route("api/[controller]")]
    [Controller]
    public class Application
    {
        private readonly IHubContext<ApplicationHub, IHubClientContract> _applicationHubContext;

        public Application(IHubContext<ApplicationHub, IHubClientContract> applicationHubContext)
        {
            this._applicationHubContext = applicationHubContext;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(200)]
        public ActionResult<List<ProcessDescriptionSummary>> GetAssemblyProcesses(string assemblyPath)
        {
            return new Inspector(assemblyPath)
                .Processes
                .Select(i => i.Summary)
                .ToList();
        }

        [HttpGet("[action]")]
        [ProducesResponseType(200)]
        public ActionResult<D3SankeyDescription> GetEstimatedExecutionPlan(string assemblyFilePath, string className, string @namespace, string streamTransformationName)
        {
            var jobDefinitionStructure = new Inspector(assemblyFilePath).GetJobDefinitionStructure(className, @namespace, streamTransformationName);
            return GetEstimatedExecutionPlanD3Sankey(jobDefinitionStructure);
        }

        private D3SankeyDescription GetEstimatedExecutionPlanD3Sankey(JobDefinitionStructure jobDefinitionStructure)
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

    }
}