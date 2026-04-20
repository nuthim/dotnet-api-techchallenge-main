using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Domain.Contracts;
using ProductCatalog.Api.Domain.Product;

namespace ProductCatalog.Api.Controllers;

[Route("products")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery(Name = "sortBy"), EnumDataType(typeof(SortOption))] SortOption sortOption,
        CancellationToken cancellationToken)
        => Ok(await productService.GetSortedProducts(sortOption, cancellationToken));
}
