namespace ProductCatalog.Api.Domain.Trolley;

public record TrolleyRequest(
    List<TrolleyProduct> Products,
    List<Special> Specials,
    List<ProductQuantity> Quantities);

public record TrolleyProduct(string Name, decimal Price);

public record Special(List<ProductQuantity> Quantities, decimal Total);

public record ProductQuantity(string Name, int Quantity);
