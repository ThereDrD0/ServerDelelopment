using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Fourth.Task3.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] User request)
    {
        return Ok(request);
    }
}

public class User
{
    [Required]
    public string Username { get; set; } = "";

    [Range(19, int.MaxValue)]
    public int Age { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(16, MinimumLength = 8)]
    public string Password { get; set; } = "";

    public string Phone { get; set; } = "Unknown";
}
