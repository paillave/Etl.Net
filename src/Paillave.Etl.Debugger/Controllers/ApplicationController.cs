using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
        public ActionResult<JobDefinitionStructure> GetEstimatedExecutionPlan(string assemblyFilePath, string className, string @namespace, string streamTransformationName)
        {
            return new Inspector(assemblyFilePath).GetJobDefinitionStructure(className, @namespace, streamTransformationName);
        }

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<ExecutionStatus>> ExecuteProcess([FromQuery]string assemblyFilePath, [FromQuery]string className, [FromQuery]string @namespace, [FromQuery]string streamTransformationName, [FromBody]Dictionary<string, string> parameters)
        {
            return await new Inspector(assemblyFilePath).ExecuteAsync(className, @namespace, streamTransformationName, parameters, te =>
            {
                if (te.Content.Level == TraceLevel.Error)
                    Console.WriteLine(te.Content);
                this._applicationHubContext.Clients.All.PushTrace(te);
            });
        }
    }
}