namespace ProductCatalog.Api.Domain.Product.Strategies;

public class SortByNameDescending : ISortStrategy
{
    public SortOption Option => SortOption.Descending;

    public IEnumerable<Product> Sort(List<Product> products, List<ShopperHistory> shopperHistory)
        => products.OrderByDescending(p => p.Name);
}
