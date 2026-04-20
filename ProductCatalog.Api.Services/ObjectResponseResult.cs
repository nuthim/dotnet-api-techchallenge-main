namespace ProductCatalog.Api.Services;

internal readonly struct ObjectResponseResult<T>(T responseObject)
{
    public T Object { get; } = responseObject;
}