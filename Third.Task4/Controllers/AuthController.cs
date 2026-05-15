using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Third.Task4.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private const string Secret = "third-task-jwt-secret";

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username != "john_doe" || request.Password != "securepassword123")
            return Unauthorized(new { detail = "Invalid credentials" });

        return Ok(new { access_token = Jwt.Create(request.Username, Secret) });
    }

    [HttpGet("protected_resource")]
    public IActionResult ProtectedResource()
    {
        var token = GetBearerToken();
        if (token is null || !Jwt.Validate(token, Secret, out _))
            return Unauthorized(new { detail = "Invalid token" });

        return Ok(new { message = "Access granted" });
    }

    private string? GetBearerToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..] : null;
    }
}

public record LoginRequest(string Username, string Password);

public static class Jwt
{
    public static string Create(string username, string secret)
    {
        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var payload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = username,
            exp = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
        }));
        var signature = Sign($"{header}.{payload}", secret);
        return $"{header}.{payload}.{signature}";
    }

    public static bool Validate(string token, string secret, out string? username)
    {
        username = null;
        var parts = token.Split('.');
        if (parts.Length != 3 || Sign($"{parts[0]}.{parts[1]}", secret) != parts[2])
            return false;

        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Base64UrlDecode(parts[1]));
        if (payload is null || !payload.TryGetValue("sub", out var sub) || !payload.TryGetValue("exp", out var exp))
            return false;

        if (exp.GetInt64() < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return false;

        username = sub.GetString();
        return username is not null;
    }

    private static string Sign(string value, string secret)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(value));
        return Base64Url(hash);
    }

    private static string Base64Url(byte[] value)
    {
        return Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var text = value.Replace('-', '+').Replace('_', '/');
        text = text.PadRight(text.Length + (4 - text.Length % 4) % 4, '=');
        return Convert.FromBase64String(text);
    }
}
