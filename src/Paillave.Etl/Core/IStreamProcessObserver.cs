namespace Paillave.Etl.Core;

public interface IStreamProcessObserver
{
    int DebugChunkSize { get; set; }

    event DebugNodeStreamEventHandler DebugNodeStream;

    JobDefinitionStructure GetDefinitionStructure();
}