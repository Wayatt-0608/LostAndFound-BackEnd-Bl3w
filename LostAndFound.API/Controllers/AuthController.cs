using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    public AuthController()
    {
    }

    // Placeholder login endpoint
    [HttpPost("login")]
    public IActionResult Login()
    {
        return Ok("AuthController placeholder");
    }
}


