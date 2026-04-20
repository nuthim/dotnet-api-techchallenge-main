using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Services;

public class ProductService(
    IProductHttpClient productClient,
    IShopperHistoryHttpClient shopperClient,
    ISortStrategyResolver sortResolver) : IProductService
{
    public async Task<List<Product>> GetSortedProducts(SortOption sortOption, CancellationToken cancellationToken)
    {
        var productsResponse = await productClient.GetProducts(cancellationToken);
        if (!productsResponse.IsSuccess)
            throw new HttpRequestException(productsResponse.Problem?.Detail ?? "Failed to fetch products");

        var history = new List<ShopperHistory>();
        if (sortOption == SortOption.Recommended)
        {
            var historyResponse = await shopperClient.GetShopperHistory(cancellationToken);
            if (!historyResponse.IsSuccess)
                throw new HttpRequestException(historyResponse.Problem?.Detail ?? "Failed to fetch shopper history");

            history = historyResponse.Content ?? [];
        }

        var strategy = sortResolver.Resolve(sortOption);
        return [.. strategy.Sort(productsResponse.Content?.ToList() ?? [], history)];
    }
}
