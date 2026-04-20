using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Mime;
using System.Net;
using Microsoft.Extensions.Logging;
using ProductCatalog.Api.Domain.Contracts;

namespace ProductCatalog.Api.Services;

public abstract class ApiClient : IApiClient
{
    protected readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _apiname;

    protected Lazy<JsonSerializerOptions> JsonSerializerOptions { get; }

    public ApiClient(IHttpClientFactory clientFactory, ILogger logger, string apiname)
    {
        _httpClient = clientFactory.CreateClient(apiname);
        _logger = logger;
        _apiname = apiname;
        JsonSerializerOptions = new(CreateSerializerSettings);
    }

    protected virtual JsonSerializerOptions CreateSerializerSettings()
    {
        var settings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowOutOfOrderMetadataProperties = true
        };

        settings.Converters.Add(new JsonStringEnumConverter());

        return settings;
    }

    protected async Task<ApiResponse<T>> SendAsync<T>(string url, object? body, HttpMethod httpMethod, CancellationToken cancellationToken)
    {
        using var request = BuildHttpRequestMessage(url, httpMethod, body);

        HttpResponseMessage? response = null;
        Dictionary<string, IEnumerable<string>>? headers = null;
        try
        {
            _logger.LogDebug("{Method} {Url}", httpMethod, url);

            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var status = (int)response.StatusCode;
            headers = GetResponseHeaders(response);

            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(NoContent))
                    return new() { StatusCode = status, IsSuccess = true, Headers = headers };

                var objectResponse = await ReadObjectResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);
                return new() { StatusCode = status, IsSuccess = true, Content = objectResponse.Object, Headers = headers };
            }
            else
            {
                _logger.LogWarning("{Method} {Url} returned {StatusCode}", httpMethod, url, status);

                try
                {
                    var objectResponse = await ReadObjectResponseAsync<ApiError>(response, cancellationToken).ConfigureAwait(false);
                    var problem = objectResponse.Object ?? CreateError("Error", response.ReasonPhrase, status, $"{status} - {response.ReasonPhrase}", $"{httpMethod} {url}");
                    return new() { StatusCode = status, IsSuccess = false, Problem = problem, Headers = headers };
                }
                catch
                {
                    return new() { StatusCode = status, IsSuccess = false, Problem = CreateError("Error", response.ReasonPhrase, status, $"{status} - {response.ReasonPhrase}", $"{httpMethod} {url}"), Headers = headers };
                }
            }
        }
        catch (Exception ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timed out: {Method} {Url}", httpMethod, url);
            return CreateErrorResponse<T>(httpMethod, url, "Timeout", "Request Timeout", (int)HttpStatusCode.ServiceUnavailable, "The request timed out");
        }
        catch (Exception ex) when (ex is TaskCanceledException || (ex is HttpRequestException && (ex.Message.Contains("actively refused") || ex.Message.Contains("error occurred while sending the request"))))
        {
            var isRefused = ex.Message.Contains("actively refused") || ex.Message.Contains("error occurred while sending the request");
            _logger.LogError(ex, "Request failed: {Method} {Url}", httpMethod, url);
            return CreateErrorResponse<T>(httpMethod, url,
                isRefused ? "Unavailable" : "Cancelled",
                isRefused ? "Service Unavailable" : "Request Cancelled",
                (int)HttpStatusCode.ServiceUnavailable,
                isRefused ? $"{ex.Message}. Ensure {_apiname} is up" : "The request was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error: {Method} {Url}", httpMethod, url);
            var statusCode = (int)(response?.StatusCode ?? HttpStatusCode.InternalServerError);
            return CreateErrorResponse<T>(httpMethod, url, ex.GetType().Name, "Error", statusCode, $"{statusCode} - {ex.Message}", headers);
        }
        finally
        {
            response?.Dispose();
        }
    }

    private static ApiError CreateError(string? type, string? title, int status, string? detail, string? instance) =>
        new() { Type = type, Title = title, Status = status, Detail = detail, Instance = instance };

    private static ApiResponse<T> CreateErrorResponse<T>(HttpMethod method, string url, string type, string title, int status, string detail, Dictionary<string, IEnumerable<string>>? headers = null) =>
        new()
        {
            StatusCode = status,
            IsSuccess = false,
            Problem = CreateError(type, title, status, detail, $"{method} {url}"),
            Headers = headers
        };

    private async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response == null || response.Content == null)
            return new(default!);

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var typedBody = responseText == string.Empty ? default : JsonSerializer.Deserialize<T>(responseText, JsonSerializerOptions.Value);
        return new(typedBody!);
    }

    private HttpRequestMessage BuildHttpRequestMessage(string url, HttpMethod httpMethod, object? body = null)
    {
        var request = new HttpRequestMessage(httpMethod, new Uri(url, UriKind.RelativeOrAbsolute));
        if (body is null)
            return request;

        switch (body)
        {
            case HttpContent content:
                request.Content = content;
                return request;
            case string str:
                request.Content = new StringContent(str);
                request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);
                break;
            default:
                {
                    var json = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerOptions.Value);
                    var bodyContent = new ByteArrayContent(json);
                    bodyContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);
                    request.Content = bodyContent;
                    break;
                }
        }

        return request;
    }

    private static Dictionary<string, IEnumerable<string>> GetResponseHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, IEnumerable<string>>();
        foreach (var item in response.Headers)
            headers[item.Key] = item.Value;
        if (response.Content != null && response.Content.Headers != null)
        {
            foreach (var item in response.Content.Headers)
                headers[item.Key] = item.Value;
        }
        return headers;
    }
}