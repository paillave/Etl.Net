using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Paillave.Etl.Debugger.Coordinator;

namespace Paillave.Etl.Debugger.Hubs
{
    public class ApplicationHub : Hub<IHubClientContract>
    {
    }
}