namespace Paillave.Etl.Core
{
    public interface IStreamProcessObserver
    {
        int DebugChunkSize { get; set; }
        string JobName { get; }

        event DebugNodeStreamEventHandler DebugNodeStream;

        JobDefinitionStructure GetDefinitionStructure(IFileValueConnectors connectors = null);
    }
}