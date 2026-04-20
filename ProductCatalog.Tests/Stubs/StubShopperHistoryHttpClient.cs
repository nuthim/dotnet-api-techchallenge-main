using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Tests.Stubs;

public class StubShopperHistoryHttpClient : IShopperHistoryHttpClient
{
    public List<ShopperHistory> ShopperHistories { get; set; } = [];

    public static StubShopperHistoryHttpClient WithHistory(List<ShopperHistory> shopperHistories)
    {
        return new StubShopperHistoryHttpClient
        {
            ShopperHistories = shopperHistories
        };
    }

    public static IShopperHistoryHttpClient WithNoHistory()
    {
        return new StubShopperHistoryHttpClient();
    }

    public Task<ApiResponse<List<ShopperHistory>>> GetShopperHistory(CancellationToken cancellationToken)
    {
        return Task.FromResult(new ApiResponse<List<ShopperHistory>>
        {
            IsSuccess = true,
            StatusCode = 200,
            Content = ShopperHistories
        });
    }
}
