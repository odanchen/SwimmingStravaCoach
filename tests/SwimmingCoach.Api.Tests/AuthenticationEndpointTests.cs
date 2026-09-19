using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SwimmingCoach.Api.Tests;

public class AuthenticationEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthenticationEndpointTests(
        WebApplicationFactory<Program> application)
    {
        _client = application
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    "Server=localhost;Database=test;User=test;Password=test;");
                builder.UseSetting("Authentication:Google:ClientId", "");
                builder.UseSetting("Authentication:Google:ClientSecret", "");
            })
            .CreateClient();
    }

    [Fact]
    public async Task GetCurrentUser_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StartGoogleLogin_WithoutConfiguration_ReturnsServiceUnavailable()
    {
        var response = await _client.GetAsync("/api/auth/google");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_WithoutSession_ReturnsUnauthorized()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            "/api/auth/account");
        request.Headers.Add("X-Requested-With", "SwimmingCoach.Web");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithoutSession_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/profile/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
