using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.RateLimiting;
using Polly;
using Polly.Timeout;

namespace ProductCatalog.Api.Services;

public static class HttpClientBuilder
{
    public static IHttpClientBuilder AddHttpClient(this IServiceCollection services, string name, ClientOptions options,
        Action<ResiliencePipelineBuilder<HttpResponseMessage>>? configurePipeline = null)
    {
        var builder = services.AddHttpClient(name, client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = options.Timeout * (options.MaxRetries + 1);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            foreach (var header in options.DefaultRequestHeaders)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
        })
            .AddHttpMessageHandler(() =>
            {
                var rateLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions()
                {
                    Window = TimeSpan.FromSeconds(1),
                    SegmentsPerWindow = options.TransactionsPerSecond,
                    PermitLimit = options.TransactionsPerSecond,
                    QueueLimit = 1,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });

                return new RateLimitedDelegationHandler(rateLimiter);
            })
            .SetHandlerLifetime(options.HandlerLifetime);

        builder.AddResilienceHandler($"{name}-pipeline", pipeline =>
        {
            if (configurePipeline is not null)
            {
                configurePipeline(pipeline);
            }
            else
            {
                pipeline
                    .AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = options.MaxRetries,
                        Delay = TimeSpan.FromMilliseconds(200),
                        BackoffType = DelayBackoffType.Exponential,
                        ShouldHandle = args => ValueTask.FromResult(
                            args.Outcome.Result?.StatusCode is HttpStatusCode.RequestTimeout
                                or HttpStatusCode.TooManyRequests
                                or >= HttpStatusCode.InternalServerError
                            || args.Outcome.Exception is HttpRequestException or TimeoutRejectedException)
                    })
                    .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.5,
                        SamplingDuration = TimeSpan.FromSeconds(30),
                        MinimumThroughput = 10,
                        BreakDuration = TimeSpan.FromSeconds(15),
                        ShouldHandle = args => ValueTask.FromResult(
                            args.Outcome.Result?.StatusCode is HttpStatusCode.RequestTimeout
                                or HttpStatusCode.TooManyRequests
                                or >= HttpStatusCode.InternalServerError
                            || args.Outcome.Exception is HttpRequestException or TimeoutRejectedException)
                    })
                    .AddTimeout(options.Timeout);
            }
        });

        return builder;
    }
}