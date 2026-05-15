using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Second.Task3.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private static readonly Dictionary<string, string> Sessions = new();

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var login = await ReadLoginRequest();
        if (login is null || login.Username != "user123" || login.Password != "password123")
            return Unauthorized(new { message = "Unauthorized" });

        var token = Guid.NewGuid().ToString();
        Sessions[token] = login.Username;
        Response.Cookies.Append("session_token", token, new CookieOptions { HttpOnly = true });

        return Ok(new { message = "Logged in" });
    }

    [HttpGet("user")]
    public IActionResult GetUser()
    {
        var token = Request.Cookies["session_token"];
        if (token is null || !Sessions.TryGetValue(token, out var username))
            return Unauthorized(new { message = "Unauthorized" });

        return Ok(new { username });
    }

    private async Task<LoginRequest?> ReadLoginRequest()
    {
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync();
            return new LoginRequest(form["username"].ToString(), form["password"].ToString());
        }

        try
        {
            return await Request.ReadFromJsonAsync<LoginRequest>();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public record LoginRequest(string Username, string Password);
