using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Second.Task2;

public record struct Product(
    [Required, Range(1, int.MaxValue)] 
    int ProductId,

    [Required, Length(2, 50)] 
    string Name,

    [Required] 
    Category Category,

    [Required, Range(0.01d, double.MaxValue)] 
    double Price
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Category
{
    Electronics,
    Accessories,
    Home,
}