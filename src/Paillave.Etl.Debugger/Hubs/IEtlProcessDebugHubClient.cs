namespace Paillave.Etl.Debugger.Hubs
{
    public interface IEtlProcessDebugHubClient
    {
        void PushTrace(string trace);
    }
}