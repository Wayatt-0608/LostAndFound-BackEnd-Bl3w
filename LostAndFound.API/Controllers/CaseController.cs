using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaseController : ControllerBase
{
    public CaseController()
    {
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("CaseController placeholder");
    }
}


