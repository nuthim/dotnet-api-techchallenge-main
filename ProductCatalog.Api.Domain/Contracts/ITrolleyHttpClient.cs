using ProductCatalog.Api.Domain.Trolley;

namespace ProductCatalog.Api.Domain.Contracts;

public interface ITrolleyHttpClient : IApiClient
{
    Task<ApiResponse<decimal>> CalculateTotal(TrolleyRequest request, CancellationToken cancellationToken);
}
