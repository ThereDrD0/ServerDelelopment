using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var feedbacks = new Dictionary<string, string>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/feedback", (Feedback feedback) =>
{
    if (feedback.Name.Length is < 2 or > 50)
        return Results.UnprocessableEntity(new { detail = "Имя должно быть от 2 до 50 символов" });
    
    if (feedback.Message.Length is < 10 or > 500)
        return Results.UnprocessableEntity(new { detail = "Сообщение должно быть от 10 до 500 символов" });

    if (Regex.IsMatch(feedback.Message, @"(?i)(кринж|рофл|вайб)"))
    {
        return Results.UnprocessableEntity(new
        {
            detail = new[] 
            {
                new 
                {
                    type = "value_error",
                    msg = "Value error, Использование недопустимых слов",
                    input = feedback.Message,
                }
            }
        });
    }
    
    feedbacks[feedback.Name] =  feedback.Message;
    return Results.Ok($"Спасибо, {feedback.Name}! Ваш отзыв сохранён.");
});

app.Run();

internal record struct Feedback(string Name, string Message);
