using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Second.Task1;

[ApiController]
[Route("")]
public class UserController : ControllerBase
{
    [HttpPost("create_user")]
    public IActionResult CreateUser([FromBody] UserCreate user)
    {
        return Ok(user);
    }
}

public class UserCreate
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int? Age { get; set; }

    [JsonPropertyName("is_subscribed")]
    public bool? IsSubscribed { get; set; }
}
