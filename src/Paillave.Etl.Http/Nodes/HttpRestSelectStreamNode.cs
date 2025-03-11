using System.Net.Http;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Http;

public class HttpRestSelectStreamNode : HttpRestBaseStreamNode
{
    public HttpRestSelectStreamNode(string name, HttpCallArgs args)
        : base(name, args) { }

    protected override IStream<HttpResponseMessage> CreateOutputStream(HttpCallArgs args)
    {
        var httpClient = HttpClientFactory.CreateClient(
            args.ConnectionParameters,
            args.AdapterParameters
        );

        var response = Helpers
            .GetResponse(args.ConnectionParameters, args.AdapterParameters, httpClient)
            .Result;

        var waitHandle = new ManualResetEvent(false); // FIXME: to be get from ETL context?
        var cancellationToken = new CancellationToken(); // FIXME: to be get from ETL context?

        var observable = PushObservable.FromSingle(response, waitHandle, cancellationToken);

        return base.CreateUnsortedStream(observable);
    }
}
