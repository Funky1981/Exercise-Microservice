using System.Net;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.Middleware;

/// <summary>
/// Integration tests for CorrelationIdMiddleware.
/// Verifies that X-Correlation-Id is always present in responses, using an
/// existing value when supplied or generating a new GUID otherwise.
/// </summary>
[Collection("Integration")]
public class CorrelationIdMiddlewareTests : IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly AuthBypassWebApplicationFactory _factory;

    public CorrelationIdMiddlewareTests(AuthBypassWebApplicationFactory factory)
        => _factory = factory;

    [Fact]
    public async Task Request_WithoutCorrelationIdHeader_ResponseContainsNewGuid()
    {
        var client   = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        response.Headers.TryGetValues("X-Correlation-Id", out var values).Should().BeTrue();
        var correlationId = values!.First();
        Guid.TryParse(correlationId, out _).Should().BeTrue("the generated correlation ID should be a valid GUID");
    }

    [Fact]
    public async Task Request_WithCorrelationIdHeader_ResponseEchosSameValue()
    {
        var client = _factory.CreateClient();
        var supplied = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", supplied);

        var response = await client.SendAsync(request);

        response.Headers.TryGetValues("X-Correlation-Id", out var values).Should().BeTrue();
        values!.First().Should().Be(supplied);
    }
}
