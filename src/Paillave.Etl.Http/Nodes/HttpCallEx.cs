using System.Net.Http;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public static partial class HttpCallEx
{
    public static IStream<HttpResponseMessage> HttpRestSelect(
        this ISingleStream<HttpAdapterConnectionParameters> connectionStream,
        ISingleStream<HttpAdapterParametersBase> adapterStream,
        string name
    )
    {
        return new HttpRestSelectStreamNode(
            name,
            new HttpCallArgs
            {
                ConnectionParameters = connectionStream.Observable.FirstAsync().Wait(),
                AdapterParameters = adapterStream.Observable.FirstAsync().Wait(),
            }
        ).Output;
    }

    public static IStream<HttpResponseMessage> HttpRestSave(
        this ISingleStream<HttpAdapterConnectionParameters> connectionStream,
        ISingleStream<HttpAdapterParametersBase> adapterStream,
        string name
    )
    {
        return new HttpRestSaveStreamNode(
            name,
            new HttpCallArgs
            {
                ConnectionParameters = connectionStream.Observable.FirstAsync().Wait(),
                AdapterParameters = adapterStream.Observable.FirstAsync().Wait(),
            }
        ).Output;
    }
}
