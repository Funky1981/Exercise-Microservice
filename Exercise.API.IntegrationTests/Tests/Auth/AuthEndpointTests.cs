using System.Net;
using System.Net.Http.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using Exercise.Application.Features.Auth.Dtos;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.Auth;

/// <summary>
/// Integration tests for auth endpoints - register, login, and refresh token.
/// Uses the real JWT factory so tokens are signed and validated end-to-end.
/// </summary>
[Collection("Integration")]
public class AuthEndpointTests : IClassFixture<ExerciseWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(ExerciseWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<HttpResponseMessage> RegisterAsync(string email, string password = "Test1!Pass")
        => await _client.PostAsJsonAsync("/api/users/register",
            new { name = "Test User", email, password });

    private async Task<LoginResponse?> LoginAsync(string email, string password = "Test1!Pass")
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        return resp.IsSuccessStatusCode
            ? await resp.Content.ReadFromJsonAsync<LoginResponse>()
            : null;
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/users/register", new
        {
            name     = "Alice Smith",
            email    = $"alice{Guid.NewGuid():N}@test.com",
            password = "Test1!Pass"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/users/register", new
        {
            name     = "Bad Email",
            email    = "not-an-email",
            password = "Test1!Pass"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/users/register", new
        {
            name     = "Weak Pass",
            email    = $"weak{Guid.NewGuid():N}@test.com",
            password = "short"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtAndRefreshToken()
    {
        var email = $"login{Guid.NewGuid():N}@test.com";
        await RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Test1!Pass" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"badpass{Guid.NewGuid():N}@test.com";
        await RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPass1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "nobody@unknown.com", password = "Test1!Pass" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokenPair()
    {
        var email = $"refresh{Guid.NewGuid():N}@test.com";
        await RegisterAsync(email);
        var login = await LoginAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { email, refreshToken = login!.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        var email = $"badref{Guid.NewGuid():N}@test.com";
        await RegisterAsync(email);
        await LoginAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { email, refreshToken = "completely-invalid-token-value" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
