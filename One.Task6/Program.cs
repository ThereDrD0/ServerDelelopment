var builder = WebApplication.CreateBuilder(args);

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
    return new Answer($"Thank you for you answer, {feedback.Name}");
});

app.Run();

internal record struct Feedback(string Name, string Message);

internal record struct Answer(string Message);