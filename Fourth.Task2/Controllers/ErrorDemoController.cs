using Microsoft.AspNetCore.Mvc;

namespace Fourth.Task2.Controllers;

[ApiController]
[Route("")]
public class ErrorDemoController : ControllerBase
{
    [HttpGet("check")]
    public IActionResult Check([FromQuery] bool ok = false)
    {
        if (!ok)
            throw new CustomExceptionA();

        return Ok(new { message = "Ok" });
    }

    [HttpGet("resources/{id:int}")]
    public IActionResult GetResource(int id)
    {
        if (id != 1)
            throw new CustomExceptionB();

        return Ok(new { id, name = "Resource" });
    }
}
