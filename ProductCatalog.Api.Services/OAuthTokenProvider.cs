using System.Net.Http.Json;
using ProductCatalog.Api.Domain.Contracts;

namespace ProductCatalog.Api.Services;

/// <summary>
/// OAuth2 client_credentials token provider for service-to-service authentication.
/// Caches the access token and re-authenticates when it expires.
/// Uses a dedicated named HttpClient ("AuthServer") without TokenDelegatingHandler
/// to avoid circular dependency.
/// </summary>
public class OAuthTokenProvider(IHttpClientFactory clientFactory, TimeProvider timeProvider) : ITokenProvider
{
    private string? _accessToken;
    private DateTimeOffset _expiresAt;

    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_accessToken is not null && timeProvider.GetUtcNow() < _expiresAt.AddSeconds(-30))
            return _accessToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_accessToken is not null && timeProvider.GetUtcNow() < _expiresAt.AddSeconds(-30))
                return _accessToken;

            var client = clientFactory.CreateClient("AuthServer");
            var request = new HttpRequestMessage(HttpMethod.Post, "oauth/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = "configured-client-id",
                    ["client_secret"] = "configured-client-secret"
                })
            };

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize token response");

            _accessToken = tokenResponse.AccessToken;
            _expiresAt = timeProvider.GetUtcNow().AddSeconds(tokenResponse.ExpiresIn);

            return _accessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private record TokenResponse(string AccessToken, int ExpiresIn);
}
