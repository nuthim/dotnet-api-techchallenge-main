using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Services;

public static class ServiceConfiguration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITokenProvider, DummyTokenProvider>();
        // For OAuth2 client_credentials:
        // services.AddHttpClient("AuthServer", c => c.BaseAddress = new Uri("https://auth.example.com/"));
        // services.AddSingleton<ITokenProvider, OAuthTokenProvider>();
        services.AddTransient<TokenDelegatingHandler>();

        // Auto-discover and register all ISortStrategy implementations
        var strategyTypes = typeof(ISortStrategy).Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(ISortStrategy)) && t is { IsInterface: false, IsAbstract: false });

        foreach (var type in strategyTypes)
            services.AddSingleton(typeof(ISortStrategy), type);

        services.AddSingleton<ISortStrategyResolver, SortStrategyResolver>();
        services.AddScoped<IProductService, ProductService>();

        services.AddApiClient<IProductHttpClient, ProductApiClient>(configuration);
        services.AddApiClient<IShopperHistoryHttpClient, ShopperHistoryApiClient>(configuration);
        services.AddApiClient<ITrolleyHttpClient, TrolleyApiClient>(configuration);

        return services;
    }

    private static IServiceCollection AddApiClient<TInterface, TClient>(
        this IServiceCollection services, IConfiguration configuration)
        where TInterface : class, IApiClient
        where TClient : class, TInterface
    {
        var clientName = typeof(TClient).Name;
        var sectionName = clientName.Replace("Client", "");
        var options = configuration.GetSection(sectionName).Get<ClientOptions>()!;

        services.AddHttpClient(clientName, options)
            .AddHttpMessageHandler<TokenDelegatingHandler>();
        services.AddScoped<TInterface, TClient>();

        return services;
    }
}
