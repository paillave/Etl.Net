using System.Threading.Tasks;

namespace Paillave.Etl.Debugger.Hubs
{
    public interface IEtlProcessDebugHubClient
    {
        Task PushTrace(string trace);
    }
}