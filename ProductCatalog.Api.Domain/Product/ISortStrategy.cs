namespace ProductCatalog.Api.Domain.Product;

public interface ISortStrategy
{
    SortOption Option { get; }

    IEnumerable<Product> Sort(List<Product> products, List<ShopperHistory> shopperHistory);
}
