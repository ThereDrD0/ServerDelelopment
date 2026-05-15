using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Second.Task2;

public record struct Product(
    [property: JsonPropertyName("product_id")]
    [property: Required, Range(1, int.MaxValue)]
    int ProductId,

    [property: Required, Length(2, 50)]
    string Name,

    [property: Required]
    Category Category,

    [property: Required, Range(0.01d, double.MaxValue)]
    double Price
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Category
{
    Electronics,
    Accessories,
    Home,
}
