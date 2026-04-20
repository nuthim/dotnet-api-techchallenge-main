using System.Text.Json.Serialization;
using Microsoft.AspNetCore.RateLimiting;
using ProductCatalog.Api.Middleware;
using ProductCatalog.Api.Services;

namespace ProductCatalog.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();

        // Defence in depth — per-instance rate limiting.
        // Global limits should be enforced by the API gateway / service mesh sidecar.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("default", config =>
            {
                config.PermitLimit = 100;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueLimit = 0;
            });
        });

        return services;
    }

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ProductApi:BaseUrl"]!;
        services.AddHttpClient(WooliesXHealthCheck.ClientName, c => c.BaseAddress = new Uri(baseUrl));
        services.AddHealthChecks()
            .AddCheck<WooliesXHealthCheck>("wooliesx-api", tags: ["ready"]);

        return services;
    }
}
