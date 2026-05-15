using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Third.Task1.Controllers;

[ApiController]
[Route("")]
public class LoginController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header) ||
            !AuthenticationHeaderValue.TryParse(header, out var auth) ||
            auth.Scheme != "Basic" ||
            auth.Parameter is null)
            return UnauthorizedBasic();

        var value = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Parameter));
        var parts = value.Split(':', 2);

        if (parts.Length != 2 || parts[0] != "user" || parts[1] != "password")
            return UnauthorizedBasic();

        return Ok("You got my secret, welcome");
    }

    private IActionResult UnauthorizedBasic()
    {
        Response.Headers.WWWAuthenticate = "Basic";
        return Unauthorized();
    }
}
