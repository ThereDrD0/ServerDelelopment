using Microsoft.AspNetCore.Mvc;

namespace Third.Task7.Controllers;

[ApiController]
[Route("")]
public class UserController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] User request)
    {
        using var connection = Database.GetDbConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO users (username, password) VALUES ($username, $password)";
        command.Parameters.AddWithValue("$username", request.Username);
        command.Parameters.AddWithValue("$password", request.Password);
        command.ExecuteNonQuery();

        return Ok(new { message = "User registered successfully!" });
    }
}

public record User(string Username, string Password);
