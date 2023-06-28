using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Paillave.Etl.GraphApi;

public static class GraphApiAdapterConnectionParametersEx
{
    public static GraphServiceClient CreateGraphApiClient(this IGraphApiConnectionInfo connectionInfo)
    {
        var confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(connectionInfo.ClientId)
            .WithTenantId(connectionInfo.TenantId)
            .WithClientSecret(connectionInfo.ClientSecret)
            .Build();

        var graphClient = new GraphServiceClient(new CustomAuthenticationProvider(confidentialClientApplication));

        return graphClient;
    }
}
internal class CustomAuthenticationProvider : IAuthenticationProvider
{
    private readonly IConfidentialClientApplication _app;
    public CustomAuthenticationProvider(IConfidentialClientApplication app) => _app = app;
    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        var result = await _app.AcquireTokenForClient(new string[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
        request.Headers.Add("Authorization", new AuthenticationHeaderValue("Bearer", result.AccessToken).ToString());
    }
}
