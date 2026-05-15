using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Third.Task2.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private static readonly List<UserInDb> Users = new();

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        Users.RemoveAll(x => FixedEquals(x.Username, user.Username));
        Users.Add(new UserInDb(user.Username, PasswordHasher.Hash(user.Password)));
        return Ok(new { message = "User registered successfully" });
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var user = AuthUser();
        if (user is null)
            return UnauthorizedBasic();

        return Ok(new { message = $"Welcome, {user.Username}!" });
    }

    private UserInDb? AuthUser()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header) ||
            !AuthenticationHeaderValue.TryParse(header, out var auth) ||
            auth.Scheme != "Basic" ||
            auth.Parameter is null)
            return null;

        string value;
        try
        {
            value = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Parameter));
        }
        catch (FormatException)
        {
            return null;
        }

        var parts = value.Split(':', 2);
        if (parts.Length != 2)
            return null;

        var user = Users.FirstOrDefault(x => FixedEquals(x.Username, parts[0]));
        if (user is null || !PasswordHasher.Verify(parts[1], user.HashedPassword))
            return null;

        return user;
    }

    private IActionResult UnauthorizedBasic()
    {
        Response.Headers.WWWAuthenticate = "Basic";
        return Unauthorized();
    }

    private static bool FixedEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}

public class UserBase
{
    [Required]
    public string Username { get; set; } = "";
}

public class User : UserBase
{
    [Required]
    public string Password { get; set; } = "";
}

public class UserInDb : UserBase
{
    public UserInDb(string username, string hashedPassword)
    {
        Username = username;
        HashedPassword = hashedPassword;
    }

    public string HashedPassword { get; set; }
}

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
