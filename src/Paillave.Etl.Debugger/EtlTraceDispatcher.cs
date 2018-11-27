using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Paillave.Etl.Debugger.Hubs;

namespace Paillave.Etl.Debugger
{
    public class EtlTraceDispatcher
    {
        private readonly IHubContext<EtlProcessDebugHub, IEtlProcessDebugHubClient> _chatHubContext;
        private readonly NewClass _cls;

        public EtlTraceDispatcher(IHubContext<EtlProcessDebugHub, IEtlProcessDebugHubClient> chatHubContext, NewClass cls)
        {
            this._chatHubContext = chatHubContext;
            this._cls = cls;
            cls.Listen(trace =>
            {
                chatHubContext.Clients.All.PushTrace(trace);
            });
        }
        public Task Start()
        {
            return Task.Run(() => _cls.Start());
        }
    }
}