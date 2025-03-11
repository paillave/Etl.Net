using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpRestSaveStreamNode : HttpRestBaseStreamNode
{
    public HttpRestSaveStreamNode(string name, HttpCallArgs args)
        : base(name, args) { }

    protected override IStream<HttpResponseMessage> CreateOutputStream(HttpCallArgs args)
    {
        var observable = Observable.Defer(async () =>
        {
            using (var httpClient = new HttpClient())
            {
                // var response = await httpClient.GetAsync(url);
                var httpClient = HttpClientFactory.CreateClient(
                    args.ConnectionParameters,
                    args.AdapterParameters
                );
                var response = Helpers
                    .GetResponse(args.ConnectionParameters, args.AdapterParameters, httpClient)
                    .Result;

                return response;
            }
        });

        return base.CreateUnsortedStream(observable);
    }
}
