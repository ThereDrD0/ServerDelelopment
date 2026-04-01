using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Second.Task1;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{   
    [HttpPost("create_user")]
    public IActionResult CreateUser([FromBody] User user)
    {
        return Ok(user);
    }
}

public record User(
    [Required] string Name,
    [Required, EmailAddress] string Email,
    [Range(1, 100)] int? Age = null,
    bool? IsSubscribed = null
);