using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace Paillave.Etl.Debugger.Hubs
{
    public interface IApplicationClientHub
    {
        Task PushTrace(TraceEvent trace);
    }
}