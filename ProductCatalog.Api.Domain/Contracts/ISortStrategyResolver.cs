using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Domain.Contracts;

public interface ISortStrategyResolver
{
    ISortStrategy Resolve(SortOption option);
}
