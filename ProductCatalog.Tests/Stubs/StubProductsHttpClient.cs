using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Tests.Stubs;

public class StubProductsHttpClient : IProductHttpClient
{
    private Product[] ProductsToReturn { get; set; } = [];

    public static StubProductsHttpClient WithProducts(List<Product> productsToReturn)
    {
        return new StubProductsHttpClient
        {
            ProductsToReturn = [.. productsToReturn]
        };
    }

    public static StubProductsHttpClient WithEmptyProducts()
    {
        return new StubProductsHttpClient
        {
            ProductsToReturn = []
        };
    }

    public Task<ApiResponse<Product[]>> GetProducts(CancellationToken cancellationToken)
    {
        return Task.FromResult(new ApiResponse<Product[]>
        {
            IsSuccess = true,
            StatusCode = 200,
            Content = ProductsToReturn
        });
    }
}
