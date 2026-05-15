using Microsoft.AspNetCore.Mvc;

namespace Fourth.Task4.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] UserIn user)
    {
        return StatusCode(201, UserStore.Create(user));
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var user = UserStore.Get(id);
        return user is null ? NotFound(new { detail = "User not found" }) : Ok(user);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return UserStore.Delete(id) ? NoContent() : NotFound(new { detail = "User not found" });
    }
}
