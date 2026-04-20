using Microsoft.Extensions.Logging;
using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Services;

public class ShopperHistoryApiClient(IHttpClientFactory clientFactory, ILogger<ShopperHistoryApiClient> logger, ITokenProvider tokenProvider)
    : ApiClient(clientFactory, logger, nameof(ShopperHistoryApiClient)), IShopperHistoryHttpClient
{
    public async Task<ApiResponse<List<ShopperHistory>>> GetShopperHistory(CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        return await SendAsync<List<ShopperHistory>>($"shopperHistory?token={token}", null, HttpMethod.Get, cancellationToken);
    }
}
