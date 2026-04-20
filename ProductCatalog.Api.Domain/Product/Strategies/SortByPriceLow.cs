namespace ProductCatalog.Api.Domain.Product.Strategies;

public class SortByPriceLow : ISortStrategy
{
    public SortOption Option => SortOption.Low;

    public IEnumerable<Product> Sort(List<Product> products, List<ShopperHistory> shopperHistory)
        => products.OrderBy(p => p.Price);
}
