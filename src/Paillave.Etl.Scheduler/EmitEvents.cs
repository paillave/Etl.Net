using Paillave.Etl.Core;
using Paillave.Scheduler;
namespace Paillave.Etl.Scheduler;

public static partial class CustomEx
{
    public static IStream<TOut> EmitEvents<TOut, TKey>(this ISingleStream<object> stream, string name, ITickSourceConnection<TOut, TKey> tickSourceConnection) where TKey : IEquatable<TKey>
        => stream.CrossApply(name, new TicksProvider<TOut, TKey>(tickSourceConnection));
}
internal class TicksProvider<TOut, TKey> : IValuesProvider<object, TOut> where TKey : IEquatable<TKey>
{
    private readonly ITickSourceConnection<TOut, TKey> _tickSourceConnection;
    public TicksProvider(ITickSourceConnection<TOut, TKey> tickSourceConnection)
        => _tickSourceConnection = tickSourceConnection;
    public string TypeName => nameof(TicksProvider<TOut, TKey>);
    public ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    public void PushValues(object input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
    {
        using (EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
        using (var tickSourceManager = TickSourceManager.Create(this._tickSourceConnection))
        {
            tickSourceManager.Tick += (sender, e) => push(e);
            _tickSourceConnection.Stopped += (sender, e) => waitHandle.Set();
            waitHandle.WaitOne();
        }
    }
}