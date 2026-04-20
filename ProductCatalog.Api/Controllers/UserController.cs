using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Domain.User;

namespace ProductCatalog.Api.Controllers;

[Route("user")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet]
    public IActionResult FindUser()
    {
        return Ok(new UserResponseModel
        {
            Name = "John Smith",
            Token = "25a4f06f-8fd5-49b3-a711-c013c156f8c8"
        });
    }
}
