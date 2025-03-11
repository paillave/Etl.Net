using System.Net.Http;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Http;

public class HttpRestStreamNode
    : StreamNodeBase<HttpResponseMessage, IStream<HttpResponseMessage>, HttpArgs>
{
    public HttpRestStreamNode(string name, HttpArgs args)
        : base(name, args) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    protected override IStream<HttpResponseMessage> CreateOutputStream(HttpArgs args)
    {
        // Map each HttpCallArgs into an HttpResponseMessage using Select/Map
        var outputObservable = args.Stream.Observable.Map(httpCallArgs =>
        {
            // Create the client for each call (or reuse a singleton if possible)
            var httpClient = HttpClientFactory.CreateClient(
                httpCallArgs.ConnectionParameters,
                httpCallArgs.AdapterParameters
            );

            // Execute the HTTP request synchronously (or async with async-friendly pattern)
            var response = Helpers
                .GetResponse(
                    httpCallArgs.ConnectionParameters,
                    httpCallArgs.AdapterParameters,
                    httpClient
                )
                .Result;

            return response;
        });

        return base.CreateUnsortedStream(outputObservable);
    }
}

public class HttpArgs
{
    public required IStream<HttpCallArgs> Stream { get; set; }
}
