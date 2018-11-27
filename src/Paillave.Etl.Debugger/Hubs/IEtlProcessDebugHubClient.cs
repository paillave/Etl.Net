using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace Paillave.Etl.Debugger.Hubs
{
    public interface IEtlProcessDebugHubClient
    {
        Task PushTrace(TraceEvent trace);
    }
}