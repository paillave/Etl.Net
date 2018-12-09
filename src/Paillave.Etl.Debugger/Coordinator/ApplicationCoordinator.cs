using System;
using System.Linq;
using Paillave.Etl.Debugger.Hubs;

namespace Paillave.Etl.Debugger.Coordinator
{
    public class ApplicationCoordinator
    {
        private IHubClientContract _hubClientProxy = null;
        public void SetHubClientProxy(IHubClientContract hubClientProxy)
        {
            _hubClientProxy = hubClientProxy;
        }
        public void SetAssembly(string assemblyPath)
        {
            var etls = new Inspector().GetEtlList(assemblyPath);
            _hubClientProxy.OnNewEtlList(etls.Select(i => i.Summary).ToList());
        }
    }
}