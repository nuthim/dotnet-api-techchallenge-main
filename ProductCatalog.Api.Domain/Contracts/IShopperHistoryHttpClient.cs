using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Domain.Contracts;

public interface IShopperHistoryHttpClient : IApiClient
{
    Task<ApiResponse<List<ShopperHistory>>> GetShopperHistory(CancellationToken cancellationToken);
}
