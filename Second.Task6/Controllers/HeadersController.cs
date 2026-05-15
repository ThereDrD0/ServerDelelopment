using Microsoft.AspNetCore.Mvc;

namespace Second.Task6.Controllers;

[ApiController]
[Route("")]
public class HeadersController : ControllerBase
{
    [HttpGet("headers")]
    public IActionResult Headers()
    {
        if (!Request.Headers.TryGetValue("User-Agent", out var userAgent) ||
            !Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
            return BadRequest(new { message = "Missing required headers" });

        return Ok(new Dictionary<string, string>
        {
            ["User-Agent"] = userAgent.ToString(),
            ["Accept-Language"] = acceptLanguage.ToString()
        });
    }
}
