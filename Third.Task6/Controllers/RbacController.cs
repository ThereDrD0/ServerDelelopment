using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Third.Task6.Controllers;

[ApiController]
[Route("")]
public class RbacController : ControllerBase
{
    private const string Secret = "third-task-six-secret";
    private static readonly List<User> Users = new();

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request.Role is not ("admin" or "user" or "guest"))
            return BadRequest(new { detail = "Invalid role" });

        Users.RemoveAll(x => x.Username == request.Username);
        Users.Add(new User(request.Username, request.Password, request.Role));
        return Ok(new { message = "User registered" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = Users.FirstOrDefault(x => x.Username == request.Username && x.Password == request.Password);
        if (user is null)
            return Unauthorized(new { detail = "Invalid credentials" });

        return Ok(new { access_token = Jwt.Create(user.Username, user.Role, Secret), token_type = "bearer" });
    }

    [HttpGet("protected_resource")]
    public IActionResult ProtectedResource()
    {
        var user = CurrentUser();
        if (user is null)
            return Unauthorized(new { detail = "Invalid token" });

        if (user.Role is not ("admin" or "user"))
            return StatusCode(403, new { detail = "Forbidden" });

        return Ok(new { message = "Access granted" });
    }

    [HttpPost("admin/resource")]
    public IActionResult CreateResource()
    {
        return RequireRoles("admin") ?? Ok(new { message = "Resource created" });
    }

    [HttpGet("user/resource")]
    public IActionResult ReadResource()
    {
        return RequireRoles("admin", "user", "guest") ?? Ok(new { message = "Resource read" });
    }

    [HttpPut("user/resource")]
    public IActionResult UpdateResource()
    {
        return RequireRoles("admin", "user") ?? Ok(new { message = "Resource updated" });
    }

    private IActionResult? RequireRoles(params string[] roles)
    {
        var user = CurrentUser();
        if (user is null)
            return Unauthorized(new { detail = "Invalid token" });

        if (!roles.Contains(user.Role))
            return StatusCode(403, new { detail = "Forbidden" });

        return null;
    }

    private User? CurrentUser()
    {
        var header = Request.Headers.Authorization.ToString();
        var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..] : null;
        if (token is null || !Jwt.Validate(token, Secret, out var username, out var role))
            return null;

        return new User(username!, "", role!);
    }
}

public record RegisterRequest(string Username, string Password, string Role);

public record LoginRequest(string Username, string Password);

public record User(string Username, string Password, string Role);

public static class Jwt
{
    public static string Create(string username, string role, string secret)
    {
        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var payload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = username,
            role,
            exp = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
        }));
        return $"{header}.{payload}.{Sign($"{header}.{payload}", secret)}";
    }

    public static bool Validate(string token, string secret, out string? username, out string? role)
    {
        username = null;
        role = null;
        var parts = token.Split('.');
        if (parts.Length != 3 || Sign($"{parts[0]}.{parts[1]}", secret) != parts[2])
            return false;

        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Base64UrlDecode(parts[1]));
        if (payload is null ||
            !payload.TryGetValue("sub", out var sub) ||
            !payload.TryGetValue("role", out var roleValue) ||
            !payload.TryGetValue("exp", out var exp))
            return false;

        if (exp.GetInt64() < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return false;

        username = sub.GetString();
        role = roleValue.GetString();
        return username is not null && role is not null;
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
