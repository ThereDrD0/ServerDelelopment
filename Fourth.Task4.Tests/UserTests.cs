using System.Net;
using System.Net.Http.Json;
using Fourth.Task4;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Fourth.Task4.Tests;

public class UserTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserTests(WebApplicationFactory<Program> factory)
    {
        UserStore.Clear();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUserReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/users", new UserIn("alex", 20));
        var user = await response.Content.ReadFromJsonAsync<UserOut>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("alex", user!.Username);
        Assert.Equal(1, user.Id);
    }

    [Fact]
    public async Task GetUserReturnsExistingUser()
    {
        var created = await (await _client.PostAsJsonAsync("/users", new UserIn("max", 30))).Content.ReadFromJsonAsync<UserOut>();
        var response = await _client.GetAsync($"/users/{created!.Id}");
        var user = await response.Content.ReadFromJsonAsync<UserOut>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, user!.Id);
    }

    [Fact]
    public async Task MissingUserReturnsNotFound()
    {
        var response = await _client.GetAsync("/users/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUserReturnsNoContent()
    {
        var created = await (await _client.PostAsJsonAsync("/users", new UserIn("kate", 25))).Content.ReadFromJsonAsync<UserOut>();
        var response = await _client.DeleteAsync($"/users/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
