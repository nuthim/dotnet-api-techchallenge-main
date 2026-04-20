using ProductCatalog.Api.Domain.Contracts;

namespace ProductCatalog.Api.Services;

public class DummyTokenProvider : ITokenProvider
{
    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
        => Task.FromResult("25a4f06f-8fd5-49b3-a711-c013c156f8c8");
}
