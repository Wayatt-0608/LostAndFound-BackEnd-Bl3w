using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecurityController : ControllerBase
{
    public SecurityController()
    {
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("SecurityController placeholder");
    }
}


