using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Paillave.Etl.Debugger.Coordinator;

namespace Paillave.Etl.Debugger.Hubs
{
    public class ApplicationHub : Hub<IHubClientContract>
    {
        private readonly ApplicationCoordinator _coordinator;

        public ApplicationHub(ApplicationCoordinator coordinator)
        {
            this._coordinator = coordinator;
        }

        public override async Task OnConnectedAsync()
        {
            _coordinator.SetHubClientProxy(this.Clients.All);
        }
    }
}