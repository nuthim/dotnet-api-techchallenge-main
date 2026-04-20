namespace ProductCatalog.Api.Domain.Contracts;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}
