using System.Net.Http.Headers;
using ProductCatalog.Api.Domain.Contracts;

namespace ProductCatalog.Api.Services;

/// <summary>
/// Injects the bearer token into the Authorization header for all outgoing requests.
/// Tokens should travel in headers over TLS, not in query params.
/// </summary>
public class TokenDelegatingHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
