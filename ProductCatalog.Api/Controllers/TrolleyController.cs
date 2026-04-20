using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Trolley;
using ProductCatalog.Api.Extensions;

namespace ProductCatalog.Api.Controllers;

[Route("trolley")]
[ApiController]
public class TrolleyController(ITrolleyHttpClient trolleyClient) : ControllerBase
{
    [HttpPost("total")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Calculate([FromBody] TrolleyRequest request, CancellationToken cancellationToken)
    {
        var response = await trolleyClient.CalculateTotal(request, cancellationToken);
        return response.ToActionResult();
    }
}
