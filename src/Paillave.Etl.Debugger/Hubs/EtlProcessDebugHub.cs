using System;
using Microsoft.AspNetCore.SignalR;

namespace Paillave.Etl.Debugger.Hubs
{
    public class EtlProcessDebugHub : Hub<IEtlProcessDebugHubClient>
    {
        private IDisposable _listen;
        public EtlProcessDebugHub(NewClass cls)
        {
            _listen = cls.Listen(i => this.Clients.Caller.PushTrace(i));
        }
        protected override void Dispose(bool disposing)
        {
            _listen.Dispose();
        }
        // public EtlProcessDebugHub()
        // {
        // }
    }
}