using Microsoft.AspNetCore.Mvc;

namespace Fourth.Task1.Controllers;

[ApiController]
[Route("products")]
public class ProductController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        using var connection = MigrationRunner.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, title, price, count, description FROM Product";

        using var reader = command.ExecuteReader();
        var products = new List<Product>();

        while (reader.Read())
        {
            products.Add(new Product(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2),
                reader.GetInt32(3),
                reader.GetString(4)
            ));
        }

        return Ok(products);
    }
}

public record Product(int Id, string Title, double Price, int Count, string Description);
