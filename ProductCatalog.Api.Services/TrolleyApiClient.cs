using Microsoft.Extensions.Logging;
using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Trolley;

namespace ProductCatalog.Api.Services;

public class TrolleyApiClient(IHttpClientFactory clientFactory, ILogger<TrolleyApiClient> logger, ITokenProvider tokenProvider)
    : ApiClient(clientFactory, logger, nameof(TrolleyApiClient)), ITrolleyHttpClient
{
    public async Task<ApiResponse<decimal>> CalculateTotal(TrolleyRequest request, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        return await SendAsync<decimal>($"trolleyCalculator?token={token}", request, HttpMethod.Post, cancellationToken);
    }
}
