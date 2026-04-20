using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Services;

public class SortStrategyResolver(IEnumerable<ISortStrategy> strategies) : ISortStrategyResolver
{
    private readonly Dictionary<SortOption, ISortStrategy> _map =
        strategies.ToDictionary(s => s.Option);

    public ISortStrategy Resolve(SortOption option) =>
        _map.TryGetValue(option, out var strategy)
            ? strategy
            : throw new ArgumentOutOfRangeException(nameof(option), option, "No strategy registered for this sort option");
}
