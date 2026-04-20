namespace ProductCatalog.Api.Domain.Product.Strategies;

public class SortByRecommended : ISortStrategy
{
    public SortOption Option => SortOption.Recommended;

    public IEnumerable<Product> Sort(List<Product> products, List<ShopperHistory> shopperHistory)
    {
        var productsOrderedByPopularity =
            from history in shopperHistory
            from order in history.Products
            group order by order.Name into grouped
            let totalOrdered = new
            {
                Count = grouped.Sum(p => p.Quantity),
                Product = products.FirstOrDefault(p => p.Name == grouped.Key)
            }
            where totalOrdered.Product is not null
            orderby totalOrdered.Count descending
            select totalOrdered.Product;

        var orderedProducts = productsOrderedByPopularity.ToList();
        var unorderedProducts = products.Except(orderedProducts);
        orderedProducts.AddRange(unorderedProducts);
        return orderedProducts;
    }
}
