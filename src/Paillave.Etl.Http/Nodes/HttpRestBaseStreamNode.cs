using System.Net.Http;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public abstract class HttpRestBaseStreamNode
    : StreamNodeBase<HttpResponseMessage, IStream<HttpResponseMessage>, HttpCallArgs>
{
    public HttpRestBaseStreamNode(string name, HttpCallArgs args)
        : base(name, args) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    // protected abstract override IStream<HttpResponseMessage> CreateOutputStream(HttpCallArgs args);
}
