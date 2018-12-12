using System.Collections.Generic;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.Debugger.Coordinator;

namespace Paillave.Etl.Debugger.Hubs
{
    public interface IHubClientContract
    {
        Task PushTrace(TraceEvent trace);
        Task OnProcessList(IEnumerable<EltDescriptionSummary> enumerable);
    }
}