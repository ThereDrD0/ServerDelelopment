using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Second.Task4.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private const string SecretKey = "student-secret-key";

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var login = await ReadLoginRequest();
        if (login is null || login.Username != "user123" || login.Password != "password123")
            return Unauthorized(new { message = "Unauthorized" });

        var userId = Guid.NewGuid().ToString();
        var token = $"{userId}.{Sign(userId)}";

        Response.Cookies.Append("session_token", token, new CookieOptions
        {
            HttpOnly = true,
            MaxAge = TimeSpan.FromMinutes(5)
        });

        return Ok(new { message = "Logged in" });
    }

    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var token = Request.Cookies["session_token"];
        if (token is null)
            return Unauthorized(new { message = "Unauthorized" });

        var parts = token.Split('.');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out _) || Sign(parts[0]) != parts[1])
            return Unauthorized(new { message = "Unauthorized" });

        return Ok(new { user_id = parts[0], username = "user123" });
    }

    private static string Sign(string value)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(SecretKey), Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
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
