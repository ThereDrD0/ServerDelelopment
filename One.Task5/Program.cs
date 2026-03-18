using System.Text.Json.Serialization;

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

app.MapPost("/user", (User user) =>
{
    user.IsAdult = user.Age >= 18;
    return user;
});

app.Run();

internal record struct User(string Name, int Age)
{
    [JsonPropertyName("is_adult")] public bool IsAdult { get; set; }
}
