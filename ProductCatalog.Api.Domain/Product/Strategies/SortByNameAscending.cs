namespace ProductCatalog.Api.Domain.Product.Strategies;

public class SortByNameAscending : ISortStrategy
{
    public SortOption Option => SortOption.Ascending;

    public IEnumerable<Product> Sort(List<Product> products, List<ShopperHistory> shopperHistory)
        => products.OrderBy(p => p.Name);
}
