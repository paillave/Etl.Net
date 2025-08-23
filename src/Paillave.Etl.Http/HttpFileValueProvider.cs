using System;
using System.Net.Http;
using System.Threading;
using Fluid;
using Paillave.Etl.Core;

namespace Paillave.Etl.Http;

public class HttpFileValueProvider(
    string code,
    string name,
    string connectionName,
    HttpAdapterConnectionParameters connectionParameters,
    HttpAdapterProviderParameters providerParameters
    )
        : FileValueProviderBase<HttpAdapterConnectionParameters, HttpAdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    public override IFileValue Provide(string name, string fileSpecific)
    {
        return new HttpFileValue(
                fileSpecific,
                connectionParameters,
                providerParameters);
    }

    protected override void Provide(object input,
        Action<IFileValue, FileReference> pushFileValue,
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters,
        CancellationToken cancellationToken
    )
    {
        var url = HttpFileValueProvider.FluidMergeAsync(connectionParameters.Url, input);
        var fileValue = new HttpFileValue(
                url,
                connectionParameters,
                providerParameters
            );
        var fileReference = new FileReference(fileValue.Name, this.Code, url);
        pushFileValue(fileValue, fileReference);
    }
    private static string FluidMergeAsync(string template, object? data)
    {
        var parser = new FluidParser();
        var context = new TemplateContext(data ?? new(), new TemplateOptions
        {
            ModelNamesComparer = StringComparer.InvariantCultureIgnoreCase,
            MemberAccessStrategy = new UnsafeMemberAccessStrategy()
        }, true);
        if (parser.TryParse(template, out var fTemplate, out var error))
        {
            return fTemplate.Render(context);
        }
        else
        {
            throw new Exception($"Error: {error}");
        }
    }

    protected override void Test(
        HttpAdapterConnectionParameters connectionParameters,
        HttpAdapterProviderParameters providerParameters)
    {
        using var httpClient = new HttpClient();
        HttpHelpers.GetResponse(connectionParameters, providerParameters, httpClient).Wait();
    }
}
