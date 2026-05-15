using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Third.Task8.Controllers;

[ApiController]
[Route("todos")]
public class TodoController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] TodoCreate request)
    {
        using var connection = Database.GetDbConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO todos (title, description, completed) VALUES ($title, $description, 0); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$title", request.Title);
        command.Parameters.AddWithValue("$description", request.Description);
        var id = Convert.ToInt32(command.ExecuteScalar());

        return StatusCode(201, new Todo(id, request.Title, request.Description, false));
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var todo = FindTodo(id);
        if (todo is null)
            return NotFound(new { detail = "Todo not found" });

        return Ok(todo);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] TodoUpdate request)
    {
        using var connection = Database.GetDbConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE todos
            SET title = $title, description = $description, completed = $completed
            WHERE id = $id
            """;
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$title", request.Title);
        command.Parameters.AddWithValue("$description", request.Description);
        command.Parameters.AddWithValue("$completed", request.Completed ? 1 : 0);

        if (command.ExecuteNonQuery() == 0)
            return NotFound(new { detail = "Todo not found" });

        return Ok(new Todo(id, request.Title, request.Description, request.Completed));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        using var connection = Database.GetDbConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM todos WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        if (command.ExecuteNonQuery() == 0)
            return NotFound(new { detail = "Todo not found" });

        return Ok(new { message = "Todo deleted successfully" });
    }

    private static Todo? FindTodo(int id)
    {
        using var connection = Database.GetDbConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, title, description, completed FROM todos WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        return ReadTodo(reader);
    }

    private static Todo ReadTodo(SqliteDataReader reader)
    {
        return new Todo(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3) == 1
        );
    }
}

public record Todo(int Id, string Title, string Description, bool Completed);

public record TodoCreate(string Title, string Description);

public record TodoUpdate(string Title, string Description, bool Completed);
