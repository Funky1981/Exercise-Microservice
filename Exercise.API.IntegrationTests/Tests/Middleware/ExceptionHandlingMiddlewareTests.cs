using System.Text.Json;
using Exercise.API.Middleware;
using Exercise.Application.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Exercise.API.IntegrationTests.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenConcurrencyExceptionThrown_ReturnsConflictProblemDetails()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/workouts/test";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ConcurrencyException("stale write", new Exception("db conflict")),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        document.RootElement.GetProperty("title").GetString().Should().Be("Concurrency conflict.");
        document.RootElement.GetProperty("detail").GetString().Should().Be("stale write");
    }
}
