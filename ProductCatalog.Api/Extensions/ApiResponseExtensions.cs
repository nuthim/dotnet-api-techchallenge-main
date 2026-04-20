using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Domain.Contracts;

namespace ProductCatalog.Api.Extensions;

public static class ApiResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response)
        => response.IsSuccess
            ? new OkObjectResult(response.Content)
            : new ObjectResult(response.Problem) { StatusCode = response.StatusCode };
}
