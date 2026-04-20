using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Domain.Contracts;

public interface IProductService
{
    Task<List<Product.Product>> GetSortedProducts(SortOption sortOption, CancellationToken cancellationToken);
}
