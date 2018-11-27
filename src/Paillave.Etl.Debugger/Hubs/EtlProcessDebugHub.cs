using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Paillave.Etl.Debugger.Hubs
{
    public class EtlProcessDebugHub : Hub<IEtlProcessDebugHubClient>
    {
        private readonly EtlTraceDispatcher _dispatcher;

        public EtlProcessDebugHub(EtlTraceDispatcher dispatcher)
        {
            this._dispatcher = dispatcher;
        }
        public async Task Start()
        {
            await _dispatcher.Start();
        }
    }
}