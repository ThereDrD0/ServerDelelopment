using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Third.Task5.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private const string Secret = "third-task-five-secret";
    private static readonly List<UserInDb> Users = new();
    private static readonly Dictionary<string, List<DateTimeOffset>> Requests = new();

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRequest request)
    {
        if (!RateLimit("register", 1))
            return StatusCode(429, new { detail = "Too many requests" });

        if (Users.Any(x => FixedEquals(x.Username, request.Username)))
            return Conflict(new { detail = "User already exists" });

        Users.Add(new UserInDb(request.Username, PasswordHasher.Hash(request.Password)));
        return StatusCode(201, new { message = "New user created" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserRequest request)
    {
        if (!RateLimit("login", 5))
            return StatusCode(429, new { detail = "Too many requests" });

        var user = Users.FirstOrDefault(x => FixedEquals(x.Username, request.Username));
        if (user is null)
            return NotFound(new { detail = "User not found" });

        if (!PasswordHasher.Verify(request.Password, user.HashedPassword))
            return Unauthorized(new { detail = "Authorization failed" });

        return Ok(new { access_token = Jwt.Create(user.Username, Secret), token_type = "bearer" });
    }

    [HttpGet("protected_resource")]
    public IActionResult ProtectedResource()
    {
        var token = GetBearerToken();
        if (token is null || !Jwt.Validate(token, Secret, out _))
            return Unauthorized(new { detail = "Invalid token" });

        return Ok(new { message = "Access granted" });
    }

    private bool RateLimit(string route, int limit)
    {
        var key = $"{route}:{HttpContext.Connection.RemoteIpAddress}";
        var now = DateTimeOffset.UtcNow;

        if (!Requests.TryGetValue(key, out var requests))
        {
            requests = new List<DateTimeOffset>();
            Requests[key] = requests;
        }

        requests.RemoveAll(x => now - x > TimeSpan.FromMinutes(1));
        if (requests.Count >= limit)
            return false;

        requests.Add(now);
        return true;
    }

    private string? GetBearerToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..] : null;
    }

    private static bool FixedEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}

public record UserRequest(string Username, string Password);

public record UserInDb(string Username, string HashedPassword);

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('.', 2);
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var test = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, test);
    }
}

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
        return $"{header}.{payload}.{Sign($"{header}.{payload}", secret)}";
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
