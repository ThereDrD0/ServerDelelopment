using Fourth.Task2;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var response = exception switch
        {
            CustomExceptionA ex => new ErrorResponse(ex.StatusCode, ex.Message),
            CustomExceptionB ex => new ErrorResponse(ex.StatusCode, ex.Message),
            _ => new ErrorResponse(500, "Internal server error")
        };

        Console.WriteLine(exception?.Message);
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
