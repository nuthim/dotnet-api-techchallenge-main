using Microsoft.AspNetCore.Diagnostics;

namespace ProductCatalog.Api.Middleware;

internal class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var inner = exception.InnerException ?? exception;

        httpContext.Response.StatusCode = inner switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            TimeoutException => StatusCodes.Status503ServiceUnavailable,
            HttpRequestException => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        logger.LogWarning(inner, "Unhandled exception: {Message}", inner.Message);

        await Results.Problem(
            type: inner.GetType().Name,
            title: "Error",
            detail: inner.Message,
            statusCode: httpContext.Response.StatusCode,
            instance: $"{httpContext.Request.Method} {httpContext.Request.Path}"
        ).ExecuteAsync(httpContext);

        return true;
    }
}
