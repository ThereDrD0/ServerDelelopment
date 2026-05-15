using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Second.Task5.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private const string SecretKey = "student-secret-key";
    private const int SessionSeconds = 300;
    private const int RefreshAfterSeconds = 180;

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var login = await ReadLoginRequest();
        if (login is null || login.Username != "user123" || login.Password != "password123")
            return Unauthorized(new { message = "Unauthorized" });

        var userId = Guid.NewGuid().ToString();
        SetSessionCookie(userId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        return Ok(new { message = "Logged in" });
    }

    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var token = Request.Cookies["session_token"];
        if (token is null)
            return Unauthorized(new { message = "Session expired" });

        var parts = token.Split('.');
        if (parts.Length != 3 || !Guid.TryParse(parts[0], out _) || !long.TryParse(parts[1], out var timestamp))
            return Unauthorized(new { message = "Invalid session" });

        var signedValue = $"{parts[0]}.{parts[1]}";
        if (Sign(signedValue) != parts[2])
            return Unauthorized(new { message = "Invalid session" });

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var elapsed = now - timestamp;

        if (elapsed < 0)
            return Unauthorized(new { message = "Invalid session" });

        if (elapsed >= SessionSeconds)
            return Unauthorized(new { message = "Session expired" });

        if (elapsed >= RefreshAfterSeconds)
            SetSessionCookie(parts[0], now);

        return Ok(new { user_id = parts[0], username = "user123" });
    }

    private void SetSessionCookie(string userId, long timestamp)
    {
        var value = $"{userId}.{timestamp}";
        var token = $"{value}.{Sign(value)}";

        Response.Cookies.Append("session_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            MaxAge = TimeSpan.FromSeconds(SessionSeconds)
        });
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
