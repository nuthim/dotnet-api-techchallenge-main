using Microsoft.Extensions.Logging;
using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Services;

public class ProductApiClient(IHttpClientFactory clientFactory, ILogger<ProductApiClient> logger, ITokenProvider tokenProvider)
    : ApiClient(clientFactory, logger, nameof(ProductApiClient)), IProductHttpClient
{
    public async Task<ApiResponse<Product[]>> GetProducts(CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        return await SendAsync<Product[]>($"products?token={token}", null, HttpMethod.Get, cancellationToken);
    }
}
