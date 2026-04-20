namespace ProductCatalog.Api.Domain.Product;

public record ShopperHistory(string CustomerId, List<Product> Products);
