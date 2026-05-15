using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Second.Task2.Controllers;

[ApiController]
[Route("")]
public class ProductController : ControllerBase
{
    private static readonly List<Product> Products = new()
    {
        new(123, "Smartphone", Category.Electronics, 599.99d),
        new(456, "Phone Case", Category.Accessories, 19.99d),
        new(789, "Iphone", Category.Electronics, 1299.99d),
        new(101, "Headphones", Category.Accessories, 99.99d),
        new(202, "Smartwatch", Category.Electronics, 299.99d)
    };

    [HttpGet("product/{product_id:int}")]
    public IActionResult GetProductById(int product_id)
    {
        var product = Products.FirstOrDefault(p => p.ProductId == product_id);
        if (product == default)
            return NotFound(new { message = $"Product with id {product_id} not found" });

        return Ok(product);
    }

    [HttpGet("products/search")]
    public IActionResult SearchProducts(
        [FromQuery, Required] string keyword,
        [FromQuery] Category? category,
        [FromQuery, Range(1, 100)] int limit = 10
    )
    {
        var results = Products.AsQueryable();
        results = results.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (category.HasValue)
            results = results.Where(p => p.Category == category.Value);

        return Ok(results.Take(limit).ToList());
    }
}
