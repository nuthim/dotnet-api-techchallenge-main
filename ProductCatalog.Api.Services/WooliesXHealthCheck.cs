using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProductCatalog.Api.Services;

public class WooliesXHealthCheck(IHttpClientFactory clientFactory) : IHealthCheck
{
    public const string ClientName = "HealthCheck";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = clientFactory.CreateClient(ClientName);
            await client.GetAsync("products", cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
