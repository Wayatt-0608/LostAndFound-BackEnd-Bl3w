using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    public StaffController()
    {
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("StaffController placeholder");
    }
}


