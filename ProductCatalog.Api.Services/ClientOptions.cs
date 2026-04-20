using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.Services;

public class ClientOptions
{
    [Required]
    public string BaseUrl { get; set; } = null!;

    [Required]
    public TimeSpan Timeout { get; set; }

    public int MaxRetries { get; set; } = 3;

    public int TransactionsPerSecond { get; set; } = 10;

    public TimeSpan HandlerLifetime { get; set; } = TimeSpan.FromMinutes(15);

    public Dictionary<string, string> DefaultRequestHeaders { get; set; } = [];
}