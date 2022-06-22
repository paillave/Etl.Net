namespace Paillave.Etl.Scheduler;

public interface ITickEmitterManager<TEmitter, TKey> where TKey : IEquatable<TKey>
{
    void RemoveEmitter(TKey emitterKey);
    void ResetEmitter(TEmitter emitter);
    void ResetEmitters(IEnumerable<TEmitter> newEmitters);
}
