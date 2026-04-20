namespace ProductCatalog.Api.Domain.Contracts;

public interface IProductHttpClient : IApiClient
{
    Task<ApiResponse<Product.Product[]>> GetProducts(CancellationToken cancellationToken);
}
