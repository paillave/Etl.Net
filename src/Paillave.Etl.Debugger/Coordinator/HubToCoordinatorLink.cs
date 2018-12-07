using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using Paillave.Etl.Core;
using Paillave.Etl.Debugger.Hubs;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Debugger.Coordinator
{
    public class HubToCoordinatorLink
    {
        private readonly IHubContext<ApplicationHub, IApplicationClientHub> _applicationHubContext;

        public HubToCoordinatorLink(IHubContext<ApplicationHub, IApplicationClientHub> applicationHubContext)
        {
            this._applicationHubContext = applicationHubContext;
        }
    }
}