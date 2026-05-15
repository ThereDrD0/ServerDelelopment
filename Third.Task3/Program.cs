using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var mode = Environment.GetEnvironmentVariable("MODE") ?? "DEV";

if (mode is not "DEV" and not "PROD")
    throw new InvalidOperationException("MODE must be DEV or PROD");

var app = builder.Build();

app.UseHttpsRedirection();

if (mode == "DEV")
{
    app.MapGet("/docs", (HttpRequest request, HttpResponse response) =>
    {
        if (!CheckDocsAuth(request))
        {
            response.Headers.WWWAuthenticate = "Basic";
            return Results.Unauthorized();
        }

        return Results.Content("<html><body>Docs</body></html>", "text/html");
    }).ExcludeFromDescription();

    app.MapGet("/openapi.json", (HttpRequest request, HttpResponse response) =>
    {
        if (!CheckDocsAuth(request))
        {
            response.Headers.WWWAuthenticate = "Basic";
            return Results.Unauthorized();
        }

        return Results.Json(new { openapi = "3.0.1", info = new { title = "Third.Task3", version = "1.0.0" } });
    }).ExcludeFromDescription();
}

app.MapGet("/", () => new { mode });
app.Run();

static bool CheckDocsAuth(HttpRequest request)
{
    var expectedUser = Environment.GetEnvironmentVariable("DOCS_USER") ?? "docs";
    var expectedPassword = Environment.GetEnvironmentVariable("DOCS_PASSWORD") ?? "password";

    if (!request.Headers.TryGetValue("Authorization", out var header) ||
        !AuthenticationHeaderValue.TryParse(header, out var auth) ||
        auth.Scheme != "Basic" ||
        auth.Parameter is null)
        return false;

    string value;
    try
    {
        value = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Parameter));
    }
    catch (FormatException)
    {
        return false;
    }

    var parts = value.Split(':', 2);
    if (parts.Length != 2)
        return false;

    return FixedEquals(parts[0], expectedUser) && FixedEquals(parts[1], expectedPassword);
}

static bool FixedEquals(string left, string right)
{
    var leftBytes = Encoding.UTF8.GetBytes(left);
    var rightBytes = Encoding.UTF8.GetBytes(right);
    return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
}
