namespace ProductCatalog.Api.Domain.Product.Strategies;

public class SortByPriceHigh : ISortStrategy
{
    public SortOption Option => SortOption.High;

    public IEnumerable<Product> Sort(List<Product> products, List<ShopperHistory> shopperHistory)
        => products.OrderByDescending(p => p.Price);
}
