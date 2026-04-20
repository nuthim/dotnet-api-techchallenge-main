namespace ProductCatalog.Api.Domain.Contracts;

public record class ApiResponse<T>
{
    public T? Content { get; init; }

    public Dictionary<string, IEnumerable<string>>? Headers { get; init; }

    public required bool IsSuccess { get; init; }

    public required int StatusCode { get; init; }

    public ApiError? Problem { get; init; }
}