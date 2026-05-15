using Microsoft.AspNetCore.Mvc;

namespace Second.Task7.Controllers;

[ApiController]
[Route("")]
public class HeadersController : ControllerBase
{
    [HttpGet("headers")]
    public IActionResult Headers()
    {
        var headers = CommonHeaders.From(Request);
        if (headers is null)
            return BadRequest(new { message = "Missing required headers" });

        return Ok(headers.ToResponse());
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        var headers = CommonHeaders.From(Request);
        if (headers is null)
            return BadRequest(new { message = "Missing required headers" });

        Response.Headers["X-Server-Time"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

        return Ok(new
        {
            message = "Добро пожаловать! Ваши заголовки успешно обработаны.",
            headers = headers.ToResponse()
        });
    }
}
