using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    public ReportController()
    {
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("ReportController placeholder");
    }
}


