using Microsoft.AspNetCore.Mvc;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    public StudentController()
    {
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("StudentController placeholder");
    }
}


