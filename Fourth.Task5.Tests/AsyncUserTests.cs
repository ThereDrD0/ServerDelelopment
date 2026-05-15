using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fourth.Task5;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Fourth.Task5.Tests;

public class AsyncUserTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Faker _faker = new();

    public AsyncUserTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        UserStore.Clear();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        UserStore.Clear();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateUserReturnsCreated()
    {
        var request = NewUser();
        var response = await _client.PostAsJsonAsync("/users", request);
        var user = await response.Content.ReadFromJsonAsync<UserOut>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(request.Username, user!.Username);
        Assert.True(user.Id > 0);
    }

    [Fact]
    public async Task GetExistingUserReturnsOk()
    {
        var created = await CreateUser();
        var response = await _client.GetAsync($"/users/{created.Id}");
        var user = await response.Content.ReadFromJsonAsync<UserOut>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, user!.Id);
    }

    [Fact]
    public async Task GetMissingUserReturnsNotFound()
    {
        var response = await _client.GetAsync("/users/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExistingUserReturnsNoContent()
    {
        var created = await CreateUser();
        var response = await _client.DeleteAsync($"/users/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSameUserTwiceReturnsNotFound()
    {
        var created = await CreateUser();
        await _client.DeleteAsync($"/users/{created.Id}");
        var response = await _client.DeleteAsync($"/users/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<UserOut> CreateUser()
    {
        var response = await _client.PostAsJsonAsync("/users", NewUser());
        return (await response.Content.ReadFromJsonAsync<UserOut>())!;
    }

    private UserIn NewUser()
    {
        return new UserIn(_faker.Internet.UserName(), _faker.Random.Int(18, 90));
    }
}
